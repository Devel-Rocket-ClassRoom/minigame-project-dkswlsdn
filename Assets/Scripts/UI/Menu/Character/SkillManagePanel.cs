using System;
using UnityEngine;
using UnityEngine.UI;

public class SkillManagePanel : MenuPanel
{
    [SerializeField] private CharacterPanelController panel;

    [Header("Data")]
    [SerializeField] private WeaponDatabase weaponDatabase;

    [Header("Text")]
    [SerializeField] private TextContainer skillDesc;
    [SerializeField] private TextContainer characterOpinion;

    [Header("Skill Buttons (Passive, E, F, Space, L, R, SL, LR)")]
    [SerializeField] private SkillButton[] skillButtons;

    [Header("Control Buttons")]
    [SerializeField] private Button magicConvertButton;     // 마법 전환 버튼
    [SerializeField] private Image magicConvertLockImage;   // 마법 전환 버튼 잠김 이미지
    [SerializeField] private Button tokenOpenButton;        // 토큰 개방 버튼
    [SerializeField] private Button commitButton;           // 커밋 버튼

    private string currentId;
    private Weapon currentWeapon;
    private SkillButton selectedSkill;

    private void OnEnable()
    {
        SaveManager.onSaveModified += Refresh;
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
        currentWeapon = FindWeapon(id);

        // 스킬 버튼 클릭 이벤트 연결 (SkillButton.Init이 중복 방지 처리)
        foreach (var btn in skillButtons)
        {
            btn.Init(OnSkillSelected);
        }

        magicConvertButton.onClick.RemoveAllListeners();
        magicConvertButton.onClick.AddListener(OnMagicConvert);

        // TODO: 토큰 개방 / 커밋 버튼의 클릭 동작 연결
        // tokenOpenButton.onClick.AddListener(...);
        // commitButton.onClick.AddListener(...);

        InitializeSkillButtons();

        // 기본으로 첫 스킬 선택
        if (skillButtons != null && skillButtons.Length > 0)
        {
            OnSkillSelected(skillButtons[0]);
        }
    }

    // 세이브가 변경되면 버튼 상태/선택 텍스트를 다시 갱신
    private void Refresh()
    {
        InitializeSkillButtons();
        if (selectedSkill != null) LoadSkillText();
    }

    // isMagicOpened 상태에 맞춰 마법 전환 버튼 잠금 + 각 스킬버튼을 일반/마법 스킬로 초기화
    private void InitializeSkillButtons()
    {
        var entry = GetEntry();

        // 마법 미개방: 마법 전환 버튼을 잠김 이미지로 막고, 모든 스킬은 일반 스킬로 초기화
        if (!entry.isMagicOpened)
        {
            if (magicConvertLockImage != null) magicConvertLockImage.gameObject.SetActive(true);
            magicConvertButton.interactable = false;

            foreach (var btn in skillButtons)
            {
                btn.SetSkill(GetSkill(btn.SkillKey, false));
            }
            return;
        }

        // 마법 개방: 잠금 완전 해제, magicOpenedSkill에 맞춰 일반/마법 스킬로 초기화
        if (magicConvertLockImage != null) magicConvertLockImage.gameObject.SetActive(false);
        magicConvertButton.interactable = true;

        foreach (var btn in skillButtons)
        {
            bool isMagic = entry.magicOpenedSkill[btn.MagicIndex];
            btn.SetSkill(GetSkill(btn.SkillKey, isMagic));
        }
    }

    private void OnSkillSelected(SkillButton skill)
    {
        selectedSkill = skill;
        LoadSkillText();
    }

    // 선택된 스킬을 마법으로 전환하고 저장 (저장 시 onSaveModified → Refresh로 UI 갱신)
    private void OnMagicConvert()
    {
        if (selectedSkill == null) return;

        var entry = GetEntry();
        if (!entry.isMagicOpened) return; // 마법 미개방 시 방어 (버튼도 비활성)

        entry.magicOpenedSkill[selectedSkill.MagicIndex] = !entry.magicOpenedSkill[selectedSkill.MagicIndex];
        SaveManager.instance.SaveRequest();
    }

    // 스킬 테이블에서 설명과 캐릭터의 의견을 받아와 표시 (마법 전환 상태면 _M 키 사용)
    private void LoadSkillText()
    {
        var entry = GetEntry();
        bool isMagic = entry.isMagicOpened && entry.magicOpenedSkill[selectedSkill.MagicIndex];

        var data = DataTableManager.SkillTable.Get(currentId, selectedSkill.SkillKey, isMagic);

        if (data == null)
        {
            skillDesc.ChangeText(string.Empty);
            characterOpinion.ChangeText(string.Empty);
            return;
        }

        skillDesc.ChangeText(data.description);
        characterOpinion.ChangeText(data.characterOpinion);
    }

    private Skill GetSkill(SkillKey key, bool isMagic)
    {
        return currentWeapon != null ? currentWeapon.GetSkill(key, isMagic) : null;
    }

    private CharacterEntry GetEntry()
    {
        var dict = SaveManager.instance.CurrentSave.characterData;
        if (!dict.TryGetValue(currentId, out CharacterEntry entry))
        {
            throw new Exception("해당 캐릭터의 데이터 없음");
        }
        return entry;
    }

    // 캐릭터 ID(대문자)와 무기 이름(예: Axe)을 대소문자 무시로 매칭
    private Weapon FindWeapon(string id)
    {
        if (weaponDatabase == null || weaponDatabase.weapons == null) return null;
        return weaponDatabase.weapons.Find(
            w => string.Equals(w.weaponName, id, StringComparison.OrdinalIgnoreCase));
    }
}
