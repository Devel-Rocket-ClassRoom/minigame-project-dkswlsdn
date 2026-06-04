using UnityEngine;

[CreateAssetMenu(menuName = "Item/Character")]
public class GetCharacterItem : Item
{
    [SerializeField] private ImplementedCharacter character;

    public override void OnUse(Character character)
    {
        base.OnUse(character);
        var list = SaveManager.CurrentSave.unlockedCharacterList;
        var c = SaveData.implementedCharacter[(int)this.character];
        if (!list.Contains(c)) { list.Add(c); }
    }
}
