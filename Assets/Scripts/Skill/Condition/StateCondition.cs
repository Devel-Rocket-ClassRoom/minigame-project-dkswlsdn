using UnityEngine;

[CreateAssetMenu(menuName = "SkillSystem/Condition/StateCheck")]
public class StateCondition : SkillCondition
{
    public CharacterState[] state;

    public override bool IsMet(Character character, SkillContext context)
    {
        bool b = false;

        for (int i = 0; i < state.Length; i++)
        {
            if (state[i] == character.State.State) b = true;
        }

        return b;
    }
}