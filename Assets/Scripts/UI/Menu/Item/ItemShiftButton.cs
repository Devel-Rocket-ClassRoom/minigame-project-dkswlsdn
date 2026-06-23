using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemShiftButton : MonoBehaviour
{
    [SerializeField] private SetItemGrid storageGrid;
    [SerializeField] private SetItemGrid characterGrid;

    private bool isStorage;
    private string item;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnShift);
    }

    public void Init(string item, bool isStorage)
    {
        this.isStorage = isStorage;
        this.item = item;
    }

    private void OnShift()
    {
        if (isStorage && SaveManager.CurrentSave.itemInStorage.ContainsKey(item))
        {
            SaveManager.InventoryIO(item, 1, !isStorage);
            SaveManager.InventoryIO(item, -1, isStorage);
        }
        else if (!isStorage && SaveManager.CurrentSave.itemInCharacter.ContainsKey(item))
        {
            SaveManager.InventoryIO(item, 1, !isStorage);
            SaveManager.InventoryIO(item, -1, isStorage);
        }

        SaveManager.SaveRequest();
    }
}
