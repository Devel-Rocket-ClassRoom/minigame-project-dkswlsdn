using System;
using UnityEngine;

public abstract class Item : ScriptableObject
{
    public string itemName;
    public string id;

    public bool canUseInBattle;
    public bool canUseInBaseCamp;

    public abstract void OnUse(Character character);

    public virtual void OnGet(Character character)
    {
        SaveManager.instance.CurrentSave.itemInCharacter.Add(new ItemSaveEntry(itemName, DateTime.Now));
    }
}

[Serializable]
public class ItemSaveEntry
{
    public ItemSaveEntry(string name, DateTime time)
    {
        itemName = name;
        getDate = time;
    }

    public string itemName;
    public DateTime getDate;
}