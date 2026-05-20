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
        var data = SaveManager.instance.LoadRequest(SaveManager.instance.CurrentSlot);
        CurrentWeapon = weaponDatabase.weapons.Find((w) => w.id == data.equipedWeapon);
        onWeaponChanged?.Invoke();
    }
}
