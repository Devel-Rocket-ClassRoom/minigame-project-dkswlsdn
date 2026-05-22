using UnityEngine;
using UnityEngine.UI;

public class ConsumeButton : MonoBehaviour
{
    private Item item;
    private Character character;
    private ItemSaveEntry entry;
    private Button button;
    [SerializeField] private SetItemGrid storageGrid;
    [SerializeField] private SetItemGrid characterGrid;
    [SerializeField] private ShowItemDescription desc;
    private bool isStorage;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Init(Item item, Character character, ItemSaveEntry entry, bool isStorage)
    {
        this.item = item;
        this.character = character;
        this.entry = entry;
        this.isStorage = isStorage;

        button.onClick.AddListener(Consume);
    }

    private void Consume()
    {
        var inventory = isStorage ? SaveManager.instance.CurrentSave.itemInStorage : SaveManager.instance.CurrentSave.itemInCharacter;
        inventory.Remove(entry);
        item.OnUse(character);
        SaveManager.instance.SaveRequest();
        button.onClick.RemoveAllListeners();
        ReLoad();
    }

    private void ReLoad()
    {
        storageGrid.Load();
        characterGrid.Load();
        desc.Clear();
    }
}
