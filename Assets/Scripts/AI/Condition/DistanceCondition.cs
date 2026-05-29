using UnityEngine;

[CreateAssetMenu(menuName = "AI/Condition/Distance")]
public class DistanceCondition : AICondition
{
    public float distance;

    public override bool IsMet(Character character, Character aggro)
    {
        if (aggro == null) return false;

        var distSqr = (character.transform.position - aggro.transform.position).sqrMagnitude;
        return distSqr < distance * distance;
    }
}
