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
        // 프리팹 기본 위치(원점 등)에서 NavMeshAgent가 OnEnable되면 "not close enough to NavMesh" 경고가 난다.
        // 생성 시점부터 네브메시 위 유효 위치(스폰 지점)에 두어 경고를 막는다. 실제 배치는 SpawnOne에서.
        Vector3 pos = (spawnPoint != null && spawnPoint.Length > 0 && spawnPoint[0] != null)
            ? spawnPoint[0].transform.position
            : transform.position;

        var c = Instantiate(spawnCharacter, pos, Quaternion.identity);
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

        // 죽음 연출은 비풀 적과 동일하게 진행하고, 연출이 끝나는 시점(비풀이 Destroy하는 시점)에 풀 반환.
        var death = c.GetComponent<CharacterDeath>();
        death.SetOwnedByPool(true);

        Action handler = () => Release(c);
        deathHandlers[c] = handler;
        death.onDeathComplete += handler;
    }

    private void Release(Character c)
    {
        if (!alive.Remove(c)) return;   // 중복 반납 방지

        if (deathHandlers.TryGetValue(c, out var h))
        {
            var death = c.GetComponent<CharacterDeath>();
            if (death != null) death.onDeathComplete -= h;
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