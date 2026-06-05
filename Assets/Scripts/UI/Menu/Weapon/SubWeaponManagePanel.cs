using UnityEngine;

/// <summary>
/// 서브웨폰 선택 패널. 그리드 셀들은 언록/선택 상태를 표시하고,
/// 이 컨트롤러가 실제 선택(세이브의 selectedSubWeapon)을 처리한다.
/// </summary>
public class SubWeaponManagePanel : MenuPanel
{
    [SerializeField] private CharacterPanelController panel;

    [Header("Sub Weapon Grids")]
    [SerializeField] private CharacterSubWeaponGrid[] grids;
    [SerializeField] private TextContainer p;
    [SerializeField] private TextContainer desc;

    private string currentId;

    private void OnEnable()
    {
        SaveManager.onSaveModified += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        SaveManager.onSaveModified -= Refresh;
    }

    public void Init(string id, CharacterGridController controller)
    {
        panel.Init(controller);
        panel.Init(id);
        currentId = id;

        Refresh();
    }

    private void Refresh()
    {
        string selected = SaveManager.CurrentSave.selectedSubWeapon;

        foreach (var grid in grids)
        {
            grid.RefreshLock();
            grid.SetSelected(grid.SubWeaponId == selected);
        }

        p.ChangeText($"{selected}_P");
        desc.ChangeText($"{selected}_DESC");
    }
}
