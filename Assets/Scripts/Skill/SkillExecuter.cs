using System;
using UnityEngine;

public class SkillExecuter : MonoBehaviour
{
    public bool IsDisableCast { get; set; }
    private Weapon currentWeapon;
    public Skill instantQSkill;
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

        for (int i = 0; i < cooldowns.Length; i++)
        {
            cooldowns[i] = 0f;
        }
    }

    private void Update()
    {
        if (IsDisableCast) return;

        for (int i = 0; i < cooldowns.Length; i++)
        {
            if (i == caster.Context.currentIndex) continue;
            cooldowns[i] -= Time.deltaTime;
        }

        for (int i = inputs.Length; i >= 1; i--)
        {
            if (skills[i] == null) continue;
            if (cooldowns[i] > 0) continue;
            if (!commander.GetInput(inputs[i - 1], false)) continue;

            // L을 누른 채 R을 누르면 LR 콤보 의도이므로 단독 R을 보류한다.
            // (LR 컴포지트 입력은 R 단독 입력보다 한 프레임 늦게 판정될 수 있어,
            //  보류하지 않으면 R이 LR보다 먼저 발동되어 버린다.)
            if (inputs[i - 1] == ConditionInput.SkillR
                && skills[3] != null && cooldowns[3] <= 0f
                && commander.GetInput(ConditionInput.SkillL, true))
                continue;

            if (caster.Cast(skills[i], i))
            {
                cooldowns[i] = skills[i].cooldown;
                break;
            }
        }

        if (skills[0] != null && cooldowns[0] <= 0 && commander.GetInput(ConditionInput.SkillL, false))
        {
            if (caster.Cast(skills[0], 0))
            {
                cooldowns[0] = skills[0].cooldown;
            }
        }
    }

    public Skill GetSkill(int index) => skills[index];

    public float GetCooldownRatio(int index)
    {
        if (skills[index] == null || skills[index].cooldown <= 0f) return 0f;
        return Mathf.Clamp01(cooldowns[index] / skills[index].cooldown);
    }

    public void CooldownReset(int index)
    {
        if (index == -1) return;

        cooldowns[index] = skills[index].cooldown;
    }

    public bool IsSkillReady(ConditionInput input)
    {
        int idx = GetSkillIndex(input);
        if (idx < 0) return false;
        return skills[idx] != null && cooldowns[idx] <= 0f;
    }

    private int GetSkillIndex(ConditionInput input)
    {
        if (input == ConditionInput.SkillL) return 0;
        int i = System.Array.IndexOf(inputs, input);
        return i >= 0 ? i + 1 : -1;
    }
}
