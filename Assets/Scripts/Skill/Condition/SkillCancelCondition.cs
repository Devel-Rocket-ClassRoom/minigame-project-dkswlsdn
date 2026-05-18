using UnityEngine;

[CreateAssetMenu(menuName = "SkillSystem/Condition/SkillCheck")]
public class SkillCancelCondition : SkillCondition
{
    public int id;
    public override bool IsMet(Character character, SkillContext context)
    {
        return context.current == null ? false : context.current.actionId == id;
    }
}
