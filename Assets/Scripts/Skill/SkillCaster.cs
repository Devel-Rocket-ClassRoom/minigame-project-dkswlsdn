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

    private float actionTimer;
    private float minTransitionTimer;

    private List<Attack> activateAttack;

    public event Action<SkillAction> onActionStart;
    public event Action onSkillEnd;
    public event Action<int> onCooldownReset;

    private void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        aim = GetComponent<CharacterAim>();
        state = GetComponent<StateManager>();
        character = GetComponent<Character>();
        anchor = GetComponent<CharacterAnchor>();

        onActionStart += movement.SkillMove;
        onSkillEnd += movement.SkillEnd;

        state.onHitstun   += OnCanceled;
        state.onAirborne  += OnCanceled;
        state.onGroggy    += OnCanceled;
        state.onDead      += OnCanceled;
        state.onKnockdown += OnCanceled;

        activateAttack = new List<Attack>();
        context = new SkillContext();
    }

    private void Update()
    {
        context.spendTime += Time.deltaTime;
        activateAttack.RemoveAll(atk => atk == null);

        foreach (var atk in activateAttack)
        {
            if (atk.IsHit) context.isHit = true;
        }

        CheckTransition();
        CheckHitbox();

        if (context.current != null && context.current.useGrab && context?.grabTarget.Count > 0)
        {
            foreach (var target in context.grabTarget)
            {
                if (target != null)
                    target.Movement.MoveToPosition(anchor.anchor.position);
            }
        }
    }

    public bool Cast(Skill skill, int idx)
    {
        if (skill == null) return false;

        context = new SkillContext();
        context.currentIndex = idx;

        if (skill.transitions.Count > 0)
        {
            for (int i = 0; i < skill.transitions.Count; i++)
            {
                if (AllConditionIsMet(skill.transitions[i]))
                {
                    OnCanceled();
                    state.ChangeState(CharacterState.Skill);
                    context.currentIndex = idx;
                    ExecuteAction(skill.transitions[i].nextAction);
                    return true;
                }
            }
        }
        else
        {
            if (state.CanMove)
            {
                OnCanceled();
                state.ChangeState(CharacterState.Skill);
                context.currentIndex = idx;
                ExecuteAction(skill.actions[0]);
                return true;
            }
        }

        context.Clear();
        return false;
    }

    public void ExecuteAction(SkillAction action)
    {
        context.targetPoint = aim.GetLookAtVector(action.targetting, transform, action.aimDistance, out _);
        context.current = action;
        actionTimer = action.actionTime + Time.time;
        minTransitionTimer = action.minTransitionTime + Time.time;
        context.wasDamagedInAction = false;
        context.isHit = false;

        StopCoroutine(nameof(CoSkillAttack));
        StartCoroutine(nameof(CoSkillAttack));

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
            yield return new WaitForSeconds(attack.preDelay);

            if (attack.hitbox != null)
            {
                var instance = Instantiate(attack.hitbox);
                instance.Activate(attack, transform, character.team, character.Id);
                activateAttack.Add(instance);

                var capturedContext = context;
                if (attack.isGrab) instance.onHit += (crt) =>
                {
                    if (crt.Id != character.Id && crt.State.State != CharacterState.Grapped)
                    {
                        capturedContext.grabTarget.Add(crt);
                        crt.State.ChangeState(CharacterState.Grapped);
                        crt.Movement.StartGrabbed();
                    }
                };
                else if (!attack.isGrab && attack.isCheckHit) instance.onHit += (crt) =>
                {
                    if (crt.Id != character.Id)
                    {
                        capturedContext.hitTarget.Add(crt);
                        if (attack.info.isReleaseGrab) ReleaseGrab(CharacterState.Airborne);
                    }
                };
            }
            else if (attack.hitbox == null && attack.toGrab)
            {
                foreach (var c in context.grabTarget)
                {
                    var info = new AttackInfo(attack.info);
                    info.id = character.Id;
                    info.origin = transform;
                    c.Stat.TakeDamage(info);
                }

                if (attack.info.isReleaseGrab) ReleaseGrab(CharacterState.Airborne);
            }
            else
            {
                throw new Exception("할당된 히트박스 없음");
            }
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

        context.current = null;
        StopAllCoroutines();

        for (int i = activateAttack.Count - 1; i >= 0; i--)
        {
            if (activateAttack[i].HitInfo.info.isDestroyOnCanceled)
            {
                Destroy(activateAttack[i].gameObject);
                activateAttack.RemoveAt(i);
            }
        }
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
