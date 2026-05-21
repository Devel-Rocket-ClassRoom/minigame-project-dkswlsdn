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
