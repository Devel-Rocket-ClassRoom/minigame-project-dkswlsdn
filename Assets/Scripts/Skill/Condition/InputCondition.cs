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
}

[CreateAssetMenu(menuName = "SkillSystem/Condition/Input")]
public class InputCondition : SkillCondition
{
    public ConditionInput input;
    // false: 누른 순간만 (WasPressedThisFrame), true: 누르는 동안 (IsPressed)
    public bool held;

    public override bool IsMet(Character character, SkillContext context)
    {
        if (character.Commander == null) return false;
        return character.Commander.GetInput(input, held);
    }
}
