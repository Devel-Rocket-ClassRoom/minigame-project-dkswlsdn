using UnityEngine;

/// <summary>
/// 시야 콜라이더를 SightRange 스탯에 맞게 크기/위치를 조정한다.
/// 캐릭터 감지는 CharacterSight(PlayerSight)가 담당한다.
/// </summary>
public class SightManager : MonoBehaviour
{
    private CharacterStat stat;
    private CapsuleCollider sightCollider;

    [SerializeField] private bool autoCenter = true;

    private void Awake()
    {
        stat = GetComponentInParent<CharacterStat>();
        sightCollider = GetComponent<CapsuleCollider>();
        sightCollider.isTrigger = true;

        stat.onStatChanged += ApplySightRange;
        ApplySightRange();
    }

    private void ApplySightRange()
    {
        float radius = stat.SightRange * 0.5f + 1.5f;
        sightCollider.radius = autoCenter ? radius : stat.SightRange;
        sightCollider.center = autoCenter
            ? new Vector3(0, 0, radius - 3f)
            : Vector3.zero;
    }
}
