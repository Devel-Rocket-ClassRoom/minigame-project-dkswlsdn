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

    private bool isGrounded;
    private float stunEndTime;
    [SerializeField] private float knockdownTimer;
    private float wakeUpTimer;

    [SerializeField] private Collider standCollider;
    [SerializeField] private Collider layCollider;

    private const float BASE_KNOCKDOWN_DURATION = 2f;
    private const float BASE_WAKEUP_DURATION    = 0.7f;
    private const float KNOCKDOWN_EXTEND_AMOUNT  = 0.1f;

    private void Awake()
    {
        stat = GetComponent<CharacterStat>();
        stat.onDamageTake += OnDamageTaken;
        
    }

    private void Update()
    {
        CheckTransition();
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
                animator.SetTrigger("Airborne");
                SetColliderState(false);
                break;
            case CharacterState.WakeUp:
                wakeUpTimer = BASE_WAKEUP_DURATION + Time.time;
                onWakeUp?.Invoke();
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

    public void SetGrounded(bool grounded) { isGrounded = grounded; }

    public void OnLand()
    {
        if (state == CharacterState.Airborne)
            ChangeState(CharacterState.Knockdown);
    }

    public void FreezeFor(float duration)
    {
        if (duration <= 0f) return;
        IsFrozen = true;
        if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
        freezeCoroutine = StartCoroutine(CoFreezeAnimator(duration));
        onFreeze?.Invoke(duration);
    }

    private IEnumerator CoFreezeAnimator(float duration)
    {
        animator.speed = 0f;
        yield return new WaitForSeconds(duration);
        animator.speed = 1f;
        IsFrozen = false;
    }

    private void OnDamageTaken(AttackInfo hit)
    {
        float stun = hit.reaction == HitReactionType.Gaurded ? hit.stunForce * 0.2f + hit.fixedStun : hit.fixedStun;
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
                    ChangeState(CharacterState.HitStun);
                    stunEndTime = hit.stunDuration + Time.time;
                }
                break;

            case HitReactionType.Airborne:
                if (state == CharacterState.Idle || state == CharacterState.HitStun || state == CharacterState.Move || state == CharacterState.Skill || state == CharacterState.Grapped)
                    knockdownTimer = BASE_KNOCKDOWN_DURATION;
                ChangeState(CharacterState.Airborne);
                break;

            case HitReactionType.Knockdown:
                if (state == CharacterState.Idle || state == CharacterState.Move || state == CharacterState.Skill || state == CharacterState.Grapped)
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
