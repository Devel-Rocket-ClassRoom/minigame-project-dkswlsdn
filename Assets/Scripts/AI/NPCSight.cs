using UnityEngine;

/// <summary>
/// NPC 전용 시야 처리.
/// 감지/놓침 이벤트를 AIBrain의 메서드로 전달한다.
/// </summary>
public class NPCSight : CharacterSight
{
    private AIBrain brain;

    protected override void Awake()
    {
        base.Awake();
        brain = GetComponentInParent<AIBrain>();
    }

    protected override void OnDetected(Character character)
    {
        brain.OnDetected(character);
    }

    protected override void OnLost(Character character)
    {
        brain.OnLost(character);
    }
}
