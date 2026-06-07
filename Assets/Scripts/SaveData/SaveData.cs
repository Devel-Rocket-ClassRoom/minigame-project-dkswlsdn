using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class SaveData
{
    public int version;
    public static readonly string[] implementedCharacter =
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
        return data;
    }
}


public class SaveDataV3 : SaveData
{
    public SaveDataV3()
    {
        version = 3;

        // null로 두어야 Json.NET이 역직렬화 시 기존 리스트에 추가하지 않고 교체함
        // 새 게임일 때만 아래 Init()을 호출해서 초기값을 채울 것
        unlockedCharacterList = null;
        unlockedSubWeaponList = null;
        characterData = null;
        itemInStorage = null;
        itemInCharacter = null;
        currentParty = null;
    }

    // 새 게임 시작 시 호출
    public SaveDataV3 Init()
    {
        unlockedCharacterList = new List<string> { implementedCharacter[0] };
        unlockedSubWeaponList = new List<string>();
        selectedSubWeapon = string.Empty;

        characterData = new Dictionary<string, CharacterEntry>();
        foreach (var id in implementedCharacter)
            characterData.Add(id, new CharacterEntry());

        itemInStorage = new();
        itemInCharacter = new();
        currentParty = new List<string>();

        currentCharacterId = implementedCharacter[0];
        currentParty.Add(currentCharacterId); // 현재 캐릭터는 항상 파티 일원

        maxInventorySpace = 10;
        maxStorageSpace = 140;

        return this;
    }

    public float playTime;
    public bool isTutorialCleared;

    public string currentCharacterId;

    public List<string> unlockedSubWeaponList;
    public List<string> unlockedCharacterList;

    public string selectedSubWeapon;
    public List<string> currentParty;

    public Dictionary<string, CharacterEntry> characterData;

    public Dictionary<string, int> itemInStorage;
    public Dictionary<string, int> itemInCharacter;

    public int maxStorageSpace;
    public int maxInventorySpace;
    public int anchCount;

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

public enum ImplementedCharacter
{
    BardHand, Axe, Dagger, Handgun, Magic
}