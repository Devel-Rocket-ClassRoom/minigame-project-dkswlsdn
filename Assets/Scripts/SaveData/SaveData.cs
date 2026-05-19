using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveDataV1
{
    public float PlayTime;
    public Weapon EquipedWeapon;
    public Dictionary<int, bool> UnlockedWeaponList;
}
