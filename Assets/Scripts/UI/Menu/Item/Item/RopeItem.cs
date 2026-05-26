using UnityEngine;

[CreateAssetMenu(menuName = "Item/Rope")]
public class RopeItem : Item
{
    [SerializeField] private Skill skill;

    public override void OnUse(Character character)
    {
        character.Caster.Cast(skill, -1);
    }
}
