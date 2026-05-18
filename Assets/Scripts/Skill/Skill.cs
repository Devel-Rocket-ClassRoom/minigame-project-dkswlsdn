using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skill")]
public class Skill : ScriptableObject
{
    public string skillName;
    public List<SkillAction> actions;
    public List<SkillTransition> transitions;
}
