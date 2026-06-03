using System;
using System.Collections;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    [SerializeField]
    private CharacterState state = CharacterState.Idle;
    public CharacterState State
    {
        get => state;
        set
        {
            state = value;
            //modelAnimation?.SetInteger("state", (int)state);
        }
    }

    public bool CanMove => state == CharacterState.Idle || state == CharacterState.Move;
    public bool CanUseSkill => CanMove && state != CharacterState.Skill;
    public bool CanRotatePlayer => CanMove || state == CharacterState.WakeUp;
    public bool CanRotateCamera => CanRotatePlayer || state == CharacterState.Knockdown;
    public bool CanNotMove => state == CharacterState.HitStun || state == CharacterState.Airborne || state == CharacterState.Grapped;

    private CharacterStat stat;
    private CharacterMovement movement;
    [SerializeField] private Animator animator;

    public event Action onIdle;
    public event Action onmove;
    public event Action onSkill;
    public event Action onHitstun;
    public event Action onKnockdown;
    public event Action onWakeUp;
    public event Action onAirborne;
    public event Action onGroggy;
    public event Action onGrab;
    public event Action onDead;
    public event Action<float> onFreeze;
    public event Action onClimb;


    public bool IsFrozen { get; private set; }
    private Coroutine freezeCoroutine;
    private float freezeEndTime;   // 현재 프리즈가 끝나는 시각(갈아끼우기 기준)
    private bool snapVerticalVelocity;  // true면 다음 에어본 갱신을 블렌드 없이 즉시 반영

    private bool isGrounded;
    private float stunEndTime;
    [SerializeField] private float knockdownTimer;
    private float wakeUpTimer;

    [SerializeField] private Collider standCollider;
    [SerializeField] private Collider layCollider;

    private const float BASE_KNOCKDOWN_DURATION = 2f;
    private const float BASE_WAKEUP_DURATION    = 0.55f;
    private const float KNOCKDOWN_EXTEND_AMOUNT  = 0.1f;

    private void Awake()
    {
        stat = GetComponent<CharacterStat>();
        movement = GetComponent<CharacterMovement>();
    }

    private void OnEnable()
    {
        stat.onDamageTake += OnDamageTaken;
    }

    private void OnDisable()
    {
        stat.onDamageTake -= OnDamageTaken;
    }

    private void Update()
    {
        CheckTransition();

        // 에어본 중 수직 속도(raw)를 애니메이터로 전달 → 올라감/정점/내려감 BlendTree 구동
        if (movement != null && state == CharacterState.Airborne)
            animator.SetFloat("VerticalVelocity", movement.VerticalVelocity);
    }

    private void CheckTransition()
    {
        switch (state)
        {
            case CharacterState.Idle:
                break;
            case CharacterState.Move:
                break;
            case CharacterState.Skill:
                break;
            case CharacterState.HitStun:
                if (Time.time >= stunEndTime) ChangeState(CharacterState.Idle);
                break;
            case CharacterState.Airborne:
                break;
            case CharacterState.Knockdown:
                if (isGrounded) knockdownTimer -= Time.deltaTime;
                if (knockdownTimer <= 0f) ChangeState(CharacterState.WakeUp);
                break;
            case CharacterState.WakeUp:
                if (Time.time >= wakeUpTimer) ChangeState(CharacterState.Idle);
                break;
            case CharacterState.Groggy:
                break;
            case CharacterState.Grapped:
                break;
            case CharacterState.Dead:
                break;
        }
    }

    public void ChangeState(CharacterState state)
    {
        if (State == CharacterState.Dead) return;

        var prev = State;
        State = state;

        switch (state)
        {
            case CharacterState.Idle:
                onIdle?.Invoke();
                animator.SetTrigger("ReturnToIdle");
                SetColliderState(true);
                break;
            case CharacterState.Move:
                onmove?.Invoke();
                break;
            case CharacterState.Skill:
                onSkill?.Invoke();
                SetColliderState(true);
                break;
            case CharacterState.HitStun:
                onHitstun?.Invoke();
                break;
            case CharacterState.Airborne:
                if (prev == CharacterState.Grapped) knockdownTimer = BASE_KNOCKDOWN_DURATION;
                onAirborne?.Invoke();
                animator.SetTrigger("Airborne");
                SetColliderState(false);
                break;
            case CharacterState.Knockdown:
                onKnockdown?.Invoke();
                animator.SetTrigger("Knockdown");
                SetColliderState(false);
                break;
            case CharacterState.WakeUp:
                wakeUpTimer = BASE_WAKEUP_DURATION + Time.time;
                onWakeUp?.Invoke();
                animator.SetTrigger("WakeUp");
                break;
            case CharacterState.Groggy:
                onGroggy?.Invoke();
                break;
            case CharacterState.Grapped:
                onGrab?.Invoke();
                break;
            case CharacterState.Dead:
                onDead?.Invoke();
                break;
            case CharacterState.Climb:
                onClimb?.Invoke();
                break;
        }
    }

    // 풀 재사용/부활 시 초기화.
    // Dead 상태에선 ChangeState가 막히므로 먼저 State로 직접 Idle을 풀어준 뒤
    // 정식 전이를 호출해 onIdle/콜라이더/애니메이션까지 복구한다.
    public void ResetState()
    {
        State = CharacterState.Idle;
        ChangeState(CharacterState.Idle);
    }

    public void SetGrounded(bool grounded) { isGrounded = grounded; }

    public void OnLand()
    {
        if (state == CharacterState.Airborne)
            ChangeState(CharacterState.Knockdown);
    }

    public void FreezeFor(float duration)
    {
        if (duration <= 0f) return;

        float newEnd = Time.time + duration;

        // 덮어쓰기: 진행 중이던 프리즈를 무시하고 항상 '지금부터 duration'으로 교체한다.
        // 늘거나 줄어든 차이(delta, 음수 가능)만 구독자에 전달 → actionTimer/skillEndTime이
        // 누적되지 않고 최신 값으로 갱신됨. (같은 프레임 다중 타격은 delta=0이라 한 번만 적용)
        float referenceEnd = Mathf.Max(freezeEndTime, Time.time);
        float delta = newEnd - referenceEnd;
        freezeEndTime = newEnd;

        IsFrozen = true;
        if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
        freezeCoroutine = StartCoroutine(CoFreezeAnimator(newEnd - Time.time)); // 남은 전체 길이로 재시작

        onFreeze?.Invoke(delta);
    }

    private IEnumerator CoFreezeAnimator(float duration)
    {
        animator.speed = 0f;
        yield return new WaitForSeconds(duration);
        animator.speed = 1f;
        IsFrozen = false;
    }

    private void OnDamageTaken(Character character, AttackInfo hit)
    {
        FreezeFor(hit.fixedStun);

        switch (hit.reaction)
        {
            case HitReactionType.HitStun:
                if (state == CharacterState.Knockdown)
                {
                    if (hit.airborneForce.y <= 0)
                        knockdownTimer = Mathf.Min(2, KNOCKDOWN_EXTEND_AMOUNT + knockdownTimer);
                    else
                        ChangeState(CharacterState.Airborne);
                }
                else if (state == CharacterState.Airborne || state == CharacterState.Grapped)
                {

                }
                else
                {
                    animator.SetTrigger("HitStun");
                    ChangeState(CharacterState.HitStun);
                    stunEndTime = hit.stunDuration + Time.time;
                }
                break;

            case HitReactionType.Airborne:
                if (state == CharacterState.Grapped) break;
                if (state == CharacterState.Idle || state == CharacterState.HitStun || state == CharacterState.Move || state == CharacterState.Skill)
                    knockdownTimer = BASE_KNOCKDOWN_DURATION;
                ChangeState(CharacterState.Airborne);
                break;

            case HitReactionType.Knockdown:
                if (state == CharacterState.Grapped) break;
                if (state == CharacterState.Idle || state == CharacterState.Move || state == CharacterState.Skill)
                    knockdownTimer = BASE_KNOCKDOWN_DURATION;
                ChangeState(CharacterState.Knockdown);
                break;

            case HitReactionType.Groggy:
                if (state == CharacterState.Idle || state == CharacterState.Move || state == CharacterState.Skill)
                {
                    knockdownTimer = BASE_KNOCKDOWN_DURATION;
                    ChangeState(CharacterState.Groggy);
                }
                break;
        }
    }

    private void SetColliderState(bool isStand)
    {
        standCollider.enabled = isStand;
        layCollider.enabled = !isStand;
    }
}
