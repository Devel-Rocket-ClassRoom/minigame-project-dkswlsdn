using System;
using UnityEngine;

public class PlayerSkillExecuter : SkillExecuter
{
    [SerializeField] private WeaponDatabase weaponDatabase;
    public event Action onWeaponChanged;

    private void OnEnable()
    {
        LoadWeapon();
    }

    public void LoadWeapon()
    {
        var character = SaveManager.instance.CurrentSave.currentCharacterId;
        var weapon = DataTableManager.StringTable.Get($"{character}_WEAPON");

        CurrentWeapon = weaponDatabase.weapons.Find((w) => w.weaponName == weapon);
        onWeaponChanged?.Invoke();
    }
}
