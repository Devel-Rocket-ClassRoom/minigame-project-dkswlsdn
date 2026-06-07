using UnityEngine;
using UnityEngine.AI;

// 전투 씬 진입 시 파티원(현재 출전 캐릭터 제외)을 플레이어 주변에 1회 스폰한다. 리스폰 없음.
// 아군은 적 NPC와 동일하지만 team=플레이어팀 + AIBrain 추종 모드이며, characterId로 스탯/모델/무기를 주입받는다.
public class AllySpawner : MonoBehaviour
{
    [SerializeField] private Character allyPrefab;   // 적 NPC 기반 아군 프리팹(AllyStat + CharacterModelSwapper 포함)
    [SerializeField] private float spawnRadius = 3f; // 플레이어 주변 배치 반경
    [SerializeField] private int allyTeam = 1;       // 플레이어와 같은 팀

    // 모든 Awake 이후(Start)엔 플레이어가 씬에 배치되어 CurrentPlayer가 세팅돼 있다.
    private void Start()
    {
        var player = Character.CurrentPlayer;
        if (player == null) { Debug.LogWarning("[AllySpawner] 플레이어 없음 — 아군 스폰 취소"); return; }
        if (allyPrefab == null) { Debug.LogWarning("[AllySpawner] allyPrefab 미설정"); return; }

        var save = SaveManager.CurrentSave;
        var currentId = save.currentCharacterId;

        int index = 0;
        foreach (var id in save.currentParty)
        {
            if (id == currentId) continue;   // 현재 조작 캐릭터(=플레이어)는 제외
            SpawnAlly(id, player, index++);
        }
    }

    private void SpawnAlly(string characterId, Character player, int index)
    {
        // 플레이어 주변 원형 배치 → NavMesh 위로 보정
        float angle = index * 120f * Mathf.Deg2Rad;
        Vector3 pos = player.transform.position
                    + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * spawnRadius;
        if (NavMesh.SamplePosition(pos, out var hit, spawnRadius + 2f, NavMesh.AllAreas))
            pos = hit.position;

        // team을 먼저 확정한 뒤 OnEnable이 돌도록, 비활성 → team 세팅 → 활성 순서로 만든다.
        // (Character.OnEnable이 team!=1이면 렌더러를 끄기 때문)
        var ally = Instantiate(allyPrefab, pos, player.transform.rotation);
        ally.gameObject.SetActive(false);
        ally.team = allyTeam;
        ally.gameObject.SetActive(true);   // OnEnable: NavMesh 워프 + 기본값 리셋(team=아군이라 렌더 유지)

        // 캐릭터 데이터 주입(활성화 후 — OnEnable의 기본/디폴트 세팅을 덮어쓴다)
        ally.GetComponent<AllyStat>()?.Initialize(characterId);

        var weaponName = DataTableManager.StringTable.Get(characterId);
        var weapon = DatabaseManager.FindWeapon(weaponName);
        if (weapon != null) ally.Executer.CurrentWeapon = weapon;   // 스킬/콤보 세팅

        ally.GetComponent<CharacterModelSwapper>()?.SwapTo(characterId);  // 모델 + 부착 무기 비주얼

        ally.GetComponent<AllyBrain>()?.SetAlly(player);   // 추종 모드 ON
    }
}
