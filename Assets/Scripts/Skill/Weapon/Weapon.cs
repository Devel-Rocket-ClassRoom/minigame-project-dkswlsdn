using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapon")]
public class Weapon : ScriptableObject
{
    public int id;
    public string weaponName;
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
}
