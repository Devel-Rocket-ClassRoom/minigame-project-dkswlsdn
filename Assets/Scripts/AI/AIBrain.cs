using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIBrain : MonoBehaviour
{
    // ── 컴포넌트 ──────────────────────────────────────────────
    private Character character;
    private NPCCommander command;
    private SkillExecuter executer;
    private CharacterStat stat;
    private SkillCaster caster;
    private NPCMovement movement;
    private StateManager state;
    private NPCCamera cam;
    private NPCSight sight;
    private NavMeshAgent agent;

    // ── 인스펙터 ──────────────────────────────────────────────
    [SerializeField] private float safeDistance;
    [SerializeField] private float maxChaseTime = 3f;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private List<Node> patrolNodes;

    // ── 상태 ──────────────────────────────────────────────────
    private enum AIState { Patrol, Combat, Chase, DestroyRope }
    private AIState currentState;

    // ── 런타임 변수 ───────────────────────────────────────────
    private Character aggro;
    private Coroutine chaseCoroutine;
    private Coroutine comboCoroutine;
    private Coroutine destroyRopeCoroutine;
    private int patrolIndex;

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
        state.onHitstun    += OnCancelled;
        state.onGrab       += OnCancelled;
        state.onGroggy     += OnCancelled;
        state.onKnockdown  += OnCancelled;
    }

    private void OnEnable()
    {
        agent.updatePosition = false;
        agent.updateRotation = true;

        if (NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas))
            agent.Warp(hit.position);

        // 생성 직후(프리팹 위치 = NavMesh 밖)엔 아직 위치/순찰이 세팅되기 전이므로 패트롤 시작을 보류한다.
        // 스포너가 위치를 잡고 다시 SetActive 하면 Warp 성공 → 여기서 정상적으로 Patrol 시작.
        if (!agent.isOnNavMesh) return;

        ChangeState(AIState.Patrol);
    }

    // 스포너가 활성화 '전에' 순찰 경로를 주입한다.
    // (C안: 이후 SetActive 시 OnEnable이 이 경로로 Warp + Patrol을 시작)
    public void SetPatrol(List<Node> patrol)
    {
        patrolNodes = patrol;
        aggro = null;
    }

    // ── 메인 루프 ─────────────────────────────────────────────
    private void Update()
    {
        agent.nextPosition = transform.position;
        CheckTransition();
        ExecuteState();
    }

    // ── 상태 전이 체크 ────────────────────────────────────────
    private void CheckTransition()
    {
        switch (currentState)
        {
            case AIState.Patrol:
                if (aggro != null)
                    ChangeState(AIState.Combat);
                break;

            case AIState.Combat:
                if (aggro == null)
                    ChangeState(AIState.Patrol);
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
    private void ExecuteState()
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
    private void ChangeState(AIState next, Anchor anchor = null)
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
        ChangeState(AIState.Patrol);
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
            ChangeState(aggro != null ? AIState.Chase : AIState.Patrol);
            yield break;
        }

        agent.isStopped = true;
        command.PressInput(ConditionInput.Q);
        yield return new WaitForSeconds(0.5f);
        command.ReleaseInput(ConditionInput.Q);

        destroyRopeCoroutine = null;
        ChangeState(aggro != null ? AIState.Chase : AIState.Patrol);
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
    private void ApplyNavMeshInput()
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
                break;
            }
        }
    }

    private Anchor FindVisitedAnchorNearby()
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
    public void OnDetected(Character target)
    {
        aggro = target;
        ChangeState(AIState.Combat);
    }

    public void OnLost(Character target)
    {
        if (aggro == target) ChangeState(AIState.Chase);
    }

    private void ChangeAggro(Character target, AttackInfo info)
    {
        aggro = target;
        if (currentState != AIState.Combat) ChangeState(AIState.Combat);
    }

    private void OnCancelled()
    {
        if (comboCoroutine != null)
        {
            StopCoroutine(comboCoroutine);
            comboCoroutine = null;
        }
    }
}
