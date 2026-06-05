using UnityEngine;

/// <summary>
/// CharacterSight를 확장해 플레이어 전용 시야 처리를 담당한다.
/// 보이게 됐을 때 대상의 렌더러를 켜고, 안 보이게 됐을 때 끈다.
/// </summary>
public class PlayerSight : CharacterSight
{
    protected override void OnDetected(Character character)
    {
        SetRenderersEnabled(character, true);
    }

    protected override void OnLost(Character character)
    {
        SetRenderersEnabled(character, false);
    }

    private static void SetRenderersEnabled(Character character, bool enabled)
    {
        foreach (var renderer in character.GetComponentsInChildren<Renderer>())
            renderer.enabled = enabled;
    }
}
