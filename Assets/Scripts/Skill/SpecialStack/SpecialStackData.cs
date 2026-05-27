using UnityEngine;

public abstract class SpecialStackData : ScriptableObject
{
    public int maxStack;

    public abstract void Apply(Character character, int count);
    public virtual void OnRemoved(Character character) { }
}
