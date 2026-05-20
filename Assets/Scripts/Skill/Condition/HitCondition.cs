using UnityEngine;

[CreateAssetMenu(menuName = "SkillSystem/Condition/Hit")]
public class HItCondition : SkillCondition
{
    public int minimunCount = 1;

    public override bool IsMet(Character character, SkillContext context)
    {
        return context.hitTarget.Count >= minimunCount;
    }
}
