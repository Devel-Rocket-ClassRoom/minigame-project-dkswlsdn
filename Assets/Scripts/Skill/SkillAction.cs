using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillAction", menuName = "Scriptable Objects/SkillAction")]
public class SkillAction : ScriptableObject
{
    public int actionId;
    public MovementMethod movementMethod;
    public List<AttackMethod> attack;
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
    public bool startToMove;
    public bool directionReverse;
    public bool rightSide;
    public bool canFreeMove;

    [Header("수평이동")]
    public float distance;
    public float neutralDistance;
    public float backwardDistance;
    public bool followTerrain;
    public bool useInputDirection;

    [Header("높이차이")]
    public float maxAltitude;
    public float minAltitude;
    public float verticalSpeed;

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

    public bool isGrab;

    public bool useAim;

    public Vector3 positionOffset;
    public Quaternion rotationOffset;
}
