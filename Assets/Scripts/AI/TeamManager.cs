using System.Collections.Generic;
using UnityEngine;

// 팀 단위 시야 공유 매니저(씬 싱글톤).
//
// 적(team != 플레이어팀)은 평소 렌더러가 꺼져 있고, "플레이어 팀(플레이어 + 아군)" 중
// 누군가의 시야에 들어올 때만 보여야 한다. 각 CharacterSight가 감지/놓침을 여기로 보고하면
// 대상별 가시 카운트(refcount)를 누적해, 0→1일 때 렌더러를 켜고 1→0일 때 끈다.
// 즉 아군이 본 적도 플레이어 화면에 함께 보인다(시야 공유).
//
// 전투가 일어나는 씬마다 하나 배치해야 한다(없으면 적이 계속 안 보임).
public class TeamManager : MonoBehaviour
{
    public static TeamManager Instance { get; private set; }

    // 적 캐릭터 -> 현재 그를 보고 있는 팀원 수
    private readonly Dictionary<Character, int> seenCount = new();

    private int PlayerTeam => Character.CurrentPlayer != null ? Character.CurrentPlayer.team : 1;

    private void Awake() { Instance = this; }
    private void OnDestroy() { if (Instance == this) Instance = null; }

    // viewer(팀원)가 target을 보기 시작(seen=true)/놓침(seen=false).
    public void ReportVisibility(Character viewer, Character target, bool seen)
    {
        if (viewer == null || target == null) return;
        if (viewer.team != PlayerTeam) return;   // 적의 시야는 렌더링에 영향 없음
        if (target.team == PlayerTeam) return;    // 아군/플레이어는 항상 보이므로 토글 대상 아님

        seenCount.TryGetValue(target, out int count);

        if (seen)
        {
            count++;
            seenCount[target] = count;
            if (count == 1) SetRenderers(target, true);   // 처음 보임 → 렌더 on
        }
        else
        {
            count = Mathf.Max(0, count - 1);
            if (count == 0)
            {
                seenCount.Remove(target);
                SetRenderers(target, false);              // 아무도 안 봄 → 렌더 off
            }
            else
            {
                seenCount[target] = count;
            }
        }
    }

    private static void SetRenderers(Character c, bool enabled)
    {
        foreach (var r in c.GetComponentsInChildren<Renderer>())
            r.enabled = enabled;
    }
}
