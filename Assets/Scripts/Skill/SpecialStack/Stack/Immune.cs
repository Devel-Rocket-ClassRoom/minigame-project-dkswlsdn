using UnityEngine;

[CreateAssetMenu(menuName = "SkillSystem/Stack/Immune")]
public class Immune : SpecialStackData
{
    public override void OnGained(Character character, int count, Character grantor)
    {
        character.Stat.ApplyImmune(true);
    }

    public override void OnRemoved(Character character, int count, Character grantor)
    {
        character.Stat.ApplyImmune(false);
    }
}
