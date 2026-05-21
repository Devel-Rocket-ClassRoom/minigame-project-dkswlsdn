using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCommander : CharacterCommander
{
    public override Vector2 MoveInput   => PlayerMovement.Action.Player.Move.ReadValue<Vector2>();
    public override Vector2 RotateInput => PlayerMovement.Action.Player.Rotate.ReadValue<Vector2>();

    public override bool GetInput(ConditionInput input, bool held)
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

    public override bool GetInputUp(ConditionInput input)
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
        return action.WasReleasedThisFrame();
    }
}
