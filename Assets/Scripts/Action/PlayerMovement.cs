using UnityEngine;

public class PlayerMovement : CharacterMovement
{
    public static PlayerInputAction action;


    public static PlayerInputAction Action
    {
        get
        {
            // 아직 생성이 안 되었다면 이때 생성 (유니티 API 호출이 안전한 시점)
            if (action == null)
            {
                action = new PlayerInputAction();
            }
            return action;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        Action.Enable();
    }
}
