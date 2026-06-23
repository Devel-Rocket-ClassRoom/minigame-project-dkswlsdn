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
    [SerializeField] private ItemShiftButton shiftButton;

    private void OnEnable()
    {
        SaveManager.onSaveModified += Clear;
    }

    private void OnDisable()
    {
        SaveManager.onSaveModified -= Clear;
    }

    public void Init(Item item, Character character, bool isStorage)
    {
        this.item = item;
        this.character = character;
        itemName.ChangeText(item.itemName);
        //icon.sprite = item.icon;
        if (DataTableManager.ItemTable.TryGet(item.itemName, out ItemData data))
            desc.ChangeText(data.description);

        if (shiftButton != null) shiftButton.Init(item.itemName, isStorage);

        consumeButton.Init(item, character, entry, isStorage);
    }

    public void Clear()
    {
        itemName.ChangeText(string.Empty);
        icon.sprite = null;
        desc.ChangeText(string.Empty);
    }
}
