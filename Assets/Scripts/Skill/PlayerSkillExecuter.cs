using System;
using UnityEngine;

public class PlayerSkillExecuter : SkillExecuter
{
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
            CurrentWeapon = DatabaseManager.FindWeapon(weaponName);
        }
        else
        {
            Init(CurrentWeapon);
        }

        onWeaponChanged?.Invoke();
    }

    protected override void Init(Weapon weapon)
    {
        base.Init(weapon);

        // 서브웨폰(Q)은 마법 개방 여부와 무관하게 항상 적용해야 하므로 먼저 처리한다.
        ApplySubWeapon();

        if (string.IsNullOrEmpty(loadedCharacterId)) return;

        var dict = SaveManager.CurrentSave.characterData;
        if (!dict.TryGetValue(loadedCharacterId, out CharacterEntry entry)) return;
        if (!entry.isMagicOpened) return;

        for (int i = 0; i <= (int)SkillKey.Space; i++)
        {
            if (entry.magicOpenedSkill[i])
            {
                skills[i] = weapon.GetSkill((SkillKey)i, true);
            }
        }
    }

    private void ApplySubWeapon()
    {
        const int qIndex = (int)SkillKey.Q;

        string subWeaponId = SaveManager.CurrentSave.selectedSubWeapon;

        skills[qIndex] = string.IsNullOrEmpty(subWeaponId)
            ? null
            : DatabaseManager.FindSubWeapon(subWeaponId);
    }
}
