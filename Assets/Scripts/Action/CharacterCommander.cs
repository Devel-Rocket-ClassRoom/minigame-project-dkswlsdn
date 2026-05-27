using UnityEngine;

public abstract class CharacterCommander : MonoBehaviour
{
    public abstract Vector2 MoveInput { get; }
    public abstract bool GetInput(ConditionInput input, bool held = false);
    public abstract bool GetInputUp(ConditionInput input);
}
