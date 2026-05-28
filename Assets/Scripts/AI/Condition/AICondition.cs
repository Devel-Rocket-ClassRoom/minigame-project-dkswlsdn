using UnityEngine;

public abstract class AICondition : ScriptableObject
{
    public abstract bool IsMet(Character character, Character aggro);
}
