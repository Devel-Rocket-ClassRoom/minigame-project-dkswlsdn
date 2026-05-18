using UnityEngine;

[CreateAssetMenu(menuName = "SkillSystem/Condition/Land")]
public class LandCondition : SkillCondition
{
    public override bool IsMet(Character character, SkillContext context) => character.Movement.GetOnGrounded();
}
