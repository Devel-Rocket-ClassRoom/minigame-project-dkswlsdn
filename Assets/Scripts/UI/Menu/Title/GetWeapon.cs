using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetWeapon : MonoBehaviour
{
    [SerializeField] private Weapon weapon;
    [SerializeField] private PlayerSkillExecuter executer;
    [SerializeField] private List<Interactor> weaponInstance;

    private void Awake()
    {
        var b = GetComponent<Button>();
        b.onClick.AddListener(UnlockWeapon);
        b.onClick.AddListener(DeleteAllWeapon);
        b.onClick.AddListener(transform.root.GetComponent<MenuManager>().CloseMenu);
        b.onClick.AddListener(EquipWeapon);
    }

    public void UnlockWeapon()
    {
        var list = SaveManager.instance.CurrentSave.unlockedWeaponList;

        if (!list.Contains(weapon.weaponName))
        {
            list.Add(weapon.weaponName);
        }
    }

    public void DeleteAllWeapon()
    {
        foreach (var weapon in weaponInstance)
        {
            weapon.gameObject.SetActive(false);
        }
    }

    public void EquipWeapon()
    {
        SaveManager.instance.CurrentSave.equipedWeapon = weapon.weaponName;
        executer.LoadWeapon();
    }
}
