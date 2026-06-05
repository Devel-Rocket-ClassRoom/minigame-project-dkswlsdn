using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SubWeaponDatabase", menuName = "Scriptable Objects/SubWeaponDatabase")]
public class SubWeaponDatabase : ScriptableObject
{
    public List<Skill> subWeaponSkills;

    // skillId로 서브웨폰 스킬을 찾는다. 없으면 null.
    public Skill Find(string skillId)
    {
        if (string.IsNullOrEmpty(skillId) || subWeaponSkills == null) return null;
        return subWeaponSkills.Find(s => s != null && s.skillId == skillId);
    }
}
