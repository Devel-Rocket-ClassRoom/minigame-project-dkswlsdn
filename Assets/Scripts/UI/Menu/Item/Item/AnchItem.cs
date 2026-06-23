using UnityEngine;

[CreateAssetMenu(menuName = "Item/Anch")]
public class AnchItem : Item
{
    public override void OnUse(Character character)
    {
        base.OnUse(character);
        SaveManager.CurrentSave.anchCount++;
    }
}
