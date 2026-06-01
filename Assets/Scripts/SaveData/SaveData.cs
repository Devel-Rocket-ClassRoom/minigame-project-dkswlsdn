using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class SaveData
{
    public int version;
    public readonly string[] implementedCharacter =
    {
        "BAREHAND",
        "AXE",
        "DAGGER",
        "HANDGUN",
        "MAGIC"
    };
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
        unlockedWeaponList.Add(implementedCharacter[0]);
    }

    public bool isTutorialCleared;

    public override SaveData NextVersion()
    {
        var data = new SaveDataV3();
        data.itemInStorage = itemInStorage;
        data.itemInCharacter = itemInCharacter;
        return data;
    }
}


public class SaveDataV3 : SaveData
{
    public SaveDataV3()
    {
        version = 3;
        unlockedCharacterList = new List<string>();
        unlockedSubWeaponList = new List<string>();

        characterData = new Dictionary<string, CharacterEntry>();
        foreach (var id in implementedCharacter)
        {
            characterData.Add(id, new CharacterEntry());
        }

        itemInStorage = new List<ItemSaveEntry>();
        itemInCharacter = new List<ItemSaveEntry>();
        currentParty = new List<string>();

        unlockedCharacterList.Add(implementedCharacter[0]);
        currentCharacterId = implementedCharacter[0];

        maxInventorySpace = 10;
        maxStorageSpace = 140;
    }

    public float playTime;
    public bool isTutorialCleared;

    public string currentCharacterId;

    public List<string> unlockedSubWeaponList;
    public List<string> unlockedCharacterList;
    public List<string> currentParty;

    public Dictionary<string, CharacterEntry> characterData;

    public List<ItemSaveEntry> itemInStorage;
    public List<ItemSaveEntry> itemInCharacter;

    public int maxStorageSpace;
    public int maxInventorySpace;

    public override SaveData NextVersion()
    {
        throw new NotImplementedException();
    }
}

public class CharacterEntry
{
    public CharacterEntry()
    {
        consumedStat = new int[6];
        consumedToken = new int[8];
        magicOpenedSkill = new bool[8];
        isMagicOpened = false;
    }

    // Newtonsoft.Json은 readonly 배열 필드를 역직렬화하지 못한다(로드 시 기본값으로 초기화됨).
    // 저장/로드를 위해 readonly를 두면 안 된다.
    public int[] consumedStat;
    public int[] consumedToken;
    public bool[] magicOpenedSkill;
    public bool isMagicOpened;
}