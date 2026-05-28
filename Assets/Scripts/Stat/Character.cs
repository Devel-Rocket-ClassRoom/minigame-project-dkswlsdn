using System;
using UnityEngine;

public class Character : MonoBehaviour
{
    private static int id = 0;
    public int Id;
    public int team;
    public bool isPlayer;

    [HideInInspector, NonSerialized]
    public int[] opennedToken = new int[9];

    public CharacterStat Stat { get; private set; }
    public SkillCaster Caster { get; private set; }
    public CharacterMovement Movement { get; private set; }
    public PlayerCamera Camera { get; private set; }
    public CharacterAim Aim { get; private set; }
    public StateManager State { get; private set; }
    public CharacterCommander Commander { get; private set; }
    public CharacterAnchor Anchor { get; private set; }
    public SpecialStackHandler Stack { get; private set; }
    public SightManager Sight { get; private set; }
    public ItemQuickSlot QuickSlot { get; private set; }

    public Node CurrentNode { get; private set; }


    private void Awake()
    {
        Stat = GetComponent<CharacterStat>();
        Caster = GetComponent<SkillCaster>();
        Movement = GetComponent<CharacterMovement>();
        Camera = GetComponent<PlayerCamera>();
        Aim = GetComponent<CharacterAim>();
        State = GetComponent<StateManager>();
        Commander = GetComponent<CharacterCommander>();
        Anchor = GetComponent<CharacterAnchor>();
        Stack = GetComponent<SpecialStackHandler>();
        Sight = GetComponentInChildren<SightManager>();
        QuickSlot = GetComponent<ItemQuickSlot>();

        team = id;
        Id = id++;

        //if (isPlayer) VisibilityManager.instance.Register(this);
    }
}
