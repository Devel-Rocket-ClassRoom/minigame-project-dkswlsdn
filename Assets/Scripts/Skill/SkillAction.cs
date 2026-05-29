using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "SkillAction", menuName = "Scriptable Objects/SkillAction")]
public class SkillAction : ScriptableObject
{
    public int actionId;
    public float actionTime;
    public float partialCooldown;
    public bool useGrab;
    public AnimationClip clip;

    public MovementMethod movementMethod;
    public List<AttackEntry> attack;
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
public struct AttackEntry
{
    public float preDelay;
    public AttackMethod method;
}

public enum SpawnTrigger  { OnHit, OnExpire }
public enum SpawnPosition { AtHitPoint, AtTarget, AtOrigin }

[Serializable]
public struct SpawnRule
{
    public SpawnTrigger  trigger;
    public SpawnPosition position;
    public float         preDelay;
    public AttackMethod  method; // SO 참조 → 직렬화 순환 없음
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
        useGrab = hit.useGrab;
    }

    [HideInInspector, NonSerialized]
    public Transform origin;
    [HideInInspector, NonSerialized]
    public bool useGrab;
    [HideInInspector, NonSerialized]
    public int id;
    [HideInInspector, NonSerialized]
    public bool isPopup;

    [Header("데미지")]
    public float damage;

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
    public List<SkillCondition> conditions;
    public int count;
    public float life;
    public float preDelay;
    public bool onCanceled;
}