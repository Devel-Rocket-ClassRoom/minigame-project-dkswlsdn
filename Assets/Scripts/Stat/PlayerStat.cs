using UnityEngine;

// 플레이어 전용 스탯. 세이브 데이터(기본 스탯 + 강화 투자)를 읽어 실제 스탯을 계산한다.
// 적/오브젝트는 베이스 CharacterStat의 인스펙터 직렬화 값을 그대로 사용한다.
public class PlayerStat : CharacterStat
{
    // 강화 1회당 증가량. 스테이터스 패널(StatusBar) 표시와 동일하게 맞춘다.
    private const float AttackPerPoint = 50f;
    private const float CriticalPerPoint = 30f;
    private const float HealthPerPoint = 1000f;
    private const float DefensePerPoint = 25f;
    private const float DodgyPerPoint = 30f;

    private void OnEnable()
    {
        ReLoad();
        SaveManager.onSaveModified += ReLoad;
    }

    private void OnDisable()
    {
        SaveManager.onSaveModified -= ReLoad;
    }

    private void ReLoad()
    {
        var characterId = SaveManager.CurrentSave.currentCharacterId;
        var dict = SaveManager.CurrentSave.characterData;

        CharacterData originalStat = DataTableManager.CharacterTable.Get(characterId);

        if (!dict.TryGetValue(characterId, out CharacterEntry additinalStat))
        {
            throw new System.Exception("해당 캐릭터의 데이터 없음");
        }

        // 최종 스탯 = 기본값 + (강화당 증가량 * 투자 횟수).
        attack = originalStat.attack + AttackPerPoint * additinalStat.consumedStat[(int)StatType.Attack];
        crit = originalStat.critical + CriticalPerPoint * additinalStat.consumedStat[(int)StatType.Critical];
        maxHealth = originalStat.health + HealthPerPoint * additinalStat.consumedStat[(int)StatType.Health];
        defense = originalStat.defense + DefensePerPoint * additinalStat.consumedStat[(int)StatType.Defense];
        dodgy = originalStat.dodgy + DodgyPerPoint * additinalStat.consumedStat[(int)StatType.Dodgy];

        health = maxHealth;

        RaiseStatChanged();
    }
}
