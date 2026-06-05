using UnityEngine;

public class CharacterGridController : MonoBehaviour
{
    [SerializeField] private CharacterStatusPanel statusPanel;
    [SerializeField] private SubWeaponManagePanel subWeaponPanel;
    [SerializeField] private SkillManagePanel skillPanel;

    private void Awake()
    {
        var paneles = GetComponentsInChildren<CharacterPanelController>();
        foreach (var p in paneles)
        {
            p.Init(this);
        }
    }

    public void RequestOpenStatusMenu(string id)
    {
        statusPanel.Init(id);
        MenuManager.instance.OpenPopup(statusPanel);
    }

    public void RequestOpenSubWeaponMenu(string id)
    {
        subWeaponPanel.Init(id, this);
        MenuManager.instance.OpenPopup(subWeaponPanel);
    }

    public void RequestOpenSkillMenu(string id)
    {
        skillPanel.Init(id, this);
        MenuManager.instance.OpenPopup(skillPanel);
    }
}
