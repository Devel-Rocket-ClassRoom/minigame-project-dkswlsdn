using UnityEngine;

[CreateAssetMenu(menuName = "Item/HpUp")]
public class HpUp : Item
{
    public override void OnUse(Character character)
    {
        character.Stat.RestoreHP(100f);
    }
}
