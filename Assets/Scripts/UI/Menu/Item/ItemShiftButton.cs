using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemShiftButton : MonoBehaviour
{
    [SerializeField] private SetItemGrid storageGrid;
    [SerializeField] private SetItemGrid characterGrid;

    private bool toStorage;
    private ItemSaveEntry entry;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnShift);
    }

    public void Init(ItemSaveEntry item, bool isStorage)
    {
        toStorage = !isStorage;
        entry = item;
    }

    private void OnShift()
    {
        var storage = SaveManager.instance.CurrentSave.itemInStorage;
        var character = SaveManager.instance.CurrentSave.itemInCharacter;

        if (toStorage)
        {
            character.Remove(entry);
            storage.Add(entry);
        }
        else
        {
            storage.Remove(entry);
            character.Add(entry);
        }

        toStorage = !toStorage;
        SaveManager.instance.SaveRequest();
        ReLoad();
    }

    private void ReLoad()
    {
        storageGrid.Load();
        characterGrid.Load();
    }
}
