using UnityEngine;

public class WaitForSecondsUnfrozen : CustomYieldInstruction
{
    private float remaining;
    private readonly StateManager state;

    public WaitForSecondsUnfrozen(float duration, StateManager state)
    {
        remaining = duration;
        this.state = state;
    }

    public override bool keepWaiting
    {
        get
        {
            if (!state.IsFrozen)
                remaining -= Time.deltaTime;
            return remaining > 0f;
        }
    }
}
