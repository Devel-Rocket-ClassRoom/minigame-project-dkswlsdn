using UnityEngine;
using UnityEngine.UI;

public class WepponButton : MonoBehaviour
{
    private Weapon weapon;

    public void Init(Weapon weapon)
    {
        this.weapon = weapon;
        GetComponent<Button>().onClick.AddListener(SaveCurrentWeapon);
        GetComponent<Image>().sprite = weapon.icon;
        GetComponent<TextContainer>().ChangeText(weapon.name);
    }

    public void SaveCurrentWeapon()
    {
        var data = SaveManager.instance.CurrentSave;
        data.equipedWeapon = weapon.weaponName;
        SaveManager.instance.SaveRequest(data);
    }
    
    public void EquipWeapon()
    {

    }
}
