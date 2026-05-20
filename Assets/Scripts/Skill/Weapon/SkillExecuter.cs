using System;
using UnityEngine;

public class SkillExecuter : MonoBehaviour
{
    private Weapon currentWeapon;
    public Weapon CurrentWeapon
    {
        get => currentWeapon;
        set
        {
            if (value == null)
            {
                if (defaultWeapon == null) throw new Exception("기본 무기 설정되지 않음");
                currentWeapon = defaultWeapon;
            }
            else
            {
                currentWeapon = value;
            }
            Init(currentWeapon);
        }
    }

    [SerializeField] private Weapon defaultWeapon;

    private SkillCaster caster;
    private CharacterCommander commander;

    private readonly Skill[] skills = new Skill[8];
    private readonly float[] cooldowns = new float[8];
    private int currentIndex = -1;

    private static readonly ConditionInput[] inputs =
    {
        ConditionInput.SkillR,
        ConditionInput.SkillSL,
        ConditionInput.SkillLR,
        ConditionInput.Q,
        ConditionInput.E,
        ConditionInput.F,
        ConditionInput.Space,
    };

    private void Awake()
    {
        caster = GetComponent<SkillCaster>();
        commander = GetComponent<CharacterCommander>();
        caster.onCooldownReset += CooldownReset;
    }

    private void OnEnable()
    {
        CurrentWeapon = defaultWeapon;
    }

    private void Init(Weapon weapon)
    {
        skills[0] = weapon.LSkill;
        skills[1] = weapon.RSkill;
        skills[2] = weapon.SLSkill;
        skills[3] = weapon.LRSkill;
        skills[4] = weapon.QSkill;
        skills[5] = weapon.ESkill;
        skills[6] = weapon.FSkill;
        skills[7] = weapon.SpaceSkill;
    }

    private void Update()
    {
        for (int i = 0; i < cooldowns.Length; i++)
        {
            if (i == currentIndex) continue;
            cooldowns[i] -= Time.deltaTime;
        }

        for (int i = 1; i < inputs.Length + 1; i++)
        {
            if (skills[i] == null) continue;
            if (cooldowns[i] > 0) continue;
            if (!commander.GetInput(inputs[i - 1], false)) continue;

            if (caster.Cast(skills[i], i))
            {
                cooldowns[i] = skills[i].cooldown;
                currentIndex = i;
                break;
            }
        }

        if (skills[0] != null && cooldowns[0] <= 0 && commander.GetInput(ConditionInput.SkillL, false))
        {
            if (caster.Cast(skills[0], 0))
            {
                cooldowns[0] = skills[0].cooldown;
                currentIndex = 0;
            }
        }
    }

    public Skill GetSkill(int index) => skills[index];

    // 0 = 준비됨, 1 = 쿨타임 최대
    public float GetCooldownRatio(int index)
    {
        if (skills[index] == null || skills[index].cooldown <= 0f) return 0f;
        return Mathf.Clamp01(cooldowns[index] / skills[index].cooldown);
    }

    public void CooldownReset(int index)
    {
        cooldowns[index] = skills[index].cooldown;
        currentIndex = -1;
    }
}
