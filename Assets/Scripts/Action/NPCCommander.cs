using System.Collections.Generic;
using UnityEngine;

public class NPCCommander : CharacterCommander
{
    private Vector2 moveInput;
    private readonly HashSet<ConditionInput> heldInputs = new();
    private readonly HashSet<ConditionInput> pressedThisFrame = new();
    private readonly HashSet<ConditionInput> releasedThisFrame = new();

    public override Vector2 MoveInput => moveInput;

    public override bool GetInput(ConditionInput input, bool held)
    {
        return held ? heldInputs.Contains(input) : pressedThisFrame.Contains(input);
    }

    // AI에서 호출
    public void SetMoveInput(Vector2 input) => moveInput = input;

    public void PressInput(ConditionInput input)
    {
        if (input == ConditionInput.Q) Debug.Log("Q버튼 눌림");

        pressedThisFrame.Add(input);
        heldInputs.Add(input);
    }

    public void ReleaseInput(ConditionInput input)
    {
        heldInputs.Remove(input);
        releasedThisFrame.Add(input);
    }

    private void Update()
    {
        pressedThisFrame.Clear();
        releasedThisFrame.Clear();
    }

    public override bool GetInputUp(ConditionInput input)
    {
        return releasedThisFrame.Contains(input);
    }
}
