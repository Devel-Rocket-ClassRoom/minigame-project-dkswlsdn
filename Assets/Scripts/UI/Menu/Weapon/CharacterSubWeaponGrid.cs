using UnityEngine;

/// <summary>
/// 서브웨폰 그리드 셀. 직렬화된 서브웨폰 id의 언록 여부에 따라 lockImage를 토글하고,
/// 클릭 시 세이브의 selectedSubWeapon을 교체한다(이미 선택된 걸 누르면 해제).
/// 선택 여부 표시는 toggleButton으로 한다(패널의 Refresh가 갱신).
/// </summary>
public class CharacterSubWeaponGrid : MonoBehaviour
{
    [SerializeField] private string subWeaponId;
    [SerializeField] private GameObject lockImage;
    [SerializeField] private ToggleButton toggleButton;

    public string SubWeaponId => subWeaponId;

    public bool IsUnlocked
    {
        get
        {
            var unlocked = SaveManager.CurrentSave.unlockedSubWeaponList;
            return unlocked != null && unlocked.Contains(subWeaponId);
        }
    }

    private void Awake()
    {
        // 클릭 등록은 패널 Init에 의존하지 않도록 여기서 직접 한다.
        if (toggleButton != null)
            toggleButton.Button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (!IsUnlocked) return;

        var save = SaveManager.CurrentSave;
        // 이미 선택된 걸 다시 누르면 해제, 아니면 이 서브웨폰으로 교체
        save.selectedSubWeapon = (save.selectedSubWeapon == subWeaponId)
            ? string.Empty
            : subWeaponId;

        SaveManager.SaveRequest();
    }

    // 언록 상태에 맞춰 잠금 이미지 갱신
    public void RefreshLock()
    {
        if (lockImage != null)
            lockImage.SetActive(!IsUnlocked);
    }

    // 현재 선택 여부 표시 (패널 Refresh가 호출)
    public void SetSelected(bool selected)
    {
        if (toggleButton != null)
            toggleButton.SetState(!selected);
    }
}
