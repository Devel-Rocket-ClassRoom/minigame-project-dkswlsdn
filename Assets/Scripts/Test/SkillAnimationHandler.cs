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
    private AnimatorOverrideController overrideController;
    private List<KeyValuePair<AnimationClip, AnimationClip>> overridesList;

    private int pingPong;
    private Coroutine phaseCoroutine;

    private void Awake()
    {
        caster = GetComponent<SkillCaster>();
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
        animator.speed = 1f;
        phaseCoroutine = StartCoroutine(CoPlayPhases(action.animationPhases));
    }

    public void StopAnimation()
    {
        if (phaseCoroutine != null)
        {
            StopCoroutine(phaseCoroutine);
            phaseCoroutine = null;
        }
        animator.speed = 1f;
    }

    private IEnumerator CoPlayPhases(List<AnimationPhase> phases)
    {
        foreach (var phase in phases)
        {
            if (phase.clip == null) continue;

            if (phase.delay > 0f)
                yield return new WaitForSeconds(phase.delay);

            PlayPhaseClip(phase);

            if (phase.speedEaseDuration > 0f)
                yield return StartCoroutine(CoEaseSpeed(phase));
            else
                animator.speed = phase.speedFrom;
        }
    }

    private void PlayPhaseClip(AnimationPhase phase)
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

        float startNormalized = phase.startFrame / (phase.clip.frameRate * phase.clip.length);
        animator.speed = phase.speedFrom;
        animator.CrossFadeInFixedTime(targetState, phase.blendTime, 0, startNormalized * phase.clip.length);
    }

    private IEnumerator CoEaseSpeed(AnimationPhase phase)
    {
        float elapsed = 0f;
        while (elapsed < phase.speedEaseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / phase.speedEaseDuration);
            float curveValue = phase.speedCurve.Evaluate(t);
            animator.speed = Mathf.LerpUnclamped(phase.speedFrom, phase.speedTo, curveValue);
            yield return null;
        }
        animator.speed = phase.speedTo;
    }
}
