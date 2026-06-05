using System.Collections.Generic;
using UnityEngine;

public class SetItemGrid : MonoBehaviour
{
    [SerializeField] private ItemButton button;
    [SerializeField] private ShowItemDescription desc;
    [SerializeField] private bool isStorage;

    private Character character;

    private void Awake()
    {
        Character.SubscribeToPlayer(OnPlayerAppeared);
    }

    private void OnPlayerAppeared(Character c)
    {
        character = c;
    }

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
            var itm = DatabaseManager.FindItem(item.Key);
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
