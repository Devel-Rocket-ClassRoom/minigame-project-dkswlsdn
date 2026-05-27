using UnityEngine;

[CreateAssetMenu(menuName = "SkillSystem/Condition/Stack")]
public class StackCondition : SkillCondition
{
    public SpecialStackData requireStack;
    public int requireCount = 1;

    public override bool IsMet(Character character, SkillContext context)
    {
        return character.Stack.Has(requireStack) && character.Stack.GetCount(requireStack).amount >= requireCount;
    }
}
