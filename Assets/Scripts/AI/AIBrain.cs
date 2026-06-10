using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIBrain : MonoBehaviour
{
    // ── 컴포넌트 ──────────────────────────────────────────────
    private Character character;
    protected NPCCommander command;
    private SkillExecuter executer;
    private CharacterStat stat;
    private SkillCaster caster;
    private NPCMovement movement;
    private StateManager state;
    private NPCCamera cam;
    protected NPCSight sight;
    protected NavMeshAgent agent;

    // ── 인스펙터 ──────────────────────────────────────────────
    [SerializeField] private float safeDistance;
    [SerializeField] private float maxChaseTime = 3f;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private List<Node> patrolNodes;

    [Header("어그로 전환 (0이면 비활성)")]
    [Tooltip("같은 적에게 이 횟수 이상 넘어지면(녹다운) 그 적으로 어그로를 바꾼다.")]
    [SerializeField] private int switchAggroOnKnockedDownCount = 0;
    [Tooltip("현재 어그로 대상을 이 횟수만큼 넘어뜨리면 시야 내 다른 적으로 어그로를 바꾼다.")]
    [SerializeField] private int switchAggroOnKnockdownDealtCount = 0;

    // ── 상태 ──────────────────────────────────────────────────
    // Follow는 아군(AllyBrain) 전용 상태다. 기반 AIBrain(적)은 진입하지 않는다.
    protected enum AIState { Patrol, Follow, Combat, Chase, DestroyRope }
    protected AIState currentState;

    // ── 런타임 변수 ───────────────────────────────────────────
    protected Character aggro;
    private Coroutine chaseCoroutine;
    private Coroutine comboCoroutine;
    private Coroutine destroyRopeCoroutine;
    private int patrolIndex;
    private bool aiActive;   // 사망 시 false → Update(상태머신) 정지. 활성화 시 true.

    // ── 어그로 전환 카운트 ────────────────────────────────────
    private Character lastAttacker;                                       // 가장 최근에 나를 때린 적(넘어뜨린 범인 추정)
    private readonly Dictionary<Character, int> knockedDownByCount = new(); // 적별 '나를 넘어뜨린' 누적 횟수
    private int knockdownDealtCount;                                      // 현재 aggro를 넘어뜨린 횟수
    private bool aggroKnockdownLatch;                                     // aggro 녹다운 1회를 중복 카운트하지 않도록

    // ── 초기화 ────────────────────────────────────────────────
    private void Awake()
    {
        character = GetComponent<Character>();
        command   = GetComponent<NPCCommander>();
        executer  = GetComponent<SkillExecuter>();
        stat      = GetComponent<CharacterStat>();
        caster    = GetComponent<SkillCaster>();
        state     = GetComponent<StateManager>();
        cam       = GetComponent<NPCCamera>();
        sight     = GetComponentInChildren<NPCSight>();
        agent     = GetComponent<NavMeshAgent>();

        stat.onDamageTake  += ChangeAggro;
        state.onAirborne   += OnCancelled;
        state.onDead       += OnCancelled;
        state.onDead       += OnDead;
        state.onHitstun    += OnCancelled;
        state.onGrab       += OnCancelled;
        state.onGroggy     += OnCancelled;
        state.onKnockdown  += OnCancelled;
        state.onKnockdown  += OnKnockedDown;
    }

    private void OnEnable()
    {
        // 풀 재사용 대비: 비활성화로 코루틴 자체는 멈췄지만 참조가 stale(non-null)로 남는다.
        // 특히 comboCoroutine이 non-null로 남으면 SkillCommand가 영영 공격을 시작하지 않는다.
        comboCoroutine = null;
        chaseCoroutine = null;
        destroyRopeCoroutine = null;
        aiActive = true;   // 리스폰 시 AI 재가동

        // 어그로 전환 카운트 초기화(풀 재사용 대비)
        lastAttacker = null;
        knockedDownByCount.Clear();
        knockdownDealtCount = 0;
        aggroKnockdownLatch = false;

        agent.updatePosition = false;
        agent.updateRotation = true;

        if (NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas))
            agent.Warp(hit.position);

        // 생성 직후(프리팹 위치 = NavMesh 밖)엔 아직 위치/순찰이 세팅되기 전이므로 패트롤 시작을 보류한다.
        // 스포너가 위치를 잡고 다시 SetActive 하면 Warp 성공 → 여기서 정상적으로 Patrol 시작.
        if (!agent.isOnNavMesh) return;

        ChangeState(IdleState());
    }

    // 스포너가 활성화 '전에' 순찰 경로를 주입한다.
    // (C안: 이후 SetActive 시 OnEnable이 이 경로로 Warp + Patrol을 시작)
    public void SetPatrol(List<Node> patrol)
    {
        patrolNodes = patrol;
        aggro = null;
    }

    // 어그로가 없을 때의 기본 상태(적은 순찰). 아군(AllyBrain)은 Follow로 오버라이드한다.
    protected virtual AIState IdleState() => AIState.Patrol;

    // 아군 ChangeState(Follow) 등에서 추격 코루틴을 정리할 때 사용.
    protected void StopChaseCoroutine()
    {
        if (chaseCoroutine != null) { StopCoroutine(chaseCoroutine); chaseCoroutine = null; }
    }

    // ── 메인 루프 ─────────────────────────────────────────────
    private void Update()
    {
        if (!aiActive) return;   // 사망 시 정지

        agent.nextPosition = transform.position;
        TrackAggroKnockdownDealt();
        CheckTransition();
        ExecuteState();
    }

    // ── 상태 전이 체크 ────────────────────────────────────────
    protected virtual void CheckTransition()
    {
        switch (currentState)
        {
            case AIState.Patrol:
                if (aggro == null) TryAcquireFromSight();
                if (aggro != null) ChangeState(AIState.Combat);
                break;

            case AIState.Combat:
                if (aggro == null)
                    ChangeState(IdleState());
                else if (FindVisitedAnchorNearby() is Anchor anchor)
                    ChangeState(AIState.DestroyRope, anchor);
                break;

            case AIState.Chase:
                if (FindVisitedAnchorNearby() is Anchor chaseAnchor)
                    ChangeState(AIState.DestroyRope, chaseAnchor);
                break;

            case AIState.DestroyRope:
                // 코루틴이 직접 상태 전환 처리
                break;
        }
    }

    // ── 상태 실행 ─────────────────────────────────────────────
    protected virtual void ExecuteState()
    {
        switch (currentState)
        {
            case AIState.Patrol:     ExecutePatrol();     break;
            case AIState.Combat:     ExecuteCombat();     break;
            case AIState.Chase:      ExecuteChase();      break;
            case AIState.DestroyRope:                     break;
        }
    }

    private void ExecutePatrol()
    {
        if (patrolNodes.Count == 0) { command.SetMoveInput(Vector2.zero); return; }

        ApplyNavMeshInput();

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            patrolIndex = (patrolIndex + 1) % patrolNodes.Count;
            agent.SetDestination(patrolNodes[patrolIndex].transform.position);
        }
    }

    private void ExecuteCombat()
    {
        cam.RotateTo(aggro.transform);

        var t = transform.position; t.y = 0;
        var a = aggro.transform.position; a.y = 0;
        var sqrDist = (t - a).sqrMagnitude;

        if (sqrDist < (safeDistance - 0.5f) * (safeDistance - 0.5f))
            command.SetMoveInput(new Vector2(0, -1));
        else if (sqrDist > (safeDistance + 0.5f) * (safeDistance + 0.5f))
            command.SetMoveInput(new Vector2(0, 1));
        else
            command.SetMoveInput(Vector2.zero);

        SkillCommand();
    }

    private void ExecuteChase()
    {
        agent.SetDestination(aggro.transform.position);
        ApplyNavMeshInput();
    }

    // ── 상태 전환 ─────────────────────────────────────────────
    protected virtual void ChangeState(AIState next, Anchor anchor = null)
    {
        currentState = next;

        switch (next)
        {
            case AIState.Patrol:
                aggro = null;
                command.SetMoveInput(Vector2.zero);
                agent.isStopped = false;
                if (patrolNodes.Count == 0) break;
                patrolIndex = patrolNodes
                    .Select((n, i) => (n, i))
                    .OrderBy(x => (x.n.transform.position - transform.position).sqrMagnitude)
                    .First().i;
                agent.SetDestination(patrolNodes[patrolIndex].transform.position);
                break;

            case AIState.Combat:
                agent.isStopped = true;
                command.SetMoveInput(Vector2.zero);
                if (chaseCoroutine != null) { StopCoroutine(chaseCoroutine); chaseCoroutine = null; }
                break;

            case AIState.Chase:
                agent.isStopped = false;
                if (chaseCoroutine != null) StopCoroutine(chaseCoroutine);
                chaseCoroutine = StartCoroutine(ChaseTimeout());
                break;

            case AIState.DestroyRope:
                agent.isStopped = false;
                if (destroyRopeCoroutine != null) StopCoroutine(destroyRopeCoroutine);
                destroyRopeCoroutine = StartCoroutine(CoDestroyRope(anchor));
                break;
        }
    }

    // ── 코루틴 ────────────────────────────────────────────────
    private IEnumerator ChaseTimeout()
    {
        yield return new WaitForSeconds(maxChaseTime);
        chaseCoroutine = null;
        ChangeState(IdleState());
    }

    private IEnumerator CoDestroyRope(Anchor anchor)
    {
        agent.SetDestination(anchor.transform.position);

        yield return new WaitUntil(() =>
            anchor == null ||
            (!agent.pathPending && agent.remainingDistance <= interactionDistance));

        if (anchor == null)
        {
            destroyRopeCoroutine = null;
            ChangeState(aggro != null ? AIState.Chase : IdleState());
            yield break;
        }

        agent.isStopped = true;
        command.PressInput(ConditionInput.Q);
        yield return new WaitForSeconds(0.5f);
        command.ReleaseInput(ConditionInput.Q);

        destroyRopeCoroutine = null;
        ChangeState(aggro != null ? AIState.Chase : IdleState());
    }

    private IEnumerator CoCombo(Combo combo)
    {
        foreach (var c in combo.comboInput)
        {
            if (c.isPress) command.PressInput(c.input);
            else           command.ReleaseInput(c.input);

            if (c.condtion.IsMet(character, aggro))
                yield return new WaitForSecondsUnfrozen(c.preDelay, state);
            else
            {
                comboCoroutine = null;
                yield break;
            }
        }
        comboCoroutine = null;
    }

    // ── 유틸 ──────────────────────────────────────────────────
    protected void ApplyNavMeshInput()
    {
        if (agent.desiredVelocity.sqrMagnitude < 0.01f)
        {
            command.SetMoveInput(Vector2.zero);
            return;
        }
        var localDir = transform.InverseTransformDirection(agent.desiredVelocity.normalized);
        command.SetMoveInput(new Vector2(localDir.x, localDir.z));
    }

    private void SkillCommand()
    {
        if (comboCoroutine != null) return;
        var w = executer.CurrentWeapon;
        foreach (var c in w.combo)
        {
            if (c.conditions.All(cond => cond.IsMet(character, aggro)))
            {
                comboCoroutine = StartCoroutine(CoCombo(c));
                return;
            }
        }

        // [임시 진단] 아무 콤보도 발동 못 함 → 각 콤보를 처음 막은 조건을 출력. 원인 파악 후 이 블록 삭제.
        foreach (var c in w.combo)
            foreach (var cond in c.conditions)
                if (!cond.IsMet(character, aggro))
                {
                    Debug.Log($"[AI진단] {name}: 콤보 차단 = {cond.GetType().Name}, aggro={(aggro != null ? aggro.name : "null")}");
                    break;
                }
        return;
    }

    protected Anchor FindVisitedAnchorNearby()
    {
        if (aggro == null) return null;
        if (aggro.State.State != CharacterState.Climb) return null;

        var hits = Physics.OverlapSphere(aggro.transform.position, interactionDistance * 5f);
        foreach (var hit in hits)
        {
            var anchor = hit.GetComponent<Anchor>();
            if (anchor != null) return anchor;
        }
        return null;
    }

    // ── 이벤트 ────────────────────────────────────────────────
    // 오브젝트(파괴물)는 어그로 대상이 될 수 없다.
    protected static bool IsObstacle(Character c) => c != null && c.Stat != null && c.Stat.IsObstacle;

    public void OnDetected(Character target)
    {
        if (aggro != null || IsObstacle(target)) return;   // 교전 중이거나 오브젝트면 무시(sticky)
        SetAggro(target);
        ChangeState(AIState.Combat);
    }

    public void OnLost(Character target)
    {
        if (aggro == target) ChangeState(AIState.Chase);
    }

    private void ChangeAggro(Character target, AttackInfo info, AttackId id)
    {
        if (IsObstacle(target)) return;   // 오브젝트는 어그로/보복 대상 아님
        lastAttacker = target;   // 넘어뜨린 범인 추적용(녹다운 카운트 ①에 사용)
        if (aggro != null) return;   // 이미 교전 대상이 있으면 피격만으로 전환하지 않음(전환은 녹다운 카운트로만)
        SetAggro(target);
        if (currentState != AIState.Combat) ChangeState(AIState.Combat);
    }

    // 유휴(Patrol/Follow) 상태에서 시야 내 가장 가까운 살아있는 적을 어그로로 잡는다.
    protected void TryAcquireFromSight()
    {
        if (sight == null) return;

        Character best = null;
        float bestSqr = float.MaxValue;
        foreach (var c in sight.Visibles)
        {
            if (c == null || c.State.State == CharacterState.Dead || IsObstacle(c)) continue;
            float sqr = (c.transform.position - transform.position).sqrMagnitude;
            if (sqr < bestSqr) { bestSqr = sqr; best = c; }
        }
        if (best != null) SetAggro(best);
    }

    // 어그로를 새 대상으로 지정. 대상이 바뀌면 '넘어뜨린 횟수' 카운트를 초기화한다.
    protected void SetAggro(Character target)
    {
        if (aggro == target) return;
        aggro = target;
        knockdownDealtCount = 0;
        aggroKnockdownLatch = false;
    }

    // [넘어짐 카운트] 내가 녹다운될 때: 범인(lastAttacker)에게 N회 이상 넘어졌으면 그 적으로 어그로 전환.
    private void OnKnockedDown()
    {
        if (switchAggroOnKnockedDownCount <= 0 || lastAttacker == null) return;

        knockedDownByCount.TryGetValue(lastAttacker, out int count);
        count++;
        knockedDownByCount[lastAttacker] = count;

        if (count >= switchAggroOnKnockedDownCount && aggro != lastAttacker)
        {
            SetAggro(lastAttacker);
            if (currentState != AIState.Combat) ChangeState(AIState.Combat);
        }
    }

    // [넘어뜨림 카운트] 현재 aggro가 녹다운에 들어가는 순간을 세어, N회면 시야 내 다른 적으로 전환.
    private void TrackAggroKnockdownDealt()
    {
        if (switchAggroOnKnockdownDealtCount <= 0 || aggro == null) { aggroKnockdownLatch = false; return; }

        bool isDown = aggro.State.State == CharacterState.Knockdown;
        if (isDown && !aggroKnockdownLatch)
        {
            aggroKnockdownLatch = true;
            knockdownDealtCount++;
            if (knockdownDealtCount >= switchAggroOnKnockdownDealtCount)
                SwitchToAnotherEnemy();
        }
        else if (!isDown)
        {
            aggroKnockdownLatch = false;
        }
    }

    // 시야 내에서 현재 aggro가 아닌 살아있는 다른 적을 골라 어그로를 옮긴다(없으면 유지).
    private void SwitchToAnotherEnemy()
    {
        foreach (var c in sight.Visibles)
        {
            if (c == null || c == aggro) continue;
            if (c.State.State == CharacterState.Dead || IsObstacle(c)) continue;
            SetAggro(c);
            return;
        }
        // 다른 적이 없으면 현재 대상 유지 + 카운트만 리셋(즉시 재발동 방지)
        knockdownDealtCount = 0;
    }

    private void OnCancelled()
    {
        if (comboCoroutine != null)
        {
            StopCoroutine(comboCoroutine);
            comboCoroutine = null;
        }
    }

    // 사망: AI 상태머신을 멈추고 진행 중인 코루틴/입력을 정리한다(시체가 계속 행동하지 않도록).
    private void OnDead()
    {
        aiActive = false;

        if (comboCoroutine != null)       { StopCoroutine(comboCoroutine);       comboCoroutine = null; }
        if (chaseCoroutine != null)       { StopCoroutine(chaseCoroutine);       chaseCoroutine = null; }
        if (destroyRopeCoroutine != null) { StopCoroutine(destroyRopeCoroutine); destroyRopeCoroutine = null; }

        command.SetMoveInput(Vector2.zero);
    }
}

