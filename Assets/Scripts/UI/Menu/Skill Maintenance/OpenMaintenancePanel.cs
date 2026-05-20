using UnityEngine;

public class OpenMaintenancePanel : MonoBehaviour
{
    [SerializeField] private WeaponDatabase WeaponDatabase;

    public void Open()
    {
        var weppon = WeaponDatabase.weapons.Find((w) => w.weaponName == SaveManager.instance.CurrentSave.equipedWeapon);

        Init(weppon);
    }

    private void Init(Weapon weapon)
    {

    }
}
