using UnityEngine;

[CreateAssetMenu(menuName = "SkillSystem/Condition/True")]
public class AITrueCondition : SkillCondition
{
    public override bool IsMet(Character character, SkillContext context)
    {
        return true;
    }
}
