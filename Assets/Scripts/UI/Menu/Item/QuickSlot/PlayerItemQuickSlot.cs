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
            var list = SaveManager.instance.CurrentSave.itemInCharacter;
            var selected = list.Find(entry => entry.itemName == itemTypeList[0]);
            if (selected != null)
            {
                var item = database.items.Find(itm => itm.itemName == itemTypeList[0]);
                item?.OnUse(character);
                list.Remove(selected);
            }
        }
    }

    public override void GetItem(Item item)
    {
        SaveManager.instance.CurrentSave.itemInCharacter.Add(new ItemSaveEntry(item.itemName, DateTime.Now));
    }
}
