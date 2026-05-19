using UnityEngine;
using UnityEngine.UI;

public class WepponButton : MonoBehaviour
{
    private Weapon weapon;

    public void Init(Weapon weapon)
    {
        this.weapon = weapon;
        GetComponent<Button>().onClick.AddListener(SaveCurrentWeapon);
    }

    public void SaveCurrentWeapon()
    {
        var data = SaveManager.instance.CurrentSave;
        data.equipedWeapon = weapon.id;
        SaveManager.instance.SaveRequest(data);
    }
    
    public void EquipWeapon()
    {

    }
}
