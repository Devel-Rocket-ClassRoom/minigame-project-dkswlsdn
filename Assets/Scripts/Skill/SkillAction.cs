using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillAction", menuName = "Scriptable Objects/SkillAction")]
public class SkillAction : ScriptableObject
{
    public int actionId;
    public float actionTime;
    public bool useGrab;

    public MovementMethod movementMethod;
    public List<AttackMethod> attack;
    public List<StackMethod> stack;

    public DestinationTargettingMethod targetting;
    public LayerMask targetLayer;
    public float aimDistance;

    public List<SkillTransition> transitions;
    public float minTransitionTime;
    public float maxTransitionTime;
    public SkillAction autoTransition;
}

[Serializable]
public class SkillTransition
{
    public List<SkillCondition> conditions;
    public SkillAction nextAction;
    public bool immediateTransition;
}

[Serializable]
public struct MovementMethod
{
    public DistanceCalculateType calcType;

    [Header("공통")]
    public float gravity;
    public float friction;
    public bool isKeepSpeed;
    public bool directionReverse;
    public bool rightSide;
    public bool canFreeMove;
    public float freeMoveSpeed;

    [Header("수평이동")]
    public float distance;
    public float neutralDistance;
    public float backwardDistance;
    public bool isFrictionAutoCalc;
    public bool followTerrain;
    public bool useInputDirection;
    public bool isStaticDirection;

    [Header("높이차이")]
    public bool useAltitudeModifire;
    public float maxAltitude;
    public float minAltitude;
    public float verticalSpeed;
    public bool isVerticalSpeedAutoCalc;

    [Header("점프")]
    public float jumpHeight;
    public float minJumpHeight;
    public float regularTime;

    [Header("카메라")]
    public float yawLimit;
    public bool cameraLocked;
    public float lookSpeedLimit;
}

[Serializable]
public struct AttackMethod
{
    public AttackInfo info;
    public HitboxType type;
    public float preDelay;

    public bool isCheckHit; // hit한 대상 기록 여부
    public bool isGrab; // 잡기판정인지 여부
    public bool toGrab; // 잡은 대상에 대한 공격인지 여부

    public bool useAim;
    public Vector3 aimDir;

    public Vector3 positionOffset;
    public Quaternion rotationOffset;
    public Vector3 scale;
    public AttackMovementMethod movementType;

    public List<SpawnRule> spawnRules;
}

public enum SpawnTrigger  { OnHit, OnExpire }
public enum SpawnPosition { AtHitPoint, AtTarget, AtOrigin }

[Serializable]
public struct SpawnRule
{
    public SpawnTrigger  trigger;
    public SpawnPosition position;
    public AttackMethod  method;
}

[Serializable]
public class AttackInfo
{
    public AttackInfo() { }

    public AttackInfo(AttackInfo hit)
    {
        origin = hit.origin;
        id = hit.id;
        isPopup = hit.isPopup;
        damage = hit.damage;
        stack = hit.stack;
        count = hit.count;
        reaction = hit.reaction;
        range = hit.range;
        stunDuration = hit.stunDuration;
        stunForce = hit.stunForce;
        airborneForce = hit.airborneForce;
        forceDirectionType = hit.forceDirectionType;
        fixedStun = hit.fixedStun;
        reverseStun = hit.reverseStun;
        activateTime = hit.activateTime;
        isDestroyOnCanceled = hit.isDestroyOnCanceled;
        isReleaseGrab = hit.isReleaseGrab;
        projectileSpeed = hit.projectileSpeed;
    }

    [HideInInspector, NonSerialized]
    public Transform origin;
    [HideInInspector, NonSerialized]
    public int id;
    [HideInInspector, NonSerialized]
    public bool isPopup;

    [Header("데미지")]
    public float damage;
    public SpecialStackData stack;
    public int count;

    [Header("경직")]
    public HitReactionType reaction;
    public RangeType range;
    public float stunDuration;
    public float stunForce;
    public Vector2 airborneForce;
    public ForceDirectionType forceDirectionType;
    public float fixedStun;
    public float reverseStun;

    [Header("기타")]
    public float activateTime;
    public bool isDestroyOnCanceled;
    public bool isReleaseGrab;

    public float projectileSpeed;
}


[Serializable]
public struct StackMethod
{
    public SpecialStackData stack;
    public int count;
    public float life;
    public float preDelay;
    public bool onCanceled;
}