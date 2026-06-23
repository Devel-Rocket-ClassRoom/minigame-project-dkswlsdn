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
    MidAir,
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

public enum RangeType
{
    None,
    Short,
    Long,
    Middle,
}

[Flags]
public enum ArmorType
{
    None,
    Short = 1,
    Long = 1 << 1,
    Full = 1 << 2,
    All = Short | Long | Full,
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
    Attack,
    Critical,
    Health,
    Defense,
    Dodgy,
    Speed,
    Carry,
}

// enum 값이 곧 세이브의 magicOpenedSkill[] 배열 인덱스다.
// (L=0, R=1, SL=2, LR=3, E=4, F=5, Space=6, Passive=7, Q=8(서브웨폰))
// SkillButton.MagicIndex 는 (int)skillKey 를 그대로 사용하므로 순서를 바꾸면 안 된다.
// Q(서브웨폰)는 기존 인덱스 보존을 위해 항상 마지막에 둔다.
public enum SkillKey
{
    L,
    R,
    SL,
    LR,
    E,
    F,
    Space,
    Passive,
    Q,
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

public enum InstantiateMethod
{
    Aim, Weapon, Hand, Foot, Attack, Character,
}

public enum StringCategory
{
    Character,
    Item,
    Weapon,
    Dialogue
}

public enum Languages
{
    Korean,
    English,
    Japanese,
}