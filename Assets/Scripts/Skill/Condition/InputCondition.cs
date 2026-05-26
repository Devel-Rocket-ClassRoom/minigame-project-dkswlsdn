using UnityEngine;

public enum ConditionInput
{
    MoveForward,
    MoveBackward,
    MoveLeft,
    MoveRight,
    SkillL,
    SkillR,
    SkillLR,
    SkillSL,
    Space,
    F,
    Q,
    E,
    Interaction,
    Item1, Item2, Item3, Item4, Item5,
}

[CreateAssetMenu(menuName = "SkillSystem/Condition/Input")]
public class InputCondition : SkillCondition
{
    public ConditionInput input;
    public bool held;
    public bool up;

    public override bool IsMet(Character character, SkillContext context)
    {
        if (character.Commander == null) return false;
        if (up) return character.Commander.GetInputUp(input);
        return character.Commander.GetInput(input, held);
    }
}
