using UnityEngine;

/// <summary>
/// 시야 감지 전용 콜라이더를 가진 자식 오브젝트에 부착한다.
/// CharacterSight는 이 컴포넌트가 붙은 콜라이더만 후보로 인식한다.
/// </summary>
[RequireComponent(typeof(CapsuleCollider))]
public class SightTarget : MonoBehaviour
{
    public Character Owner { get; private set; }
    public CapsuleCollider Collider { get; private set; }

    private void Awake()
    {
        Owner    = GetComponentInParent<Character>();
        Collider = GetComponent<CapsuleCollider>();
        gameObject.layer = LayerMask.NameToLayer("SightTarget");
    }
}
