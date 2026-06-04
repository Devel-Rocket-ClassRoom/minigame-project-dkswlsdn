using UnityEngine;
using UnityEngine.UI;

public class ShowCreateDescription : MonoBehaviour
{
    [SerializeField] private TextContainer itemName;
    [SerializeField] private Image icon;
    [SerializeField] private TextContainer desc;
    [SerializeField] private CreateItemButton createButton;

    private void OnEnable()
    {
        //SaveManager.onSaveModified += Clear;
    }

    private void OnDisable()
    {
        //SaveManager.onSaveModified -= Clear;
    }

    public void Init(Item item)
    {
        itemName.ChangeText(item.itemName);
        //icon.sprite = item.icon;
        if (DataTableManager.ItemTable.TryGet(item.itemName, out ItemData data))
            desc.ChangeText(data.description);

        createButton.Init(item);
    }

    public void Clear()
    {
        itemName.ChangeText(string.Empty);
        icon.sprite = null;
        desc.ChangeText(string.Empty);
    }
}
