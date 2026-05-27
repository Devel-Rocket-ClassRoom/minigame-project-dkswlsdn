using Unity.VisualScripting;
using UnityEngine;

public class PlayerCamera : CharacterCamera
{

    private float rotationX = 0f;
    public float RotationX
    {
        get => rotationX;
        private set
        {
            rotationX = Mathf.Clamp(value, minVerticalRotation, maxVerticalRotation);
        }
    }
    [SerializeField] private float distance;
    [SerializeField] protected Transform playerTransform, lookAtTransform, cam;
    [SerializeField] private bool reverseY, reverseX;
    [SerializeField] private float verticalSpeed, horizontalSpeed, minVerticalRotation, maxVerticalRotation;
    private float vSpeed, hSpeed;


    protected override void Awake()
    {
        base.Awake();
        
    }


    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        vSpeed = verticalSpeed;
        hSpeed = horizontalSpeed;
    }

    private void LateUpdate()
    {
        if (cam == null) return;
        var mouseInput = PlayerMovement.Action.Player.Rotate.ReadValue<Vector2>();
        RotationX -= mouseInput.y * vSpeed * Time.deltaTime;
        lookAtTransform.localRotation = Quaternion.Euler(RotationX, 0, 0);
        if (canRotateCharacter) playerTransform.Rotate(new Vector3(0, mouseInput.x, 0) * hSpeed * Time.deltaTime);
        cam.transform.position = lookAtTransform.position - lookAtTransform.forward * distance;
        cam.transform.LookAt(lookAtTransform);
    }

    public float VerticalRate()
    {
        var diff = maxVerticalRotation - minVerticalRotation;
        var t = (RotationX - minVerticalRotation) / diff;
        return 0.85f - t * (0.85f - 0.3f);
    }

    public override void OnUseSkill(SkillAction action)
    {
        base.OnUseSkill(action);
        hSpeed = action.movementMethod.lookSpeedLimit;
        vSpeed = action.movementMethod.lookSpeedLimit;
    }

    public override void ReturnOrigin()
    {
        base.ReturnOrigin();
        vSpeed = verticalSpeed;
        hSpeed = horizontalSpeed;
    }

    public override void OnStun()
    {
        base.OnStun();
        vSpeed = 0;
        hSpeed = 0;
    }
}
