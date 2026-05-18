using UnityEngine;

public class Character : MonoBehaviour
{
    private static int id = 0;
    public int Id;
    public int team;

    public Character()
    {
        team = id;
        Id = id++;
    }

    public CharacterStat Stat { get; private set; }
    public SkillCaster Caster { get; private set; }
    public CharacterMovement Movement { get; private set; }
    public PlayerCamera Camera { get; private set; }
    public CharacterAim Aim { get; private set; }
    public StateManager State { get; private set; }


    private void Awake()
    {
        Stat = GetComponent<CharacterStat>();
        Caster = GetComponent<SkillCaster>();
        Movement = GetComponent<CharacterMovement>();
        Camera = GetComponent<PlayerCamera>();
        Aim = GetComponent<CharacterAim>();
        State = GetComponent<StateManager>();
    }
}
