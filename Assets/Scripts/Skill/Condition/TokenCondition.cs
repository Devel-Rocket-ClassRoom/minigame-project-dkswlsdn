using UnityEngine;

[CreateAssetMenu(menuName = "SkillSystem/Condition/TakeDamage")]
public class TokenCondition : SkillCondition
{
    public int requireToken;

    public override bool IsMet(Character character, SkillContext context)
    {
        return character.opennedToken[context.currentIndex] >= requireToken;
    }
}
