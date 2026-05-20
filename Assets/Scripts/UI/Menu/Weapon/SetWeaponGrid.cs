using UnityEngine;
using UnityEngine.UI;

public class SetWeaponGrid : MonoBehaviour
{
    [SerializeField] private WeaponDatabase weaponDatabase;
    [SerializeField] private WepponButton weaponButton;

    private void Awake()
    {
        foreach (var weapon in weaponDatabase.weapons)
        {
            var b = Instantiate(weaponButton, transform);
            b.Init(weapon);
        }
    }

    private void OnEnable()
    {
        // 세이브데이터와 비교해서 잠금상태 업데이트
    }
}
