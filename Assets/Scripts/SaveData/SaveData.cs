using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class SaveData
{
    public int version;
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

    public float playTime;
    public string equipedWeapon;
    public List<string> unlockedWeaponList;
    public List<ItemSaveEntry> itemInStorage;
    public List<ItemSaveEntry> itemInCharacter;
    public int maxStorageSpace;
    public int maxInventorySpace;

    public override SaveData NextVersion()
    {
        var data = new SaveDataV2();
        return data;
    }
}

public class SaveDataV2 : SaveDataV1
{
    public SaveDataV2()
    {
        isTutorialCleared = false;
        version = 2;
        maxInventorySpace = 10;
        maxStorageSpace = 140;
        itemInStorage = new List<ItemSaveEntry>();
        itemInCharacter = new List<ItemSaveEntry>();
        unlockedWeaponList = new List<string>();
        unlockedWeaponList.Add("BareHand");
    }

    public bool isTutorialCleared;

    public override SaveData NextVersion()
    {
        return new SaveDataV3();
    }
}


public class SaveDataV3 : SaveData
{
    public SaveDataV3()
    {
        version = 3;
        unlockedCharacterList = new List<string>();
        unlockedSubWeaponList = new List<string>();
        itemInStorage = new List<ItemSaveEntry>();
        itemInCharacter = new List<ItemSaveEntry>();
        currentParty = new List<string>();
    }

    public SaveDataV3(List<ItemSaveEntry> IIS, List<ItemSaveEntry> IIC)
    {
        version = 3;
        unlockedCharacterList = new List<string>();
        unlockedSubWeaponList = new List<string>();
        itemInStorage = new List<ItemSaveEntry>(IIS);
        itemInCharacter = new List<ItemSaveEntry>(IIC);
        currentParty = new List<string>();

        maxInventorySpace = 10;
        maxStorageSpace = 140;
    }

    public float playTime;
    public bool isTutorialCleared;

    public string currentCharacterId;

    public List<string> unlockedSubWeaponList;
    public List<string> unlockedCharacterList;
    public List<string> currentParty;

    public List<ItemSaveEntry> itemInStorage;
    public List<ItemSaveEntry> itemInCharacter;

    public int maxStorageSpace;
    public int maxInventorySpace;

    public override SaveData NextVersion()
    {
        throw new NotImplementedException();
    }
}