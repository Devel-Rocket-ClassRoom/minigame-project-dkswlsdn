using UnityEngine;
using UnityEngine.UI;

public class WepponButton : MonoBehaviour
{
    public static string selected { get; private set; }

    private Weapon weapon;

    public void Init(Weapon weapon)
    {
        this.weapon = weapon;
        GetComponent<Button>().onClick.AddListener(SaveCurrentWeapon);
        GetComponent<Image>().sprite = weapon.icon;
        GetComponent<TextContainer>().ChangeText(weapon.weaponName);
    }

    public void SaveCurrentWeapon()
    {
        var data = SaveManager.instance.CurrentSave;
        data.equipedWeapon = weapon.weaponName;
        SaveManager.instance.SaveRequest();
        selected = data.equipedWeapon;
    }
}
