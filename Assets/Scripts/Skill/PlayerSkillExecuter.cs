using System;
using UnityEngine;

public class PlayerSkillExecuter : SkillExecuter
{
    [SerializeField] private WeaponDatabase weaponDatabase;
    public event Action onWeaponChanged;

    private string loadedCharacterId;

    private void OnEnable()
    {
        LoadWeapon();
        SaveManager.onSaveModified += LoadWeapon;
    }

    private void OnDisable()
    {
        SaveManager.onSaveModified -= LoadWeapon;
    }

    public void LoadWeapon()
    {
        var character = SaveManager.CurrentSave.currentCharacterId;

        if (character != loadedCharacterId)
        {
            // 캐릭터(무기)가 바뀐 경우: 무기를 새로 로드 (setter가 Init 호출)
            loadedCharacterId = character;
            var weaponName = DataTableManager.StringTable.Get($"{character}_WEAPON");
            CurrentWeapon = weaponDatabase.weapons.Find((w) => w.weaponName == weaponName);
        }
        else
        {
            // 무기는 그대로지만 마법 전환 등 세이브 변경을 즉시 반영
            Init(CurrentWeapon);
        }

        onWeaponChanged?.Invoke();
    }

    // 일반 스킬로 채운 뒤, 세이브의 magicOpenedSkill이 켜진 슬롯만 마법 전환 스킬로 교체
    protected override void Init(Weapon weapon)
    {
        base.Init(weapon);

        if (string.IsNullOrEmpty(loadedCharacterId)) return;

        var dict = SaveManager.CurrentSave.characterData;
        if (!dict.TryGetValue(loadedCharacterId, out CharacterEntry entry)) return;
        if (!entry.isMagicOpened) return;

        // skills[i] 와 magicOpenedSkill[i] 가 1:1 (i = (int)SkillKey).
        // skills 는 7칸이라 Passive(7)는 자동 제외된다.
        for (int i = 0; i < skills.Length; i++)
        {
            if (entry.magicOpenedSkill[i])
            {
                skills[i] = weapon.GetSkill((SkillKey)i, true);
            }
        }
    }
}
