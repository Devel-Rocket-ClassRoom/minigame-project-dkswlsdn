using UnityEngine;

public class GetWeapon : MonoBehaviour
{
    [SerializeField] private Weapon weapon;

    public void UnlockWeapon()
    {
        var list = SaveManager.instance.CurrentSave.unlockedWeaponList;

        if (!list.Contains(weapon.weaponName))
        {
            list.Add(weapon.weaponName);
        }
    }
}
