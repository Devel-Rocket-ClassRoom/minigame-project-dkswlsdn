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

    public List<AnimationPhase> animationPhases;
    public MovementMethod movementMethod;
    public List<AttackEntry> attack;
    public int grabImmuneLevel;
    public List<StackMethod> stack;
    public List<EffectEntry> effects;
    public List<CameraShakeEntry> cameraShakes;

    public bool normalizePivotOnCameraAction;
    public List<CameraActionEntry> cameraActions;

    public List<SkillTransition> transitions;
    public float minTransitionTime;
    public float maxTransitionTime;
    public SkillAction autoTransition;

    private void OnValidate()
    {
        if (animationPhases == null) return;
        foreach (var phase in animationPhases)
        {
            if (phase == null) continue;
            if (phase.blendTime == 0f) phase.blendTime = 0.1f;
            if (phase.easingCurve == null || phase.easingCurve.keys.Length == 0)
                phase.easingCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }
    }
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
    public DestinationTargettingMethod targetting;
    public LayerMask targetLayer;

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
    public AttackMethod  method;
}

[Serializable]
public class AttackInfo
{
    public AttackInfo() { }

    public AttackInfo(AttackInfo hit)
    {
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
        useFrozen = hit.useFrozen;
        stackOnHit = hit.stackOnHit;
    }

    [HideInInspector, NonSerialized]
    public bool useGrab;

    [Header("데미지")]
    public float mult;
    public float add;
    public float crit;
    public int grabLevel;

    [Header("카메라")]
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

    [Header("피격 스택")]
    public List<StackMethod> stackOnHit;

    [Header("기타")]
    public float activateTime;
    public bool isDestroyOnCanceled;
    public bool isReleaseGrab;
    public float projectileSpeed;
    public bool isBreakable;
    public bool useFrozen;
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

[Serializable]
public class AnimationPhase
{
    public AnimationClip clip;
    [Tooltip("재생 시작 프레임")]
    public int startFrame;
    [Tooltip("재생 끝 프레임 (0 = 마지막 프레임)")]
    public int endFrame;
    [Tooltip("startFrame에서 endFrame까지 걸리는 시간(초). 0이면 자연 속도")]
    public float duration;
    [Tooltip("진입 블렌드 시간(초)")]
    public float blendTime = 0.1f;
    [Tooltip("이전 페이즈 시작 후 이 페이즈까지 대기 시간(초)")]
    public float delay;
    [Tooltip("x=경과시간 0~1, y=진행도 0~1")]
    public AnimationCurve easingCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
}
