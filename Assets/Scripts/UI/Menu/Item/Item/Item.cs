using System;
using UnityEngine;

public abstract class Item : ScriptableObject
{
    public static event Action onUse;
    public static event Action onGet;

    public string itemName;
    public float weight;

    public bool canUseInBattle;
    public bool canUseInBaseCamp;

    public virtual void OnUse(Character character) { onUse?.Invoke(); }

    public virtual void OnGet(Character character)
    {
        SaveManager.instance.CurrentSave.itemInCharacter.Add(new ItemSaveEntry(itemName, DateTime.Now));
        onGet?.Invoke();
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