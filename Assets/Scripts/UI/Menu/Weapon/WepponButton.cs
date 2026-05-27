using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WepponButton : MonoBehaviour
{
    public static string selected { get; private set; }
    [SerializeField] Image backGround;
    [SerializeField] Image icon;
    [SerializeField] TextContainer container;

    private Weapon weapon;
    private bool isUnlock;

    public void Init(Weapon weapon, bool isUnlock)
    {
        this.weapon = weapon;
        this.isUnlock = isUnlock;
        GetComponent<Button>().onClick.AddListener(SaveCurrentWeapon);
        icon.sprite = weapon.icon;
        container.ChangeText(weapon.weaponName);
        if (!isUnlock)
        {
            var color = new Color(0.5f, 0.5f, 0.5f);
            backGround.color = color;
        }
    }

    public void SaveCurrentWeapon()
    {
        if (!isUnlock) return;

        var data = SaveManager.instance.CurrentSave;
        data.equipedWeapon = weapon.weaponName;
        SaveManager.instance.SaveRequest();
        selected = data.equipedWeapon;
    }
}
