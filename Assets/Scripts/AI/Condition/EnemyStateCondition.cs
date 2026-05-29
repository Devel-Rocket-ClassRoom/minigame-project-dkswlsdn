using UnityEngine;

[CreateAssetMenu(menuName = "AI/Condition/EnemyState")]
public class EnemyStateCondition : AICondition
{
    public CharacterState targetState;
    public bool reverse;

    public override bool IsMet(Character character, Character aggro)
    {
        return aggro.State.State == targetState ^ reverse;
    }
}
