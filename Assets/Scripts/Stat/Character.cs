using System;
using UnityEngine;

public class Character : MonoBehaviour
{
    private static int id = 0;
    public int Id;
    public int team;

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

        team = id;
        Id = id++;
    }
}
