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

    private int pingPong; // 0/1, 매 스킬마다 토글해서 직전과 다른 스테이트로 진입

    private void Awake()
    {
        caster = GetComponent<SkillCaster>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        Rebind(animator);
    }

    // 모델 교체 시 새 Animator로 오버라이드 컨트롤러를 다시 빌드한다.
    // 새 모델 인스턴스의 Animator는 기본 컨트롤러를 들고 있으므로(오버라이드 중첩 아님) 그대로 감싸면 된다.
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
        if (action.clip == null) return;

        // 매 스킬마다 A↔B 토글.
        // 직전 스킬과 "다른" 스테이트로 들어가야 CrossFade가 이전 모션→새 모션을 제대로 블렌딩한다.
        // (같은 스테이트로 CrossFade하면 오버라이드 교체 때문에 블렌딩이 안 됨)
        pingPong ^= 1;
        string targetClipKey = placeholderClipNames[pingPong];
        string targetState   = skillStateNames[pingPong];

        // 이번에 들어갈 스테이트의 플레이스홀더만 새 클립으로 교체.
        // 반대쪽 스테이트는 직전 클립을 유지하므로 블렌딩 소스가 살아있다.
        for (int i = 0; i < overridesList.Count; i++)
        {
            if (overridesList[i].Key.name == targetClipKey)
            {
                overridesList[i] = new KeyValuePair<AnimationClip, AnimationClip>(
                    overridesList[i].Key, action.clip);
                break;
            }
        }

        overrideController.ApplyOverrides(overridesList);

        // blendTime > 0 이면 그 시간(초)만큼 진입 보간, 0이면 즉시 전환
        if (action.blendTime > 0f)
            animator.CrossFadeInFixedTime(targetState, action.blendTime, 0, 0f);
        else
            animator.Play(targetState, 0, 0f);
    }
}
