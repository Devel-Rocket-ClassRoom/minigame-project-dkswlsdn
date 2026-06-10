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
    [Tooltip("모션 진입 시 크로스페이드 보간 시간(초). 0이면 즉시 전환")]
    public float blendTime;

    public MovementMethod movementMethod;
    public List<AttackEntry> attack;
    public int grabImmuneLevel;
    public List<StackMethod> stack;
    public List<EffectEntry> effects;
    public List<CameraShakeEntry> cameraShakes;

    public bool normalizePivotOnCameraAction;
    public List<CameraActionEntry> cameraActions;

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
    public bool useTargetting;

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

[Serializable]
public class CameraShakeEntry
{
    public float preDelay;
    public CameraShakeSettings settings;
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
        mult = hit.mult;
        add = hit.add;
        crit = hit.crit;
        cameraShake = hit.cameraShake;
        effects = hit.effects;
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
        isBreakable = hit.isBreakable;
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
    public float mult;
    public float add;
    public float crit;
    public int grabLevel;

    [Header("카메라")]
    [Tooltip("적중 시 발생할 카메라 흔들림. 비워두면 흔들림 없음")]
    public CameraShakeSettings cameraShake;

    [Header("경직")]
    public HitReactionType reaction;
    public RangeType range;
    public float stunDuration;
    public float stunForce;
    public Vector2 airborneForce;
    public ForceDirectionType forceDirectionType;
    public float fixedStun;
    public float reverseStun;

    [Header("이펙트")]
    public List<EffectEntry> effects;

    [Header("기타")]
    public float activateTime;
    public bool isDestroyOnCanceled;
    public bool isReleaseGrab;
    public float projectileSpeed;
    public bool isBreakable;
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

[Serializable]
public class EffectEntry
{
    public EffectData effect;
    public float preDelay;
    public InstantiateMethod method;
    public bool isLeft;
}
