using System;
using Unity.VisualScripting;
using UnityEngine;

public class Character : MonoBehaviour
{
    private static int id = 0;
    [HideInInspector] public int Id;
    public int team;
    public bool isPlayer;
    public static Character CurrentPlayer { get; private set; }
    public static event Action<Character> OnPlayerAppeared;

    

    public static void SubscribeToPlayer(Action<Character> callback)
    {
        OnPlayerAppeared += callback;
        if (CurrentPlayer != null) callback(CurrentPlayer);
    }

    public static void UnsubscribeFromPlayer(Action<Character> callback)
    {
        OnPlayerAppeared -= callback;
    }

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
    public CharacterSight Sight { get; private set; }
    public SightTarget SightTarget { get; private set; }
    public ItemQuickSlot QuickSlot { get; private set; }
    public InteractionManager Interaction { get; private set; }



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
        Sight = GetComponentInChildren<CharacterSight>();
        SightTarget = GetComponentInChildren<SightTarget>();
        QuickSlot = GetComponent<ItemQuickSlot>();
        Interaction = GetComponent<InteractionManager>();

        Id = id++;

        if (isPlayer)
        {
            CurrentPlayer = this;
            OnPlayerAppeared?.Invoke(this);
        }
    }

    private void OnEnable()
    {
        // 아군 팀(1)은 항상 보이고, 적(그 외)은 평소 숨김 → TeamManager가 시야에 들어올 때 켠다.
        // team이 바뀌어 OnEnable이 다시 도는 경우(아군 스폰)에도 확실히 반영되도록 명시적으로 on/off.
        bool visible = team == 1;
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = visible;

        Stat?.ResetState();       // 체력 최대로
        State?.ResetState();      // Dead 해제 → Idle (콜라이더/애니 복구)
        Movement?.ResetState();   // 속도/중력 초기화
        Caster?.ResetState();     // 진행중 스킬 정리 + 캐스팅 재활성
        Executer?.ResetState();   // 쿨다운/캐스트락 초기화
        Stack?.ResetState();      // 스택 효과 되돌리고 비우기
        Sight?.ResetState();      // 감지 목록 비우기
    }
}