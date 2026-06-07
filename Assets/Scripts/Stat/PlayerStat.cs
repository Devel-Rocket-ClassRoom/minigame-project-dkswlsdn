using UnityEngine;

// 플레이어 전용 스탯. 세이브 데이터(기본 스탯 + 강화 투자)를 읽어 실제 스탯을 계산한다.
// 적/오브젝트는 베이스 CharacterStat의 인스펙터 직렬화 값을 그대로 사용한다.
public class PlayerStat : CharacterStat
{
    protected override void OnEnable()
    {
        base.OnEnable();
        ReLoad();
        SaveManager.onSaveModified += ReLoad;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        SaveManager.onSaveModified -= ReLoad;
    }

    private void ReLoad()
    {
        // 현재 출전 캐릭터의 테이블+강화 스탯 로드(공식은 CharacterStat에 공유).
        ApplyCharacterStats(SaveManager.CurrentSave.currentCharacterId);
    }
}
