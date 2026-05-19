using UnityEngine;
using UnityEngine.UI;

public class SetWeaponGrid : MonoBehaviour
{
    [SerializeField] private WeaponDatabase weaponDatabase;
    [SerializeField] private Button weaponButton;

    private void Awake()
    {
        foreach (var weapon in weaponDatabase.weapons)
        {
            Instantiate(weaponButton, transform);
        }
    }

    private void OnEnable()
    {
        // 세이브데이터와 비교
    }
}
