using System.Collections.Generic;
using UnityEngine;

public class NPCCommander : CharacterCommander
{
    private Vector2 moveInput;
    private Vector2 rotateInput;
    private readonly HashSet<ConditionInput> heldInputs = new();
    private readonly HashSet<ConditionInput> pressedThisFrame = new();
    private readonly HashSet<ConditionInput> releasedThisFrame = new();

    public override Vector2 MoveInput => moveInput;
    public override Vector2 RotateInput => rotateInput;

    public override bool GetInput(ConditionInput input, bool held)
    {
        return held ? heldInputs.Contains(input) : pressedThisFrame.Contains(input);
    }

    // AI에서 호출
    public void SetMoveInput(Vector2 input) => moveInput = input;
    public void SetRotateInput(Vector2 input) => rotateInput = input;

    public void PressInput(ConditionInput input)
    {
        pressedThisFrame.Add(input);
        heldInputs.Add(input);
    }

    public void ReleaseInput(ConditionInput input)
    {
        heldInputs.Remove(input);
        releasedThisFrame.Add(input);
    }

    private void LateUpdate()
    {
        pressedThisFrame.Clear();
        releasedThisFrame.Clear();
    }

    public override bool GetInputUp(ConditionInput input)
    {
        return releasedThisFrame.Contains(input);
    }
}
