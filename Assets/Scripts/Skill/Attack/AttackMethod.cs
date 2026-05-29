using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackMethod", menuName = "SkillSystem/AttackMethod")]
public class AttackMethod : ScriptableObject
{
    public AttackInfo info;
    public HitboxType type;

    public bool isCheckHit; // hit한 대상 기록 여부
    public bool isGrab;     // 잡기판정인지 여부
    public bool toGrab;     // 잡은 대상에 대한 공격인지 여부

    public bool useAim;
    public Vector3 aimDir;

    public Vector3 positionOffset;
    public Quaternion rotationOffset;
    public Vector3 scale;
    public AttackMovementMethod movementType;

    public List<SpawnRule> spawnRules;
}
