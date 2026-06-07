using UnityEngine;

// 적 AIBrain을 상속한 아군 전용 두뇌.
// 적과의 차이:
//   1) 어그로가 없을 때 순찰(Patrol) 대신 플레이어를 추종(Follow)한다.
//   2) 교전 중 적이 죽거나(State==Dead)/사라지면 즉시 추종으로 복귀하고,
//      살아있지만 시야에서 사라지면 Chase(기존 타임아웃 → 복귀)로 넘긴다.
// 그 외 전투/추격/로프파괴 로직은 전부 기반 AIBrain 그대로 사용한다.
public class AllyBrain : AIBrain
{
    private Character followTarget;
    [SerializeField] private float followStopDistance = 2.5f; // 플레이어와 이 거리만큼 떨어져 정지

    // AllySpawner가 호출: 추종 대상(플레이어) 지정 + 추종 시작.
    public void SetAlly(Character player)
    {
        followTarget = player;
        agent.stoppingDistance = followStopDistance;
        ChangeState(AIState.Follow);
    }

    // 어그로 없을 때의 기본 상태를 '추종'으로.
    protected override AIState IdleState() => AIState.Follow;

    protected override void CheckTransition()
    {
        switch (currentState)
        {
            case AIState.Follow:
                if (aggro == null) TryAcquireFromSight();
                if (aggro != null) ChangeState(AIState.Combat);
                return;

            case AIState.Combat:
                // 죽었거나 사라진 적 → 즉시 어그로 해제 후 추종 복귀
                if (aggro == null
                    || aggro.State.State == CharacterState.Dead
                    || !aggro.gameObject.activeInHierarchy)
                {
                    aggro = null;
                    ChangeState(IdleState());
                    return;
                }
                // 살아있지만 안 보임 → Chase(기존 3초 타임아웃 → 복귀)
                if (!sight.IsVisible(aggro))
                {
                    ChangeState(AIState.Chase);
                    return;
                }
                if (FindVisitedAnchorNearby() is Anchor anchor)
                    ChangeState(AIState.DestroyRope, anchor);
                return;

            default:
                base.CheckTransition(); // Patrol/Chase/DestroyRope는 기반 로직 그대로
                return;
        }
    }

    protected override void ExecuteState()
    {
        if (currentState == AIState.Follow) { ExecuteFollow(); return; }
        base.ExecuteState();
    }

    protected override void ChangeState(AIState next, Anchor anchor = null)
    {
        if (next == AIState.Follow)
        {
            currentState = next;
            aggro = null;
            agent.isStopped = false;
            command.SetMoveInput(Vector2.zero);
            StopChaseCoroutine();
            return;
        }
        base.ChangeState(next, anchor);
    }

    // 적 NPC(Patrol/Chase)와 동일하게 NavMesh 목적지를 플레이어로 잡고 desiredVelocity를 입력으로 변환한다.
    // stoppingDistance 이내로 가까워지면 desiredVelocity가 0 → 입력 0 → 자연히 멈춘다.
    private void ExecuteFollow()
    {
        if (followTarget == null) { command.SetMoveInput(Vector2.zero); return; }

        agent.SetDestination(followTarget.transform.position);
        ApplyNavMeshInput();
    }
}
