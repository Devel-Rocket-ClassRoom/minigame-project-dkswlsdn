using UnityEngine;

[CreateAssetMenu(menuName = "AI/Condition/SkillReady")]
public class SkillReadyCondition : AICondition
{
    public ConditionInput skillInput;

    public override bool IsMet(Character character, Character aggro)
    {
        return character.Executer != null && character.Executer.IsSkillReady(skillInput);
    }
}
