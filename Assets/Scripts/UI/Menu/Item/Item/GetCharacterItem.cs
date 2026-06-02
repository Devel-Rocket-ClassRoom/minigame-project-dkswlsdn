using UnityEngine;

[CreateAssetMenu(menuName = "Item/Character")]
public class GetCharacterItem : Item
{
    [SerializeField] private ImplementedCharacter character;

    public override void OnUse(Character character)
    {
        base.OnUse(character);
        var list = SaveManager.instance.CurrentSave.unlockedCharacterList;
        var c = SaveManager.instance.CurrentSave.implementedCharacter[(int)this.character];
        if (!list.Contains(c)) { list.Add(c); }
    }
}
