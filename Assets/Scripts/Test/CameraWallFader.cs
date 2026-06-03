using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 카메라 자식(=실제 Camera 오브젝트)에 붙인다.
/// 카메라에서 캐릭터 방향으로 뻗은 캡슐 트리거로
///  (1) 카메라가 벽에 파묻힌 경우
///  (2) 카메라와 캐릭터 사이에 벽이 있는 경우
/// 두 가지를 모두 감지해서, 해당 벽을 반투명(양면)으로 페이드시킨다.
///
/// 필요 조건
///  - 벽 콜라이더에 "Wall" 태그
///  - 벽 머티리얼은 URP Lit 계열 (Render Face 설정은 안 해도 됨 — 스크립트가 강제로 양면 처리)
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CameraWallFader : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("벽으로 취급할 태그")]
    [SerializeField] private string wallTag = "Wall";

    [Tooltip("카메라가 바라보는 대상(캐릭터/LookAt). 캡슐 길이를 자동으로 여기까지 맞춘다.")]
    [SerializeField] private Transform aimTarget;

    [Tooltip("캡슐 반지름. 크면 비껴있는 벽까지 페이드(어지러움), 작으면 살짝 걸친 벽 놓침.")]
    [SerializeField] private float capsuleRadius = 0.4f;

    [Tooltip("캡슐 길이에 더해줄 여유분")]
    [SerializeField] private float lengthPadding = 0.3f;

    [Header("Fade")]
    [Tooltip("가려졌을 때의 알파(0=완전투명, 1=불투명)")]
    [Range(0f, 1f)]
    [SerializeField] private float fadedAlpha = 0.35f;

    [Tooltip("페이드 속도(초당 알파 변화량)")]
    [SerializeField] private float fadeSpeed = 8f;

    [Tooltip("Rigidbody/CapsuleCollider를 자동으로 추가·설정")]
    [SerializeField] private bool autoSetup = true;

    private CapsuleCollider capsule;
    private readonly Dictionary<Collider, WallFadeTarget> active = new();

    private void Awake()
    {
        if (autoSetup) Setup();
    }

    private void Setup()
    {
        // 정적 콜라이더(벽)와 트리거 이벤트를 주고받으려면 한쪽에 Rigidbody가 필요하다.
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        capsule = GetComponent<CapsuleCollider>();
        if (capsule == null) capsule = gameObject.AddComponent<CapsuleCollider>();
        capsule.isTrigger = true;
        capsule.direction = 2; // Z축 = 카메라 정면(LookAt 방향)
        capsule.radius = capsuleRadius;
        ResizeCapsule();
    }

    private void ResizeCapsule()
    {
        if (capsule == null) return;

        float dist = aimTarget != null
            ? Vector3.Distance(transform.position, aimTarget.position)
            : Mathf.Max(capsule.height, 1f);

        capsule.height = dist + lengthPadding;
        // 카메라(z=0)에서 캐릭터(z=dist) 사이를 덮도록 중심을 정면으로 민다.
        capsule.center = new Vector3(0f, 0f, dist * 0.5f);
    }

    private void LateUpdate()
    {
        // 카메라~캐릭터 거리는 거의 고정이라 사실상 변화 없지만,
        // distance 값이 바뀌어도 따라가도록 갱신해둔다.
        if (aimTarget != null) ResizeCapsule();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(wallTag)) return;
        if (active.ContainsKey(other)) return;

        var target = other.GetComponent<WallFadeTarget>();
        if (target == null) target = other.gameObject.AddComponent<WallFadeTarget>();

        target.Configure(fadedAlpha, fadeSpeed);
        target.SetHidden(true);
        active.Add(other, target);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!active.TryGetValue(other, out var target)) return;
        if (target != null) target.SetHidden(false);
        active.Remove(other);
    }
}
