using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCaster : MonoBehaviour
{
    private CharacterMovement movement;
    private CharacterAim aim;
    private StateManager state;
    private Character character;
    private CharacterAnchor anchor;

    private SkillContext context;
    public SkillContext Context => context;

    private float actionTimer;
    private float minTransitionTimer;

    private List<Attack> spawnedAttacks;

    public event Action<SkillAction> onActionStart;
    public event Action onSkillEnd;
    public event Action<int> onCooldownReset;

    private bool enable = true;

    public float CostRatio => Mathf.Clamp01(context.cost / 100f);

    private void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        aim = GetComponent<CharacterAim>();
        state = GetComponent<StateManager>();
        character = GetComponent<Character>();
        anchor = GetComponent<CharacterAnchor>();
        
        spawnedAttacks = new List<Attack>();
        context = new SkillContext();
    }

    private void OnEnable()
    {
        onActionStart += movement.SkillMove;
        onSkillEnd += movement.SkillEnd;

        state.onHitstun += OnCanceled;
        state.onAirborne += OnCanceled;
        state.onGroggy += OnCanceled;
        state.onGrab += OnCanceled;
        state.onDead += OnCanceled;
        state.onKnockdown += OnCanceled;
        state.onFreeze += OnFreeze;
        state.onDead += OnDead;
    }

    private void OnDisable()
    {
        onActionStart -= movement.SkillMove;
        onSkillEnd -= movement.SkillEnd;

        state.onHitstun -= OnCanceled;
        state.onAirborne -= OnCanceled;
        state.onGroggy -= OnCanceled;
        state.onGrab -= OnCanceled;
        state.onDead -= OnCanceled;
        state.onKnockdown -= OnCanceled;
        state.onFreeze -= OnFreeze;
        state.onDead -= OnDead;
    }

    private void Update()
    {
        if (!enable) return;

        context.spendTime += Time.deltaTime;
        spawnedAttacks.RemoveAll(atk => atk == null);

        CheckTransition();
        CheckHitbox();

        if (context.current != null && context.current.useGrab && context?.grabTarget.Count > 0)
        {
            foreach (var target in context.grabTarget)
            {
                if (target != null)
                {
                    target.Movement.MoveToPosition(anchor.anchor);
                }
            }
        }
    }

    public bool Cast(Skill skill, int idx)
    {
        if (skill == null || !enable) return false;

        if (skill.transitions.Count > 0)
        {
            int prevIndex = context.currentIndex;
            context.currentIndex = idx;

            for (int i = 0; i < skill.transitions.Count; i++)
            {
                if (AllConditionIsMet(skill.transitions[i]))
                {
                    OnCanceled();
                    state.ChangeState(CharacterState.Skill);
                    context.Clear();
                    context.currentIndex = idx;
                    ExecuteAction(skill.transitions[i].nextAction);
                    return true;
                }
            }

            context.currentIndex = prevIndex;
        }
        else
        {
            if (state.CanMove)
            {
                OnCanceled();
                state.ChangeState(CharacterState.Skill);
                context.Clear();
                context.currentIndex = idx;
                ExecuteAction(skill.actions[0]);
                return true;
            }
        }
        
        return false;
    }

    public void ExecuteAction(SkillAction action)
    {
        if (action == null)
        {
            SkillEnd();
            return;
        }

        context.targetPoint = aim.GetLookAtVector(action.targetting, action.targetLayer, action.aimDistance, out _);
        context.current = action;
        actionTimer = action.actionTime + Time.time;
        minTransitionTimer = action.minTransitionTime + Time.time;
        context.wasDamagedInAction = false;
        context.isHit = false;

        StopCoroutine(nameof(CoSkillAttack));
        StartCoroutine(nameof(CoSkillAttack));
        StopCoroutine(nameof(CoGetStack));
        StartCoroutine(nameof(CoGetStack));

        onActionStart.Invoke(action);
    }

    public void CheckTransition()
    {
        if (context.current == null) return;

        if (Time.time > minTransitionTimer && Time.time < actionTimer)
        {
            if (context.next == null)
            {
                foreach (var transition in context.current.transitions)
                {
                    if (AllConditionIsMet(transition))
                    {
                        if (transition.immediateTransition)
                        {
                            if (transition.nextAction != null)
                                ExecuteAction(transition.nextAction);
                            return;
                        }
                        else
                            context.next = transition.nextAction;
                        break;
                    }
                }
            }
        }
        else if (Time.time > actionTimer)
        {
            if (context.next != null)
            {
                SkillAction next = context.next;
                context.next = null;
                ExecuteAction(next);
            }
            else if (context.current.autoTransition != null)
            {
                ExecuteAction(context.current.autoTransition);
            }
            else
            {
                SkillEnd();
            }
        }
    }

    private void SkillEnd()
    {
        ReleaseGrab(CharacterState.Idle);
        state.ChangeState(CharacterState.Idle);
        onCooldownReset?.Invoke(context.currentIndex);
        context.current = null;
        context.Clear();
        onSkillEnd.Invoke();
    }

    public bool AllConditionIsMet(SkillTransition transition)
    {
        foreach (var condition in transition.conditions)
        {
            if (!condition.IsMet(character, context)) return false;
        }
        return true;
    }


    IEnumerator CoSkillAttack()
    {
        foreach (var attack in context.current.attack)
        {
            yield return new WaitForSecondsUnfrozen(attack.preDelay, state);

            if (attack.toGrab)
            {
                foreach (var c in context.grabTarget)
                {
                    var info = new AttackInfo(attack.info);
                    info.id = character.Id;
                    info.isPopup = character.isPlayer;
                    info.origin = transform;
                    c.Stat.TakeDamage(character, info);
                }

                if (attack.info.isReleaseGrab) ReleaseGrab(CharacterState.Airborne);
            }
            else if (attack.type != HitboxType.None)
            {
                var atk = attack;
                atk.aimDir = (context.targetPoint - (transform.position + transform.TransformVector(atk.positionOffset))).normalized;

                var instance = AttackManager.instance.RequestAttack(character, atk, context.targetPoint);
                if (instance == null) continue;

                spawnedAttacks.Add(instance);

                var capturedContext = context;
                if (attack.isGrab)
                {
                    instance.onHit += (crt) =>
                    {
                        if (crt.Id != character.Id && crt.State.State != CharacterState.Grapped)
                        {
                            capturedContext.grabTarget.Add(crt);
                            crt.State.ChangeState(CharacterState.Grapped);
                            crt.Movement.StartGrabbed();
                        }
                    };
                }
                else
                {
                    instance.onHit += (crt) => { if (crt.Id != character.Id) capturedContext.isHit = true; };

                    if (attack.isCheckHit) instance.onHit += (crt) =>
                    {
                        if (crt.Id != character.Id)
                            capturedContext.hitTarget.Add(crt);
                    };
                }

                if (attack.info.isReleaseGrab) ReleaseGrab(CharacterState.Airborne);
            }
            else
            {
                throw new Exception("할당된 히트박스 없음");
            }
        }
    }

    IEnumerator CoGetStack()
    {
        foreach (var stack in context.current.stack)
        {
            yield return new WaitForSecondsUnfrozen(stack.preDelay, state);

            character.Stack.Request(stack.stack, stack.count, stack.life);
        }
    }

    private void OnFreeze(float duration)
    {
        actionTimer += duration;

        foreach (var target in context.grabTarget)
        {
            if (target != null)
                target.State.FreezeFor(duration);
        }
    }

    private void ReleaseGrab(CharacterState state)
    {
        if (context == null) return;
        foreach (var target in context.grabTarget)
        {
            if (target == null) continue;
            if (target.State.State == CharacterState.Grapped)
                target.State.ChangeState(state);
            target.Movement.EndGrabbed();
        }
        context.grabTarget.Clear();
    }

    public void OnCanceled()
    {
        ReleaseGrab(CharacterState.Idle);

        if (context.current != null)
        {
            foreach (var stack in context.current.stack)
            {
                if (stack.onCanceled)
                    character.Stack.Request(stack.stack, stack.count, stack.life);
            }
        }

        context.Clear();
        StopAllCoroutines();

        for (int i = spawnedAttacks.Count - 1; i >= 0; i--)
        {
            if (spawnedAttacks[i] != null && spawnedAttacks[i].method.info.isDestroyOnCanceled)
            {
                AttackManager.instance.DestroyAttack(spawnedAttacks[i]);
                spawnedAttacks.RemoveAt(i);
            }
        }
    }

    private void OnDead()
    {
        OnCanceled();
        enable = false;
    }

    private void CheckHitbox()
    {

    }
}

public class SkillContext
{
    // 현재 스킬
    public int currentIndex;
    public float spendTime;
    public bool isHit;
    public int hitCount;
    public float cost;
    public List<Character> hitTarget = new List<Character>();
    public List<Character> grabTarget = new List<Character>();
    public bool wasDamagedInAction;

    // 현재 액션
    public Transform target;
    public Vector3 targetPoint;
    public SkillAction current;
    public SkillAction next;

    public void Clear()
    {
        currentIndex = -1;
        spendTime = 0f;
        isHit = false;
        hitCount = 0;
        cost = 0f;
        hitTarget.Clear();
        grabTarget.Clear();
        wasDamagedInAction = false;
        target = null;
        targetPoint = Vector3.zero;
        current = null;
        next = null;
    }
}
