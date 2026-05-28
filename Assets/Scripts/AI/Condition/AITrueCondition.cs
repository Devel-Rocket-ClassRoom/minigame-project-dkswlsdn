using UnityEngine;

[CreateAssetMenu(menuName = "AI/Condition/True")]
public class TrueCondition : AICondition
{
    public override bool IsMet(Character character, Character aggro)
    {
        return true;
    }
}
