using System;
using System.Collections;
using UnityEngine;

// 캐릭터 사망 연출 + 소멸 처리.
//
// 진입 분기(StateManager.onDead 시):
//  - 파괴판정으로 죽음(DeathByBreak)      → 즉시 사망 모션
//  - 죽기 직전 Airborne                   → 착지까지 대기 → 정지 → 사망 모션
//  - 그 외                                 → 그로기 모션 재생 후 → 사망 모션
//
// "사망 모션" = 현재(마지막) 포즈를 그대로 정지(holdBeforeEffect초) → 죽음 이펙트만 재생.
//
// 소멸:
//  - 플레이어         : 이 컴포넌트는 관여하지 않음(게임오버 등 별도 흐름).
//  - 스폰매니저 소속  : 소멸은 SpawnManager가 corpseDelay로 처리(여기선 연출만).
//  - 그 외            : 사망 모션이 끝나면 스스로 Destroy.
[RequireComponent(typeof(StateManager))]
public class CharacterDeath : MonoBehaviour
{
    [Header("연출 타이밍")]
    [Tooltip("그로기 모션 길이(그 외 상태에서 죽었을 때)")]
    [SerializeField] private float groggyDuration = 1.0f;
    [Tooltip("마지막 포즈를 정지한 채 유지하는 시간")]
    [SerializeField] private float holdBeforeEffect = 0.3f;
    [Tooltip("Airborne에서 착지를 기다리는 최대 시간(안전장치)")]
    [SerializeField] private float maxAirborneWait = 10f;

    [Header("이펙트")]
    [SerializeField] private EffectData deathEffect;

    private StateManager state;
    private CharacterMovement movement;
    private CharacterAnchor anchor;
    private Character character;

    private Coroutine routine;
    private bool ownedByPool;   // SpawnManager가 관리하는 인스턴스인지

    // 풀 소속 인스턴스에서 사망 연출이 끝난 시점(비풀이면 Destroy하는 그 시점)에 발생.
    // 구독자(SpawnManager)가 이때 풀로 반환한다.
    public event Action onDeathComplete;

    private void Awake()
    {
        state = GetComponent<StateManager>();
        movement = GetComponent<CharacterMovement>();
        anchor = GetComponent<CharacterAnchor>();
        character = GetComponent<Character>();
    }

    private void OnEnable()
    {
        // 풀 재사용 대비 초기화
        ownedByPool = false;
        if (routine != null) { StopCoroutine(routine); routine = null; }

        state.onDead += OnDead;
    }

    private void OnDisable()
    {
        state.onDead -= OnDead;
    }

    // SpawnManager가 풀에서 꺼낸 직후 호출 → 자가 Destroy를 막는다.
    public void SetOwnedByPool(bool owned) => ownedByPool = owned;

    private void OnDead()
    {
        if (character == null) return;   // Character 컴포넌트가 없으면 무시

        if (routine != null) StopCoroutine(routine);
        bool breakable = state.DeathByBreak;
        routine = StartCoroutine(CoDeath(breakable));
    }

    private IEnumerator CoDeath(bool breakable)
    {
        if (breakable)
        {
            // 즉시 사망 모션 (공중이어도 바로 정지)
        }
        else if (movement != null && !movement.GetOnGrounded() && state.State == CharacterState.Airborne)
        {
            float timeout = Time.time + maxAirborneWait;
            while (movement != null && !movement.GetOnGrounded() && Time.time < timeout)
                yield return null;
        }
        else
        {
            // 지상 사망 → 그로기 모션
            state.DeathGroggy();
            yield return new WaitForSeconds(groggyDuration);
        }

        // --- 사망 모션: 마지막 포즈 정지 ---
        if (movement != null) movement.StopImmediately();
        state.DeathFreezePose();
        yield return new WaitForSeconds(holdBeforeEffect);

        // --- 이펙트만 재생 ---
        PlayDeathEffect();

        // --- 소멸 ---
        if (character.isPlayer)
        {
            // 플레이어는 파괴하지 않고 게임오버 흐름으로 넘긴다(카메라/입력/CurrentPlayer 참조 보호)
            MenuManager.instance.GameOver();
        }
        else if (ownedByPool)
        {
            // 풀 소속: Destroy 대신 이 시점(=비풀이 Destroy하는 시점)에 풀 반환을 요청한다.
            routine = null;
            onDeathComplete?.Invoke();
            yield break;
        }
        else
        {
            Destroy(gameObject);
        }

        routine = null;
    }

    private void PlayDeathEffect()
    {
        if (deathEffect == null || deathEffect.effect == null) return;
        if (EffectManager.instance == null) return;

        Vector3 pos = anchor != null && anchor.head != null ? anchor.head.position : transform.position;
        EffectManager.instance.Play(deathEffect, pos);   // 부모 없이 월드 좌표 → Destroy해도 살아남음
    }
}
