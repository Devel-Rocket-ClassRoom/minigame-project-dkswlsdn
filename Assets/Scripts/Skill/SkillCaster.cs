using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCaster : MonoBehaviour
{
    private CharacterMovement movement;
    private StateManager state;
    private Character character;
    private CharacterAnchor anchor;

    private Skill currentSkill;
    private SkillAction currentAction;
    private SkillContext context;

    private float actionTimer;
    private float minTransitionTimer;

    private List<Attack> activateAttack;

    public event Action<MovementMethod, float> onActionStart;
    public event Action onSkillEnd;

    private void Awake()
    {
        movement = GetComponent<CharacterMovement>();
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
    }

    private void Update()
    {
        activateAttack.RemoveAll(atk => atk == null);

        foreach (var atk in activateAttack)
        {
            if (atk.IsHit) context.isHit = true;
        }

        CheckTransition();
        CheckHitbox();

        if (currentAction != null && currentAction.useGrab && context?.grabTarget.Count > 0)
        {
            foreach (var target in context.grabTarget)
            {
                if (target != null)
                    target.Movement.MoveToPosition(anchor.anchor.position);
            }
        }
    }

    public void Cast(Skill skill)
    {
        context = new SkillContext();
        currentSkill = skill;

        if (skill.transitions.Count > 0)
        {
            for (int i = 0; i < skill.transitions.Count; i++)
            {
                if (AllConditionIsMet(skill.transitions[i]))
                {
                    state.ChangeState(CharacterState.Skill);
                    ExecuteAction(skill.transitions[i].nextAction);
                    break;
                }
            }
        }
        else
        {
            if (state.CanMove)
            {
                state.ChangeState(CharacterState.Skill);
                ExecuteAction(skill.actions[0]);
            }
        }
    }

    public void ExecuteAction(SkillAction action)
    {
        currentAction = action;
        actionTimer = action.actionTime + Time.time;
        minTransitionTimer = action.minTransitionTime + Time.time;
        context.wasDamagedInAction = false;
        context.isHit = false;

        StopCoroutine(nameof(CoSkillAttack));
        StartCoroutine(nameof(CoSkillAttack));

        onActionStart.Invoke(action.movementMethod, action.actionTime);
    }

    public void CheckTransition()
    {
        if (currentAction == null) return;

        if (Time.time > minTransitionTimer && Time.time < actionTimer)
        {
            if (context.next == null)
            {
                foreach (var transition in currentAction.transitions)
                {
                    if (AllConditionIsMet(transition))
                    {
                        if (transition.immediateTransition)
                        {
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
            else if (currentAction.autoTransition != null)
            {
                ExecuteAction(currentAction.autoTransition);
            }
            else
            {
                ReleaseGrab(CharacterState.Idle);
                currentAction = null;
                currentSkill = null;
                state.ChangeState(CharacterState.Idle);
                onSkillEnd.Invoke();
            }
        }
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
        foreach (var attack in currentAction.attack)
        {
            yield return new WaitForSeconds(attack.preDelay);

            var instance = Instantiate(attack.hitbox);
            instance.Activate(attack.info, transform.position + attack.positionOffset, transform.forward, character.team, attack.isGrab);
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
            else instance.onHit += (crt) =>
            {
                if (crt.Id != character.Id)
                {
                    capturedContext.hitTarget.Add(crt);
                    if (attack.info.isReleaseGrab) ReleaseGrab(CharacterState.Airborne);
                }
            };
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

        currentAction = null;
        currentSkill = null;
        StopAllCoroutines();

        for (int i = activateAttack.Count - 1; i >= 0; i--)
        {
            if (activateAttack[i].HitInfo.isDestroyOnCanceled)
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
    public SkillAction next;
    public bool isHit;
    public int hitCount;
    public List<Character> hitTarget = new List<Character>();
    public List<Character> grabTarget = new List<Character>();
    public bool wasDamagedInAction;
}
