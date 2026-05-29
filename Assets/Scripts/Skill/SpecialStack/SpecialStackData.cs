using UnityEngine;

public abstract class SpecialStackData : ScriptableObject
{
    public int maxStack;
    public bool useFreeze;

    public virtual void Apply(Character character, int count) { }
    public abstract void OnGained(Character character, int gained);
    public virtual void OnRemoved(Character character, int count) { }
}
