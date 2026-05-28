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

    public Skill LSkill;
    public Skill RSkill;
    public Skill LRSkill;
    public Skill SLSkill;
    public Skill FSkill;
    public Skill QSkill;
    public Skill ESkill;
    public Skill SpaceSkill;

    [Header("콤보")]
    public List<Combo> combo;
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
