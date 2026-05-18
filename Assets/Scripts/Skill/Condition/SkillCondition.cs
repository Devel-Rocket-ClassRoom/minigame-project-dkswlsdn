using UnityEngine;

public abstract class SkillCondition : ScriptableObject
{
    public abstract bool IsMet(Character character, SkillContext context);
}
