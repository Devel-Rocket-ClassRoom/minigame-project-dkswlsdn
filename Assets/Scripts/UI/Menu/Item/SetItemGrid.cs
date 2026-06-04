using System.Collections.Generic;
using UnityEngine;

public class SetItemGrid : MonoBehaviour
{
    [SerializeField] private ItemDatabase database;
    [SerializeField] private ItemButton button;
    [SerializeField] private ShowItemDescription desc;
    [SerializeField] private Character character;
    [SerializeField] private bool isStorage;



    private void OnEnable()
    {
        Load();
        SaveManager.onSaveModified += Load;
    }

    private void OnDisable()
    {
        SaveManager.onSaveModified -= Load;
    }

    public void Load()
    {
        Clear();

        var storage = isStorage ? SaveManager.CurrentSave.itemInStorage : SaveManager.CurrentSave.itemInCharacter;

        foreach (var item in storage)
        {
            var itm = database.items.Find(i => i.itemName == item.Key);
            CreateItemButton(itm, item.Value);
        }
    }

    private void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private void CreateItemButton(Item item, int amount)
    {
        if (item == null)
        {
            return;
        }

        var b = Instantiate(button, transform);
        b.Init(item, amount, desc, character, isStorage);
    }
}
