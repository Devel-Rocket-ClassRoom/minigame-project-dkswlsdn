using UnityEngine;
using UnityEngine.InputSystem;

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
        var p = PlayerMovement.Action.Player;

        InputAction action = input switch
        {
            ConditionInput.MoveForward  => p.MoveForward,
            ConditionInput.MoveBackward => p.MoveBackward,
            ConditionInput.MoveLeft     => p.MoveLeft,
            ConditionInput.MoveRight    => p.MoveRight,
            ConditionInput.SkillL       => p.SkillL,
            ConditionInput.SkillR       => p.SkillR,
            ConditionInput.SkillLR      => p.SkillLR,
            ConditionInput.SkillSL      => p.SkillSL,
            ConditionInput.Space        => p.Space,
            ConditionInput.F            => p.F,
            ConditionInput.Q            => p.Q,
            ConditionInput.E            => p.E,
            _                           => null
        };

        if (action == null) return false;
        return held ? action.IsPressed() : action.WasPressedThisFrame();
    }
}
