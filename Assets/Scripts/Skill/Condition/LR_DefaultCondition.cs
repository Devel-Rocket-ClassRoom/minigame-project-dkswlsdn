using UnityEngine;

[CreateAssetMenu(menuName = "SkillSystem/Condition/LR Default Condition")]
public class LR_DefaultCondition : SkillCondition
{
    public override bool IsMet(Character character, SkillContext context)
    {
        // 기본 상태(시전 중인 스킬 없음)면 허용
        if (context.current == null) return true;

        bool canMove = character.State.CanMove;
        bool isLSkill = context.current.actionId == 1 && context.spendTime <= 0.1f;
        return canMove || isLSkill;
    }
}
