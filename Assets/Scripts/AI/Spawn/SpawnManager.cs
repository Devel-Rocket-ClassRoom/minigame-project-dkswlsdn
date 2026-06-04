using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

public class SpawnManager : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private Node[] spawnPoint;          // 스폰 위치 & 순찰 경로
    [SerializeField] private Character spawnCharacter;   // 소환 프리팹
    [SerializeField] private int spawnTeam = 0;          // 내보낼 팀
    [SerializeField] private float spawnInterval = 1f;   // 소환 간격
    [SerializeField] private int spawnCap = 5;           // 동시 최대치
    [SerializeField] private bool useRespawn = true;     // 죽으면 다시 채울지

    [Header("Detection")]
    [SerializeField] private float checkRadius = 15f;
    [SerializeField] private LayerMask characterMask;    // Character 레이어
    [SerializeField] private int detectTeam = 1;         // 감지할 팀(플레이어 파티)
    [SerializeField] private float checkInterval = 1f;

    [Header("Death")]
    [SerializeField] private float corpseDelay = 2f;

    [Header("Death")]
    [SerializeField] private List<ItemDropRate> dropRateList;

    private ObjectPool<Character> pool;
    private readonly List<Character> alive = new();
    private readonly Dictionary<Character, Action> deathHandlers = new();
    private readonly Collider[] buffer = new Collider[32];
    private List<Node> patrolList;
    private int spawnedEver;
    private Coroutine loop;

    private void Awake()
    {
        pool = new ObjectPool<Character>(
            createFunc: CreateCharacter,
            actionOnGet: null,                                    // C안: 꺼낸 직후엔 비활성 유지
            actionOnRelease: c => c.gameObject.SetActive(false),
            actionOnDestroy: c => Destroy(c.gameObject),
            collectionCheck: false,
            defaultCapacity: Mathf.Max(spawnCap, 1),
            maxSize: Mathf.Max(spawnCap, 1));

        patrolList = new List<Node>(spawnPoint);
    }

    private void OnEnable() => loop = StartCoroutine(SpawnLoop());

    private void OnDisable()
    {
        if (loop != null) StopCoroutine(loop);
        loop = null;
    }

    private Character CreateCharacter()
    {
        var c = Instantiate(spawnCharacter);
        c.gameObject.SetActive(false);
        return c;
    }

    private IEnumerator SpawnLoop()
    {
        var spawnWait = new WaitForSeconds(spawnInterval);
        var checkWait = new WaitForSeconds(checkInterval);

        while (true)
        {
            bool canSpawn = alive.Count < spawnCap
                            && (useRespawn || spawnedEver < spawnCap)
                            && PlayerInRadius();

            if (canSpawn) { SpawnOne(); yield return spawnWait; }
            else          { yield return checkWait; }
        }
    }

    private bool PlayerInRadius()
    {
        int n = Physics.OverlapSphereNonAlloc(transform.position, checkRadius, buffer, characterMask);
        for (int i = 0; i < n; i++)
        {
            var c = buffer[i].GetComponentInParent<Character>();
            if (c != null && c.team == detectTeam) return true;
        }
        return false;
    }

    private void SpawnOne()
    {
        if (spawnPoint == null || spawnPoint.Length == 0) return;

        var node = spawnPoint[UnityEngine.Random.Range(0, spawnPoint.Length)];
        var c = pool.Get();

        c.transform.SetPositionAndRotation(node.transform.position, node.transform.rotation);
        c.team = spawnTeam;
        c.GetComponent<AIBrain>()?.SetPatrol(patrolList);
        alive.Add(c);
        spawnedEver++;

        var drop = c.GetComponent<NPCItemQuickSlot>();

        foreach (var item in dropRateList)
        {
            if (UnityEngine.Random.Range(0f, 100f) <= item.rate)
            {
                drop.SetItem(item.item);
            }
        }

        c.gameObject.SetActive(true);
        c.GetComponent<StateManager>()?.ResetState();
        c.GetComponent<CharacterMovement>()?.ResetState();
        c.GetComponent<CharacterStat>()?.ResetState();

        Action handler = () => OnDead(c);
        deathHandlers[c] = handler;
        c.State.onDead += handler;
    }

    private void OnDead(Character c) => StartCoroutine(ReleaseAfter(c, corpseDelay));

    private IEnumerator ReleaseAfter(Character c, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        Release(c);
    }

    private void Release(Character c)
    {
        if (!alive.Remove(c)) return;   // 중복 반납 방지

        if (deathHandlers.TryGetValue(c, out var h))
        {
            c.State.onDead -= h;
            deathHandlers.Remove(c);
        }
        pool.Release(c);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}

[Serializable]
public class ItemDropRate
{
    public Item item;
    public float rate;
}