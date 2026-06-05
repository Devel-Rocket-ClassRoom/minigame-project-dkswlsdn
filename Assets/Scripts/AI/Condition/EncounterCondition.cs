using UnityEngine;

[CreateAssetMenu(menuName = "AI/Condition/Encounter")]
public class EncounterCondition : AICondition
{
    public override bool IsMet(Character character, Character aggro)
    {
        bool hasAggro = aggro != null;
        bool isInSight = character.Sight.Visibles.Contains(aggro);
        return hasAggro && isInSight;
    }
}
