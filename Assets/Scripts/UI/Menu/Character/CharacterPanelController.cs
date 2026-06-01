using UnityEngine;
using UnityEngine.UI;

public class CharacterPanelController : MonoBehaviour
{
    private CharacterGridController controller;

    [SerializeField] private Button statusButton;
    [SerializeField] private Button subWeaponButton;
    [SerializeField] private Button skillButton;
    [SerializeField] private Button deployButton;
    [SerializeField] private Button joinButton;

    [SerializeField] private Image lockImage;
    [SerializeField] private string thisCharacterId;
    private const string daggerCharacter = "DAGGER";
    private const string handgunCharacter = "HANDGUN";
    private const string magicCharacter = "MAGIC";
    private const string axeCharacter = "AXE";

    private const string lockImageKey = "LOCK";


    private void OnEnable()
    {
        var list = SaveManager.instance.CurrentSave.unlockedCharacterList;

        if (list.Contains(thisCharacterId))
        {
            statusButton.onClick.AddListener(OpenStatusPopup);

            if (list.Contains(handgunCharacter))
            {
                subWeaponButton.onClick.AddListener(OpenSubWeaponPopup);
            }
            if (list.Contains(magicCharacter) || list.Contains(daggerCharacter))
            {
                skillButton.onClick.AddListener(OpenSkillPopup);
            }

            deployButton.onClick.AddListener(DeployCharacter);
            joinButton.onClick.AddListener(JoinCharacter);

            lockImage.gameObject.SetActive(false);
        }
        else
        {
            lockImage.gameObject.SetActive(true);
        }
    }

    private void OnDisable()
    {
        statusButton.onClick?.RemoveAllListeners();
        subWeaponButton.onClick?.RemoveAllListeners();
        skillButton.onClick?.RemoveAllListeners();
        deployButton.onClick?.RemoveAllListeners();
        joinButton.onClick?.RemoveAllListeners();
    }

    public void Init(CharacterGridController controller)
    {
        this.controller = controller;
    }

    public void OpenStatusPopup()
    {
        controller.RequestOpenStatusMenu(thisCharacterId);
    }

    public void OpenSubWeaponPopup()
    {
        controller.RequestOpenSubWeaponMenu(thisCharacterId);
    }

    public void OpenSkillPopup()
    {
        controller.RequestOpenSkillMenu(thisCharacterId);
    }

    public void DeployCharacter()
    {
        SaveManager.instance.CurrentSave.currentCharacterId = thisCharacterId;
    }

    public void JoinCharacter()
    {
        if (!SaveManager.instance.CurrentSave.currentParty.Contains(thisCharacterId)) SaveManager.instance.CurrentSave.currentParty.Add(thisCharacterId);
    }
}
