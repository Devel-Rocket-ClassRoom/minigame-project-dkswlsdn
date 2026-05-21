using UnityEngine;

public class PlayerMovement : CharacterMovement
{
    public static PlayerInputAction action;


    public static PlayerInputAction Action
    {
        get
        {
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
