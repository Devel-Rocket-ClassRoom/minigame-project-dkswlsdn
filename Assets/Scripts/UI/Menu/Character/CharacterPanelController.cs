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

    [SerializeField] private TextContainer deployText; // 출전/출전중 라벨
    [SerializeField] private TextContainer joinText;   // 합류/이탈 라벨

    [SerializeField] private Image lockImage;
    [SerializeField] private string thisCharacterId;
    private const string daggerCharacter = "DAGGER";
    private const string handgunCharacter = "HANDGUN";
    private const string magicCharacter = "MAGIC";
    private const string axeCharacter = "AXE";

    private const string lockImageKey = "LOCK";
    private const int MaxPartyCount = 3;

    // 버튼 라벨용 StringTable 키
    private const string deployKey       = "DEPLOY";        // 출전
    private const string deployActiveKey = "DEPLOY_ACTIVE"; // 출전중
    private const string joinKey         = "JOIN";          // 합류
    private const string leaveKey        = "LEAVE";         // 이탈


    private void OnEnable()
    {
        Load();

        SaveManager.onSaveModified += ReLoad;
    }

    private void OnDisable()
    {
        Exit();

        SaveManager.onSaveModified -= ReLoad;
    }

    public void Init(string id)
    {
        thisCharacterId = id;
    }

    private void ReLoad()
    {
        Exit();
        Load();
    }

    private void Load()
    {
        var list = SaveManager.CurrentSave.unlockedCharacterList;

        if (list.Contains(thisCharacterId))
        {
            statusButton.onClick.AddListener(OpenStatusPopup);
            subWeaponButton.onClick.AddListener(OpenSubWeaponPopup);
            skillButton.onClick.AddListener(OpenSkillPopup);
            deployButton.onClick.AddListener(DeployCharacter);
            joinButton.onClick.AddListener(JoinCharacter);

            UpdateButtons();

            lockImage.gameObject.SetActive(false);
        }
        else
        {
            lockImage.gameObject.SetActive(true);
        }
    }

    private void Exit()
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

    // deploy: 현재 캐릭터를 파티 0번 슬롯에 둔다(0번 = 항상 조작 중인 캐릭터).
    //  - 이미 파티원: 해당 인덱스와 0번을 swap
    //  - 비파티원:   0번을 덮어씀(기존 0번 캐릭터는 파티에서 빠짐), 빈 파티면 추가
    // 이로써 현재 캐릭터는 항상 파티원이라 목숨/carry 계산에 포함된다.
    public void DeployCharacter()
    {
        SaveManager.CurrentSave.currentCharacterId = thisCharacterId;

        var party = SaveManager.CurrentSave.currentParty;
        int i = party.IndexOf(thisCharacterId);
        if (i >= 0)
        {
            var tmp = party[0];
            party[0] = party[i];
            party[i] = tmp;
        }
        else if (party.Count == 0)
        {
            party.Add(thisCharacterId);
        }
        else
        {
            party[0] = thisCharacterId;
        }

        SaveManager.SaveRequest();
    }

    // 파티 가입/해제 토글. 파티에 있으면 해제, 없으면 추가(최대 인원 미만일 때만).
    // 단, 현재 deploy된 캐릭터(0번)는 항상 파티원이어야 하므로 해제 불가.
    public void JoinCharacter()
    {
        var party = SaveManager.CurrentSave.currentParty;
        if (party.Contains(thisCharacterId))
        {
            if (thisCharacterId == SaveManager.CurrentSave.currentCharacterId) return; // 현재 캐릭터는 해제 불가
            party.Remove(thisCharacterId);            // 토글 OFF: 파티에서 해제
        }
        else
        {
            if (party.Count >= MaxPartyCount) return; // 가득 차면 가입 불가(버튼도 비활성이지만 안전망)
            party.Add(thisCharacterId);               // 토글 ON: 파티 합류
        }
        SaveManager.SaveRequest();
    }

    // deploy/join 버튼의 활성 상태 + 라벨 재평가(onSaveModified→ReLoad→Load 경로에서 매번 호출).
    //  출전: 현재 캐릭터면 '출전중' + 비활성, 아니면 '출전' + 활성
    //  합류/이탈: 파티에 있으면 '이탈', 없으면 '합류'.
    //            현재 캐릭터는 이탈 불가(비활성), 비파티원은 자리가 남았을 때만(파티 < 최대) 활성
    private void UpdateButtons()
    {
        var save = SaveManager.CurrentSave;
        var party = save.currentParty;
        bool inParty = party.Contains(thisCharacterId);
        bool isCurrent = thisCharacterId == save.currentCharacterId;

        deployButton.interactable = !isCurrent;
        deployText?.ChangeText(isCurrent ? deployActiveKey : deployKey);

        joinButton.interactable = !isCurrent && (inParty || party.Count < MaxPartyCount);
        joinText?.ChangeText(inParty ? leaveKey : joinKey);
    }
}
