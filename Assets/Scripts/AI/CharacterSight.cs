using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 트리거 콜라이더에 들어온 적 팀 캐릭터를 후보에 올리고,
/// 매 프레임 7개 지점(중심 + 상단 4모서리 + 하단 2모서리)에 레이를 쏴
/// 하나라도 벽에 막히지 않으면 '보이는' 캐릭터로 판단한다.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CharacterSight : MonoBehaviour
{
    private Transform eyePoint;
    [SerializeField] private LayerMask wallMask;    // 시야를 막는 레이어

    protected Character owner;

    private readonly List<Character> candidates = new();
    private readonly HashSet<Character> visibles  = new();
    public HashSet<Character> Visibles { get { return visibles; } }
    private readonly Dictionary<Character, CapsuleCollider> colCache = new();

    private static readonly Vector3[] pointBuffer = new Vector3[8]; // 재사용 버퍼 (GC 방지)

    public event Action<Character> onDetected;
    public event Action<Character> onLost;

    public IReadOnlyCollection<Character> VisibleCharacters => visibles;

    // ── 초기화 ────────────────────────────────────────────────────────────
    protected virtual void Awake()
    {
        owner = GetComponentInParent<Character>();
        eyePoint = owner.Anchor.head;

        var col = GetComponent<Collider>();
        col.isTrigger = true;
        gameObject.layer = LayerMask.NameToLayer("SightDetector");
    }

    // ── 후보 등록/해제 ────────────────────────────────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        // SightTarget 전용 콜라이더만 반응 — 히트박스·물리 콜라이더 중복 호출 방지
        var sightTarget = other.GetComponent<SightTarget>();
        if (sightTarget == null) return;

        var character = sightTarget.Owner;
        if (!IsValidEnemy(character)) return;

        if (!candidates.Contains(character))
        {
            candidates.Add(character);
            colCache[character] = sightTarget.Collider;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var sightTarget = other.GetComponent<SightTarget>();
        if (sightTarget == null) return;

        var character = sightTarget.Owner;
        if (character == null) return;

        candidates.Remove(character);
        colCache.Remove(character);

        if (visibles.Remove(character))
        {
            OnLost(character);
            onLost?.Invoke(character);
            TeamManager.Instance?.ReportVisibility(owner, character, false);
        }
    }

    // ── 매 프레임 가시성 갱신 ─────────────────────────────────────────────
    private void Update()
    {
        for (int i = candidates.Count - 1; i >= 0; i--)
        {
            var candidate = candidates[i];

            // 도중에 파괴된 오브젝트 정리
            if (candidate == null)
            {
                candidates.RemoveAt(i);
                continue;
            }

            // 풀링으로 비활성화된 후보: 보임 상태였다면 해제(렌더러 끔)해 stale을 막는다.
            // (비활성화 시 OnTriggerExit가 항상 발생하진 않음). candidates엔 남겨 재활성 시 다시 감지되게 한다.
            if (!candidate.gameObject.activeInHierarchy)
            {
                if (visibles.Remove(candidate))
                {
                    OnLost(candidate);
                    onLost?.Invoke(candidate);
                    TeamManager.Instance?.ReportVisibility(owner, candidate, false);
                }
                continue;
            }

            bool nowVisible = CheckVisible(candidate);
            bool wasVisible = visibles.Contains(candidate);

            if (nowVisible && !wasVisible)
            {
                visibles.Add(candidate);
                OnDetected(candidate);
                onDetected?.Invoke(candidate);
                TeamManager.Instance?.ReportVisibility(owner, candidate, true);
            }
            else if (!nowVisible && wasVisible)
            {
                visibles.Remove(candidate);
                OnLost(candidate);
                onLost?.Invoke(candidate);
                TeamManager.Instance?.ReportVisibility(owner, candidate, false);
            }
        }
    }

    // ── 레이캐스팅 ────────────────────────────────────────────────────────
    private bool CheckVisible(Character target)
    {
        Vector3 origin = eyePoint != null ? eyePoint.position : transform.position;

        int count = FillTargetPoints(target, pointBuffer);
        for (int i = 0; i < count; i++)
        {
            Vector3 dir = pointBuffer[i] - origin;
            // 레이가 벽에 맞지 않으면 시야 확보
            if (!Physics.Raycast(origin, dir.normalized, dir.magnitude, wallMask))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 캡슐 콜라이더 기준 샘플 지점을 buffer에 채우고 개수를 반환한다.
    /// 상단 중심(1) + 하단 중심(1) + 중단 6각형(6) = 8개
    /// </summary>
    private int FillTargetPoints(Character target, Vector3[] buffer)
    {
        if (!colCache.TryGetValue(target, out var col) || col == null)
        {
            buffer[0] = target.transform.position;
            return 1;
        }

        Transform t = col.transform;
        Vector3 worldCenter = t.TransformPoint(col.center);
        float worldRadius   = col.radius * Mathf.Max(t.lossyScale.x, t.lossyScale.z);
        float worldHalfH    = col.height * 0.5f * t.lossyScale.y;

        buffer[0] = worldCenter + Vector3.up   * worldHalfH;   // 1. 상단 중심
        buffer[1] = worldCenter + Vector3.down * worldHalfH;   // 2. 하단 중심

        for (int i = 0; i < 6; i++)                            // 3~8. 중단 6각형
        {
            float angle = i * 60f * Mathf.Deg2Rad;
            buffer[2 + i] = worldCenter + new Vector3(
                Mathf.Cos(angle) * worldRadius,
                0f,
                Mathf.Sin(angle) * worldRadius);
        }

        return 8;
    }

    // ── 이벤트 훅 (하위 클래스에서 오버라이드) ───────────────────────────
    protected virtual void OnDetected(Character character) { }
    protected virtual void OnLost(Character character) { }

    // ── 유틸 ─────────────────────────────────────────────────────────────
    public bool IsVisible(Character character) => visibles.Contains(character);

    private bool IsValidEnemy(Character character)
    {
        if (character == null) return false;
        if (owner == null) return true;
        return character.team != owner.team;
    }

    /// <summary>풀 재사용/비활성화 때 stale 데이터 초기화</summary>
    public void ResetState()
    {
        candidates.Clear();
        visibles.Clear();
        colCache.Clear();
    }

    // 시야 소유자가 비활성화/파괴되면, 보고 있던 대상들의 팀 가시 카운트를 정리한다.
    // (OnTriggerExit가 비활성화 시 항상 발생하진 않아 stale 렌더가 남는 것을 방지)
    protected virtual void OnDisable()
    {
        if (TeamManager.Instance != null)
            foreach (var c in visibles)
                TeamManager.Instance.ReportVisibility(owner, c, false);

        visibles.Clear();
    }
}
