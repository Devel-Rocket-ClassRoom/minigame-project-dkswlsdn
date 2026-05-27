using UnityEngine;
using UnityEngine.UI;

public class SetWeaponGrid : MonoBehaviour
{
    [SerializeField] private WeaponDatabase weaponDatabase;
    [SerializeField] private WepponButton weaponButton;
    [SerializeField] private PlayerSkillExecuter executer;


    private void OnEnable()
    {
        var list = SaveManager.instance.CurrentSave.unlockedWeaponList;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        foreach (var weapon in weaponDatabase.weapons)
        {
            var b = Instantiate(weaponButton, transform);
            var isUnlock = list.Contains(weapon.weaponName);
            b.Init(weapon, isUnlock);
        }
    }

    private void OnDisable()
    {
        executer.LoadWeapon();
    }
}
