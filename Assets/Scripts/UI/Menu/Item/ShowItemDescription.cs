using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowItemDescription : MonoBehaviour
{
    private Item item;
    private Character character;
    private ItemSaveEntry entry;
    [SerializeField] private TextContainer itemName;
    [SerializeField] private Image icon;
    [SerializeField] private TextContainer desc;
    [SerializeField] private ConsumeButton consumeButton;

    public void Init(Item item, Character character, ItemSaveEntry entry, bool isStorage)
    {
        this.item = item;
        this.character = character;
        this.entry = entry;
        itemName.ChangeText(item.itemName);
        icon.sprite = item.icon;
        desc.ChangeText(item.desc);
        consumeButton.Init(item, character, entry, isStorage);
    }

    public void Clear()
    {
        itemName.ChangeText(string.Empty);
        icon.sprite = null;
        desc.ChangeText(string.Empty);
    }
}
