using System;
using System.Collections.Generic;
using UnityEngine;

public class SightManagerTest : MonoBehaviour
{
    private Character character;
    private CharacterStat stat;
    private CapsuleCollider sightCollider;
    private Collider observerBody;   // 관찰자(이 캐릭터)의 몸 콜라이더

    [SerializeField] private bool autoCenter = true;

    [Header("Line of Sight")]
    [Tooltip("시야를 가리는 장애물 레이어(벽 등). 반드시 지정해야 가림 판정이 동작.")]
    [SerializeField] private LayerMask obstacleMask;

    [Tooltip("시야 재검사 간격(초). 0이면 매 프레임. 0.1 권장.")]
    [SerializeField] private float losCheckInterval = 0.1f;

    [Tooltip("부분 가림 판정용 수직 샘플 위치 비율(콜라이더 높이 기준)")]
    [Range(0f, 1f)]
    [SerializeField] private float verticalSampleRatio = 0.9f;

    [Tooltip("관찰자 '눈' 높이 비율(몸 콜라이더 중심→위 방향). LOS 시작점.")]
    [Range(0f, 1f)]
    [SerializeField] private float eyeHeightRatio = 0.7f;

    // 후보: 시야 콜라이더(범위) 안에 들어온 적
    private readonly HashSet<Character> candidates = new();
    // 후보가 트리거에 넣은 콜라이더(샘플 bounds 계산용)
    private readonly Dictionary<Character, Collider> candidateColliders = new();

    // 실제 가시: 범위 내 + 시야 확보(벽에 안 막힌 직선 존재)
    public HashSet<Character> visibleCharacters { get; private set; } = new HashSet<Character>();
    public Character FirstEncounter { get; private set; }

    public event Action<Character> onDetected;
    public event Action<Character> onLost;

    private float nextCheckTime;

    // 샘플 점 버퍼(할당 회피)
    private static readonly List<Vector3> targetPoints = new();

    private void Awake()
    {
        character = GetComponentInParent<Character>();
        stat = GetComponentInParent<CharacterStat>();
        sightCollider = GetComponent<CapsuleCollider>();
        sightCollider.isTrigger = true;
        observerBody = character != null ? character.GetComponent<Collider>() : null;

        stat.onStatChanged += ApplySightRange;
        ApplySightRange();

        // 프레임 분산: 첫 검사 시점을 인스턴스마다 무작위로 어긋나게 해
        // 모든 적이 같은 프레임에 LOS 검사를 몰아 하지 않도록 한다.
        nextCheckTime = Time.time + UnityEngine.Random.Range(0f, Mathf.Max(0.0001f, losCheckInterval));
    }

    private void ApplySightRange()
    {
        float radius = stat.SightRange * 0.5f + 1.5f;
        sightCollider.radius = autoCenter ? radius : stat.SightRange;
        sightCollider.center = autoCenter
            ? new Vector3(0, 0, radius - 3f)
            : Vector3.zero;
    }

    // ── 범위(트리거) 판정: 후보 등록/해제 ─────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        var target = other.GetComponent<Character>();
        if (target == null || target.team == character.team) return;

        candidates.Add(target);
        candidateColliders[target] = other;

        // 들어온 즉시 한 번 검사해서 반응 지연을 줄임
        Evaluate(target, other);
    }

    private void OnTriggerExit(Collider other)
    {
        var target = other.GetComponent<Character>();
        if (target == null) return;

        candidates.Remove(target);
        candidateColliders.Remove(target);

        // 범위를 벗어났으면 가시였든 아니든 감지 해제
        if (visibleCharacters.Remove(target))
        {
            FixFirstEncounter(target);
            onLost?.Invoke(target);
        }
    }

    // ── 주기적 시야 재검사 ────────────────────────────────────────
    private void Update()
    {
        if (Time.time < nextCheckTime) return;
        nextCheckTime = Time.time + losCheckInterval;

        foreach (var target in candidates)
        {
            if (target == null) continue;
            candidateColliders.TryGetValue(target, out var col);
            Evaluate(target, col);
        }
    }

    // 후보 한 명의 가시 여부를 갱신하고 이벤트를 발생시킨다.
    private void Evaluate(Character target, Collider targetCol)
    {
        bool canSee = HasLineOfSight(targetCol);
        bool wasVisible = visibleCharacters.Contains(target);

        if (canSee && !wasVisible)
        {
            if (visibleCharacters.Count == 0)
                FirstEncounter = target;

            visibleCharacters.Add(target);
            onDetected?.Invoke(target);
        }
        else if (!canSee && wasVisible)
        {
            visibleCharacters.Remove(target);
            FixFirstEncounter(target);
            onLost?.Invoke(target);
        }
    }

    // ── 조건 1·2 핵심: 관찰자 눈에서 대상 콜라이더의 여러 점으로 쏴서
    //    벽에 안 막힌 직선이 하나라도 있으면 true ──
    private bool HasLineOfSight(Collider targetCol)
    {
        if (targetCol == null || observerBody == null) return false;

        // 관찰자는 눈 1점만 사용(성능). 부분 가림은 대상 쪽 샘플로 잡는다.
        Bounds ob = observerBody.bounds;
        Vector3 eye = ob.center + Vector3.up * (ob.extents.y * eyeHeightRatio);

        // targetPoints[0]은 중심 → 자연스럽게 '중심 사전컷' 역할(대부분 여기서 끝남)
        BuildSamplePoints(targetCol, targetPoints);

        for (int j = 0; j < targetPoints.Count; j++)
        {
            // 벽에 안 막힌(=Linecast가 아무것도 안 맞은) 직선이 하나라도 있으면 보임
            if (!Physics.Linecast(eye, targetPoints[j],
                                  obstacleMask, QueryTriggerInteraction.Ignore))
                return true;
        }
        return false; // 모든 직선이 벽에 막힘 → 조건 1
    }

    // 콜라이더 bounds 기준 샘플 점(중심 + 좌우/앞뒤/상하 끝).
    // 부분 가림(조건 2)을 잡기 위해 가장자리까지 포함한다.
    private void BuildSamplePoints(Collider col, List<Vector3> buf)
    {
        buf.Clear();
        Bounds b = col.bounds;
        Vector3 c = b.center;
        Vector3 e = b.extents;
        float vy = e.y * verticalSampleRatio;

        buf.Add(c);
        buf.Add(c + new Vector3(e.x, 0f, 0f));
        buf.Add(c + new Vector3(-e.x, 0f, 0f));
        buf.Add(c + new Vector3(0f, 0f, e.z));
        buf.Add(c + new Vector3(0f, 0f, -e.z));
        buf.Add(c + new Vector3(0f, vy, 0f));
        buf.Add(c + new Vector3(0f, -vy, 0f));
    }

    // 제거된 대상이 FirstEncounter였다면 갱신
    private void FixFirstEncounter(Character removed)
    {
        if (FirstEncounter != removed) return;

        FirstEncounter = null;
        foreach (var c in visibleCharacters)
        {
            FirstEncounter = c;
            break;
        }
    }
}
