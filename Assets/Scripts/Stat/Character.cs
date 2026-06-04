using System;
using UnityEngine;

public class Character : MonoBehaviour
{
    private static int id = 0;
    [HideInInspector] public int Id;
    public int team;
    public bool isPlayer;

    [HideInInspector, NonSerialized]
    public int[] opennedToken = new int[9];

    public CharacterStat Stat { get; private set; }
    public SkillCaster Caster { get; private set; }
    public SkillExecuter Executer { get; private set; }
    public CharacterMovement Movement { get; private set; }
    public CharacterCamera Camera { get; private set; }
    public CharacterAim Aim { get; private set; }
    public StateManager State { get; private set; }
    public CharacterCommander Commander { get; private set; }
    public CharacterAnchor Anchor { get; private set; }
    public SpecialStackHandler Stack { get; private set; }
    public SightManager Sight { get; private set; }
    public ItemQuickSlot QuickSlot { get; private set; }



    private void Awake()
    {
        Stat = GetComponent<CharacterStat>();
        Caster = GetComponent<SkillCaster>();
        Executer = GetComponent<SkillExecuter>();
        Movement = GetComponent<CharacterMovement>();
        Camera = GetComponent<CharacterCamera>();
        Aim = GetComponent<CharacterAim>();
        State = GetComponent<StateManager>();
        Commander = GetComponent<CharacterCommander>();
        Anchor = GetComponent<CharacterAnchor>();
        Stack = GetComponent<SpecialStackHandler>();
        Sight = GetComponentInChildren<SightManager>();
        QuickSlot = GetComponent<ItemQuickSlot>();

        Id = id++;
    }

    private void OnEnable()
    {
        // 풀 재사용/활성화 시 깨끗한 상태로 복귀. 없는 컴포넌트는 ?. 로 건너뜀("null이면 동작 안 함").
        // 순서 주의: Stat(체력) → State(Dead 해제) → 나머지.
        Stat?.ResetState();       // 체력 최대로
        State?.ResetState();      // Dead 해제 → Idle (콜라이더/애니 복구)
        Movement?.ResetState();   // 속도/중력 초기화
        Caster?.ResetState();     // 진행중 스킬 정리 + 캐스팅 재활성
        Executer?.ResetState();   // 쿨다운/캐스트락 초기화
        Stack?.ResetState();      // 스택 효과 되돌리고 비우기
        Sight?.ResetState();      // 감지 목록 비우기
    }
}