using UnityEngine;

[CreateAssetMenu(menuName = "SkillSystem/Condition/Land")]
public class LandCondition : SkillCondition
{
    public bool isExecuteOnLand;
    public override bool IsMet(Character character, SkillContext context) => isExecuteOnLand ? character.Movement.GetOnGrounded() : !character.Movement.GetOnGrounded();
}
