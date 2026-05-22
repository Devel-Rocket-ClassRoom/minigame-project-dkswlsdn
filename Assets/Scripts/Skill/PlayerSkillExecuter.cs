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
        SaveManager.instance.LoadRequest(SaveManager.instance.CurrentSlot);
        CurrentWeapon = weaponDatabase.weapons.Find((w) => w.weaponName == SaveManager.instance.CurrentSave.equipedWeapon);
        onWeaponChanged?.Invoke();
    }
}
