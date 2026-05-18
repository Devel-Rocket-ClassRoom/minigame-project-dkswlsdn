using UnityEngine;

[CreateAssetMenu(menuName = "SkillSystem/Condition/TakeDamage")]
public class TakeDamageCondition : SkillCondition
{
    public override bool IsMet(Character character, SkillContext context) => context.wasDamagedInAction;
}
