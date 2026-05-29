using UnityEngine;

[CreateAssetMenu(menuName = "AI/Condition/Angle")]
public class AngleCondition : AICondition
{
    public enum Direction { Front, Back }

    public Direction direction;
    [Range(0f, 180f)] public float angle;

    public override bool IsMet(Character character, Character aggro)
    {
        if (aggro == null) return false;

        var toAggro = (aggro.transform.position - character.transform.position).normalized;
        var forward = character.transform.forward;

        if (direction == Direction.Back)
            forward = -forward;

        return Vector3.Angle(forward, toAggro) < angle;
    }
}
