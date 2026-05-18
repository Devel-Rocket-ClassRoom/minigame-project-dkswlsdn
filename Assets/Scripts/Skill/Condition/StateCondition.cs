using UnityEngine;

[CreateAssetMenu(menuName = "SkillSystem/Condition/StateCheck")]
public class StateCondition : SkillCondition
{
    public CharacterState state;

    public override bool IsMet(Character character, SkillContext context)
    {
        return character.State.State == state;
    }
}