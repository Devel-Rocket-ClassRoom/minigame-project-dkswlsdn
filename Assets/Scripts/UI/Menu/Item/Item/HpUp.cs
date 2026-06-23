using UnityEngine;

[CreateAssetMenu(menuName = "Item/HpUp")]
public class HpUp : Item
{
    public override void OnUse(Character character)
    {
        base.OnUse(character);
        character.Stat.RestoreHP(100f);
    }
}
