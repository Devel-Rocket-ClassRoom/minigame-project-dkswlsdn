using System.Collections.Generic;
using UnityEngine;

public class CharacterInventory : MonoBehaviour
{
    [SerializeField] private ItemDatabase database;
    private Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        AutoConsumeItems();
    }

    private void AutoConsumeItems()
    {
        var inventory = new Dictionary<string, int>(SaveManager.CurrentSave.itemInCharacter);

        foreach (var item in inventory)
        {
            var itm = database.items.Find(i => i.itemName == item.Key);
            if (itm == null) { Debug.Log(itm.itemName); continue;  }
            if (itm.useWhenReturn)
            {
                for (int i = 0; i < inventory[item.Key]; i++)
                {
                    SaveManager.InventoryIO(item.Key, -1, false);
                    itm.OnUse(character);
                }
            }
        }

        SaveManager.SaveRequest();
    }
}
