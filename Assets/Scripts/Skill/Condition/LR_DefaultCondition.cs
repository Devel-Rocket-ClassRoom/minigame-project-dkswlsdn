using UnityEngine;

[CreateAssetMenu(menuName = "SkillSystem/Condition/LR Default Condition")]
public class LR_DefaultCondition : SkillCondition
{
    public override bool IsMet(Character character, SkillContext context)
    {
        bool canMove = character.State.CanMove;
        bool isLSkill = context.current.actionId == 1 && context.spendTime <= 0.1f;
        return canMove || isLSkill;
    }
}
