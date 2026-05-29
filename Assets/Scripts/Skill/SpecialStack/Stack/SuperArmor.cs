using UnityEngine;

[CreateAssetMenu(menuName = "SkillSystem/Stack/SuperArmor")]
public class SuperArmor : SpecialStackData
{
    [SerializeField] private ArmorType armorType;

    public override void OnGained(Character character, int count)
    {
        character.Stat.ApplyArmor(armorType);
    }

    public override void OnRemoved(Character character, int count)
    {
        character.Stat.RemoveArmor(armorType);
    }
}
