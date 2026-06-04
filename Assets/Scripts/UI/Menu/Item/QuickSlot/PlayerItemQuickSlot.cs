using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemQuickSlot : ItemQuickSlot
{
    public List<string> itemTypeList;
    [SerializeField] protected ItemDatabase database;

    protected virtual void Update()
    {
        if (commander.GetInput(ConditionInput.Item1) && itemTypeList.Count > 0 && enable)
        {
            var dict = SaveManager.CurrentSave.itemInCharacter;
            if (dict.ContainsKey(itemTypeList[0]))
            {
                var item = database.items.Find(itm => itm.itemName == itemTypeList[0]);
                item?.OnUse(character);

                SaveManager.InventoryIO(item.itemName, -1, false);
                if (dict[itemTypeList[0]] <= 0)
                {
                    dict.Remove(itemTypeList[0]);
                }
            }
        }
    }

    public override void GetItem(Item item)
    {
        SaveManager.InventoryIO(item.itemName, 1, false);
    }
}
