using System;

public enum CharacterState
{
    Idle,
    Move,
    Skill,
    HitStun,
    Airborne,
    Knockdown,
    WakeUp,
    Groggy,
    Grapped,
    Dead,
    Climb,
}

public enum HitReactionType
{
    None,
    HitStun,
    Airborne,
    Knockdown,
    Groggy,
    Gaurded,
}

[Flags]
public enum ArmorType
{
    None,
    HitStun = 1,
    Airborne = 1 << 1,
    All
}

public enum DistanceCalculateType
{
    Fixed, UseInput, UseAim, Mixed,
}
public enum MovementAction
{
    None,
    Stop,
    Jump,
    Constant,
    Acceleration,
    FreeMove,
}

public enum StatType
{
    HP, MP,
}

public enum ForceDirectionType
{
    Fixed,
    Spread,
    Random,
    World,
}

public enum AttackMovementMethod
{
    Fixed,
    FollowCharacter,
    Teleport,
    Linear,
    Parabola,
}

public enum DestinationTargettingMethod
{
    LowAngle, // 캐릭터에서 사거리만큼
    HighAngle,
    FromCamera, // 카메라에서 사거리 + 10만큼
}

public enum HitboxType
{
    None, Box, Circle, Anchor
}
