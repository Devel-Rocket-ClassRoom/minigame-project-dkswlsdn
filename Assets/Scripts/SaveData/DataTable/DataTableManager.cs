using UnityEngine;

public class DataTableManager : MonoBehaviour
{
    private const string iconFormatPath = "Icons/Weapons/Icon/{0}";

    // Weapon SO
    public string id; // 이미 있음, 추가 없음

    // 로드
    
    public static Sprite GetIcon (string iconName)
    {
        return Resources.Load<Sprite>(string.Format(iconFormatPath, iconName));
    }
}
