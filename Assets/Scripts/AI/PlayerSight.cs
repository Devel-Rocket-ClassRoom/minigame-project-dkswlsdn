using UnityEngine;

/// <summary>
/// 플레이어 전용 시야. 가시성 판정은 base(CharacterSight)가 수행하고,
/// 적 렌더러 on/off는 TeamManager가 팀 전체(플레이어 + 아군) 시야를 합산해 처리한다.
/// 따라서 아군이 본 적도 플레이어 화면에 함께 보인다(시야 공유).
/// </summary>
public class PlayerSight : CharacterSight
{
}
