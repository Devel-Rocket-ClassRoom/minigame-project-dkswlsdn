using System;
using UnityEngine;

public class SkillExecuter : MonoBehaviour
{
    public bool IsDisableCast { get; set; }
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

    // 인덱스 = (int)SkillKey (Passive 제외). L=0, R=1, SL=2, LR=3, E=4, F=5, Space=6
    // → 세이브의 magicOpenedSkill[i] 와 skills[i] 가 1:1 로 매칭된다.
    protected readonly Skill[] skills = new Skill[7];
    private readonly float[] cooldowns = new float[7];

    // skills[i] 를 발동시키는 입력 (인덱스 1:1)
    private static readonly ConditionInput[] inputs =
    {
        ConditionInput.SkillL,   // 0 L
        ConditionInput.SkillR,   // 1 R
        ConditionInput.SkillSL,  // 2 SL
        ConditionInput.SkillLR,  // 3 LR
        ConditionInput.E,        // 4 E
        ConditionInput.F,        // 5 F
        ConditionInput.Space,    // 6 Space
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

    protected virtual void Init(Weapon weapon)
    {
        skills[(int)SkillKey.L]     = weapon.LSkill;
        skills[(int)SkillKey.R]     = weapon.RSkill;
        skills[(int)SkillKey.SL]    = weapon.SLSkill;
        skills[(int)SkillKey.LR]    = weapon.LRSkill;
        skills[(int)SkillKey.E]     = weapon.ESkill;
        skills[(int)SkillKey.F]     = weapon.FSkill;
        skills[(int)SkillKey.Space] = weapon.SpaceSkill;

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

        // L(기본 공격, index 0)을 제외한 스킬을 높은 인덱스부터 검사한다.
        for (int i = skills.Length - 1; i >= 1; i--)
        {
            if (skills[i] == null) continue;
            if (cooldowns[i] > 0) continue;
            if (!commander.GetInput(inputs[i], false)) continue;

            // L을 누른 채 R을 누르면 LR 콤보 의도이므로 단독 R을 보류한다.
            // (LR 컴포지트 입력은 R 단독 입력보다 한 프레임 늦게 판정될 수 있어,
            //  보류하지 않으면 R이 LR보다 먼저 발동되어 버린다.)
            if (inputs[i] == ConditionInput.SkillR
                && skills[(int)SkillKey.LR] != null && cooldowns[(int)SkillKey.LR] <= 0f
                && commander.GetInput(ConditionInput.SkillL, true))
                continue;

            if (caster.Cast(skills[i], i))
            {
                cooldowns[i] = skills[i].cooldown;
                break;
            }
        }

        // L (기본 공격) 별도 처리
        const int lIndex = (int)SkillKey.L; // 0
        if (skills[lIndex] != null && cooldowns[lIndex] <= 0 && commander.GetInput(ConditionInput.SkillL, false))
        {
            if (caster.Cast(skills[lIndex], lIndex))
            {
                cooldowns[lIndex] = skills[lIndex].cooldown;
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
        return Array.IndexOf(inputs, input);
    }
}
