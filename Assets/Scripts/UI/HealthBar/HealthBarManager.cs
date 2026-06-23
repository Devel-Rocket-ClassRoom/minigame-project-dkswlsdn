using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// 적 체력바를 플레이어 시야(PlayerSight)와 연동해 풀로 관리한다.
//  - 적이 플레이어 시야에 보이면(onDetected) 풀에서 바를 꺼내 부여
//  - 반환 트리거:
//      · 바인드된 적의 State.onDead   → 사망 즉시 풀 반환
//      · 플레이어 시야 onLost          → 살아서 시야 밖/벽 뒤로 사라질 때 반환
public class HealthBarManager : MonoBehaviour
{
    [SerializeField] private HealthBarFollow barPrefab;
    [SerializeField] private Transform barParent;   // Screen Space - Overlay 캔버스(또는 그 하위)
    [SerializeField] private int defaultCapacity = 8;
    [SerializeField] private int maxSize = 32;

    private ObjectPool<HealthBarFollow> pool;
    private readonly Dictionary<Character, HealthBarFollow> bars = new();
    private readonly Dictionary<Character, Action> deathHandlers = new();
    private CharacterSight sight;

    private void Awake()
    {
        pool = new ObjectPool<HealthBarFollow>(
            createFunc:      () => Instantiate(barPrefab, barParent),
            actionOnGet:     b => b.gameObject.SetActive(true),
            actionOnRelease: b => b.gameObject.SetActive(false),
            actionOnDestroy: b => Destroy(b.gameObject),
            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize);
    }

    private void Start() => Character.SubscribeToPlayer(OnPlayerAppeared);

    private void OnPlayerAppeared(Character player)
    {
        if (sight != null)   // 플레이어 교체 대비
        {
            sight.onDetected -= OnDetected;
            sight.onLost     -= OnLost;
        }

        sight = player.Sight;
        if (sight == null) return;

        sight.onDetected += OnDetected;
        sight.onLost     += OnLost;
    }

    private void OnDetected(Character enemy)
    {
        if (barPrefab == null || barParent == null) return;
        if (bars.ContainsKey(enemy)) return;

        var bar = pool.Get();
        bar.Bind(enemy);          // 재사용이므로 새 대상으로 초기화
        bars[enemy] = bar;

        // 바인드된 적이 죽으면 즉시 풀 반환
        Action onDead = () => RemoveBar(enemy);
        deathHandlers[enemy] = onDead;
        if (enemy.State != null) enemy.State.onDead += onDead;
    }

    private void OnLost(Character enemy) => RemoveBar(enemy);

    private void RemoveBar(Character enemy)
    {
        // onDead 구독 해제 (사망/시야이탈 어느 쪽으로 제거되든 안전하게)
        if (deathHandlers.TryGetValue(enemy, out var onDead))
        {
            if (enemy != null && enemy.State != null) enemy.State.onDead -= onDead;
            deathHandlers.Remove(enemy);
        }

        if (bars.TryGetValue(enemy, out var bar))
        {
            if (bar != null) pool.Release(bar);
            bars.Remove(enemy);
        }
    }

    private void OnDestroy()
    {
        Character.UnsubscribeFromPlayer(OnPlayerAppeared);
        if (sight != null)
        {
            sight.onDetected -= OnDetected;
            sight.onLost     -= OnLost;
        }
    }
}
