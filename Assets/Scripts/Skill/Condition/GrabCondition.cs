using UnityEngine;

[CreateAssetMenu(menuName = "SkillSystem/Condition/Grab")]
public class GrabCondition : SkillCondition
{
    public int minimunCount = 1;

    public override bool IsMet(Character character, SkillContext context)
    {
        return context.grabTarget.Count >= minimunCount;
    }
}
