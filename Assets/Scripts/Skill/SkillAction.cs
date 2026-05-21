using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillAction", menuName = "Scriptable Objects/SkillAction")]
public class SkillAction : ScriptableObject
{
    public int actionId;
    public MovementMethod movementMethod;
    public List<AttackMethod> attack;
    public TargettingMethod targetting;
    public float aimDistance;
    public bool useGrab;
    public float actionTime;
    public float minTransitionTime;
    public float maxTransitionTime;
    public List<SkillTransition> transitions;
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
    public Attack hitbox;
    public float preDelay;

    public bool isCheckHit;
    public bool isGrab;
    public bool toGrab;

    public bool useAim;
    public Vector3 aimDir;

    public Vector3 positionOffset;
    public Quaternion rotationOffset;
    public AttackMovementMethod movementType;
}

[Serializable]
public struct TargettingMethod
{
    public bool isHighAngle;
    public bool useOnlyCamera;
}
