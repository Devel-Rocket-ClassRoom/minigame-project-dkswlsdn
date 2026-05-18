using UnityEngine;

[CreateAssetMenu(menuName = "SkillSystem/Condition/StatCheck")]
public class StatCondition : SkillCondition
{
    public StatType statType;
    public float thresholdPercent;
    public bool isAbove;

    public override bool IsMet(Character character, SkillContext context)
    {
        float currentVal = character.Stat != null ? character.Stat.GetStatPercent(statType) : 1f;
        return isAbove ? currentVal >= thresholdPercent : currentVal <= thresholdPercent;
    }
}