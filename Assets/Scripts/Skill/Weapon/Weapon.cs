using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapon")]
public class Weapon : ScriptableObject
{
    public int id;
    public string weaponName;
    public bool cannotUse;
    public Sprite icon;
    public GameObject model;

    [Header("기본 스킬")]
    public Skill PassiveSkill;
    public Skill ESkill;
    public Skill FSkill;
    public Skill SpaceSkill;
    public Skill LSkill;
    public Skill RSkill;
    public Skill SLSkill;
    public Skill LRSkill;

    [Header("마법 전환 스킬")]
    public Skill MagicPassiveSkill;
    public Skill MagicESkill;
    public Skill MagicFSkill;
    public Skill MagicSpaceSkill;
    public Skill MagicLSkill;
    public Skill MagicRSkill;
    public Skill MagicSLSkill;
    public Skill MagicLRSkill;

    [Header("콤보")]
    public List<Combo> combo;

    // SkillKey와 마법 전환 여부로 해당 스킬을 반환
    public Skill GetSkill(SkillKey key, bool isMagic)
    {
        switch (key)
        {
            case SkillKey.Passive: return isMagic ? MagicPassiveSkill : PassiveSkill;
            case SkillKey.E:       return isMagic ? MagicESkill : ESkill;
            case SkillKey.F:       return isMagic ? MagicFSkill : FSkill;
            case SkillKey.Space:   return isMagic ? MagicSpaceSkill : SpaceSkill;
            case SkillKey.L:       return isMagic ? MagicLSkill : LSkill;
            case SkillKey.R:       return isMagic ? MagicRSkill : RSkill;
            case SkillKey.SL:      return isMagic ? MagicSLSkill : SLSkill;
            case SkillKey.LR:      return isMagic ? MagicLRSkill : LRSkill;
            default: return null;
        }
    }
}

[Serializable]
public class Combo
{
    public List<AICondition> conditions;

    public List<InputElement> comboInput;
}

[Serializable]
public class InputElement
{
    public ConditionInput input;
    public float preDelay;
    public bool isPress;
    public AICondition condtion;
}
