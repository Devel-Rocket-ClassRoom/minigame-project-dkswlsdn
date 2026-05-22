using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class SetItemGrid : MonoBehaviour
{
    [SerializeField] private ItemDatabase database;
    [SerializeField] private ItemButton button;
    [SerializeField] private ShowItemDescription desc;
    [SerializeField] private Character character;
    [SerializeField] private bool isStorage;
    private List<ItemSaveEntry> storage;



    private void OnEnable()
    {
        Load();
    }

    public void Load()
    {
        Clear();

        storage = isStorage ? SaveManager.instance.CurrentSave.itemInStorage : SaveManager.instance.CurrentSave.itemInCharacter;

        foreach (var item in storage)
        {
            var itm = database.items.Find(i => i.itemName == item.itemName);
            CreateItemButton(itm, item);
        }
    }

    private void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void CreateItemButton(Item item, ItemSaveEntry entry)
    {
        var b = Instantiate(button, transform);

        if (item == null) return;

        b.Init(item, desc, character, entry, isStorage);
    }
}
