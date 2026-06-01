using UnityEngine;

public class CharacterGridController : MonoBehaviour
{
    private MenuManager manager;
    [SerializeField] private CharacterStatusPanel statusPanel;
    [SerializeField] private SkillManagePanel skillPanel;

    private void Awake()
    {
        manager = transform.root.GetComponent<MenuManager>();

        var paneles = GetComponentsInChildren<CharacterPanelController>();
        foreach (var p in paneles)
        {
            p.Init(this);
        }
    }

    public void RequestOpenStatusMenu(string id)
    {
        statusPanel.Init(id);
        manager.OpenPopup(statusPanel);
    }

    public void RequestOpenSubWeaponMenu(string id)
    {
        Debug.Log(id);
    }

    public void RequestOpenSkillMenu(string id)
    {
        skillPanel.Init(id, this);
        manager.OpenPopup(skillPanel);
    }
}
