using UnityEngine;

[CreateAssetMenu(menuName = "Item/Rope")]
public class Rope : Item
{
    [SerializeField] private Skill skill;

    public override void OnUse(Character character)
    {
        character.Caster.Cast(skill, -1);
    }
}
