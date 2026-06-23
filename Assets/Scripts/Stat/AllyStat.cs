using UnityEngine;

// AI 아군 전용 스탯. 스폰 시 지정된 characterId의 테이블+강화 스탯을 로드한다(플레이어와 동일 공식).
// PlayerStat과 달리 onSaveModified를 구독하지 않고, Initialize로 한 번만 세팅한다.
public class AllyStat : CharacterStat
{
    private string characterId;

    public void Initialize(string characterId)
    {
        this.characterId = characterId;
        ApplyCharacterStats(characterId);
    }
}
