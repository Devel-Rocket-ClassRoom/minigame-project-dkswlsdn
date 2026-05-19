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
    public int version = 1;

    public float playTime;
    public string equipedWeapon;
    public List<string> unlockedWeaponList;

    public override SaveData NextVersion()
    {
        throw new NotImplementedException();
    }
}
