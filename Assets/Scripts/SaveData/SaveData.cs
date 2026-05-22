using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class SaveData
{
    public abstract SaveData NextVersion();
}

[Serializable]
public class SaveDataV1 : SaveData
{
    public SaveDataV1()
    {
        version = 1;
        maxInventorySpace = 10;
        maxStorageSpace = 140;
        itemInStorage = new List<ItemSaveEntry>();
        itemInCharacter = new List<ItemSaveEntry>();
        unlockedWeaponList = new List<string>();
    }

    public int version = 1;

    public float playTime;
    public string equipedWeapon;
    public List<string> unlockedWeaponList;
    public List<ItemSaveEntry> itemInStorage;
    public List<ItemSaveEntry> itemInCharacter;
    public int maxStorageSpace;
    public int maxInventorySpace;

    public override SaveData NextVersion()
    {
        throw new NotImplementedException();
    }
}
