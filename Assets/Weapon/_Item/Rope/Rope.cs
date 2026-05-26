using UnityEngine;

public class Rope : Interactor
{
    [HideInInspector] public Anchor owner;

    public override bool OnDetected(Character character)
    {
        var s = character.State.State;
        if (s != CharacterState.Idle && s != CharacterState.Move) return false;

        character.Movement.EnterClimb(transform.position);
        character.State.ChangeState(CharacterState.Climb);
        owner.AddClimber(character);
        return true;
    }
}
