using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkillCaster))]
public class SkillAnimationHandler : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private SkillCaster caster;
    private AnimatorOverrideController overrideController;
    private List<KeyValuePair<AnimationClip, AnimationClip>> overridesList;

    private void Awake()
    {
        caster = GetComponent<SkillCaster>();

        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;

        overridesList = new List<KeyValuePair<AnimationClip, AnimationClip>>(overrideController.overridesCount);
        overrideController.GetOverrides(overridesList);
    }

    private void OnEnable()  => caster.onActionStart += OnActionStart;
    private void OnDisable() => caster.onActionStart -= OnActionStart;

    private void OnActionStart(SkillAction action)
    {
        if (action.clip == null) return;

        for (int i = 0; i < overridesList.Count; i++)
        {
            if (overridesList[i].Key.name == "Skill")
            {
                overridesList[i] = new KeyValuePair<AnimationClip, AnimationClip>(
                    overridesList[i].Key, action.clip);
                break;
            }
        }

        overrideController.ApplyOverrides(overridesList);
        animator.Play("Skill", 0, 0f);
    }
}
