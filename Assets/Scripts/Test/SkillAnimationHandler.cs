using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkillCaster))]
public class SkillAnimationHandler : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("핑퐁 스킬 스테이트 (콤보/연속 스킬 진입 블렌딩용)")]
    [Tooltip("Animator Controller에 만든 스킬 스테이트 이름 2개")]
    [SerializeField] private string[] skillStateNames = { "Skill A", "Skill B" };
    [Tooltip("각 스테이트가 쓰는 플레이스홀더 클립 이름 2개 (오버라이드 대상). 위 스테이트와 순서 일치")]
    [SerializeField] private string[] placeholderClipNames = { "SkillA", "SkillB" };

    private SkillCaster caster;
    private StateManager state;
    private AnimatorOverrideController overrideController;
    private List<KeyValuePair<AnimationClip, AnimationClip>> overridesList;

    private int pingPong;
    private Coroutine phaseCoroutine;

    private void Awake()
    {
        caster = GetComponent<SkillCaster>();
        state  = GetComponent<StateManager>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        Rebind(animator);
    }

    public void Rebind(Animator newAnimator)
    {
        animator = newAnimator;
        if (animator == null) return;

        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;

        overridesList = new List<KeyValuePair<AnimationClip, AnimationClip>>(overrideController.overridesCount);
        overrideController.GetOverrides(overridesList);

        pingPong = 0;
    }

    private void OnEnable()  => caster.onActionStart += OnActionStart;
    private void OnDisable() => caster.onActionStart -= OnActionStart;

    private void OnActionStart(SkillAction action)
    {
        if (action.animationPhases == null || action.animationPhases.Count == 0) return;

        if (phaseCoroutine != null) StopCoroutine(phaseCoroutine);
        phaseCoroutine = StartCoroutine(CoPlayPhases(action.animationPhases));
    }

    public void StopAnimation()
    {
        if (phaseCoroutine != null)
        {
            StopCoroutine(phaseCoroutine);
            phaseCoroutine = null;
        }
    }

    private IEnumerator CoPlayPhases(List<AnimationPhase> phases)
    {
        foreach (var phase in phases)
        {
            if (phase.clip == null) continue;

            if (phase.delay > 0f)
                yield return new WaitForSecondsUnfrozen(phase.delay, state);

            string targetState = PlayPhaseClip(phase);

            if (phase.blendTime > 0f)
                yield return new WaitForSecondsUnfrozen(phase.blendTime, state);

            if (phase.duration > 0f)
                yield return StartCoroutine(CoDriveAnimation(phase, targetState));
        }
    }

    private string PlayPhaseClip(AnimationPhase phase)
    {
        pingPong ^= 1;
        string targetClipKey = placeholderClipNames[pingPong];
        string targetState   = skillStateNames[pingPong];

        for (int i = 0; i < overridesList.Count; i++)
        {
            if (overridesList[i].Key.name == targetClipKey)
            {
                overridesList[i] = new KeyValuePair<AnimationClip, AnimationClip>(
                    overridesList[i].Key, phase.clip);
                break;
            }
        }
        overrideController.ApplyOverrides(overridesList);

        float totalFrames    = phase.clip.frameRate * phase.clip.length;
        float startNormalized = phase.startFrame / totalFrames;
        animator.CrossFadeInFixedTime(targetState, phase.blendTime, 0, startNormalized * phase.clip.length);

        return targetState;
    }

    private IEnumerator CoDriveAnimation(AnimationPhase phase, string stateName)
    {
        float totalFrames = phase.clip.frameRate * phase.clip.length;
        int   endFrame    = phase.endFrame <= 0 ? (int)totalFrames : phase.endFrame;

        float elapsed = 0f;
        while (elapsed < phase.duration)
        {
            if (!state.IsFrozen)
            {
                float t             = Mathf.Clamp01(elapsed / phase.duration);
                float progress      = phase.easingCurve.Evaluate(t);
                float normalizedTime = Mathf.Lerp(phase.startFrame, endFrame, progress) / totalFrames;
                animator.Play(stateName, 0, normalizedTime);
                elapsed += Time.deltaTime;
            }
            yield return null;
        }

        animator.Play(stateName, 0, (float)endFrame / totalFrames);
    }
}
