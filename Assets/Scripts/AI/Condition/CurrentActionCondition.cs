using UnityEngine;

[CreateAssetMenu(menuName = "AI/Condition/CurrentAction")]
public class CurrentActionCondition : AICondition
{
    public int actionId;

    public override bool IsMet(Character character, Character aggro)
    {
        var current = character.Caster?.Context?.current;
        return current != null && current.actionId == actionId;
    }
}
