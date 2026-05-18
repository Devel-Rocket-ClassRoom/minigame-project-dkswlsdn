using Unity.VisualScripting;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private StateManager state;
    private SkillCaster caster;


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
    [SerializeField] private bool reverseY, reverseX, canRotateCharacter;
    [SerializeField] private float verticalSpeed, horizontalSpeed, minVerticalRotation, maxVerticalRotation;
    private float vSpeed, hSpeed;


    private void Awake()
    {
        state = GetComponent<StateManager>();
        caster = GetComponent<SkillCaster>();

        state.onIdle += ReturnOrigin;
        state.onWakeUp += ReturnOrigin;
        state.onHitstun += OnStun;
        state.onAirborne += OnStun;
        state.onKnockdown += OnStun;
        state.onGroggy += OnStun;
        state.onGrab += OnStun;
        state.onDead += OnStun;

        caster.onActionStart += OnUseSkill;
        caster.onSkillEnd += ReturnOrigin;
    }


    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        vSpeed = verticalSpeed;
        hSpeed = horizontalSpeed;
    }

    private void  LateUpdate()
    {
        if (cam == null) return;
        var mouseInput = PlayerMovement.Action.Player.Rotate.ReadValue<Vector2>();
        RotationX -= mouseInput.y  * vSpeed * Time.deltaTime;
        lookAtTransform.localRotation = Quaternion.Euler(RotationX, 0, 0);
        if (canRotateCharacter) playerTransform.Rotate(new Vector3(0, mouseInput.x, 0) * hSpeed * Time.deltaTime);
        cam.transform.position = lookAtTransform.position - lookAtTransform.forward * distance;
        cam.transform.LookAt(lookAtTransform);
    }

    public float VerticalRate()
    {
        var diff = maxVerticalRotation - minVerticalRotation;
        var offset = diff * 0.33f;
        var x = 45f - RotationX + offset;
        return x / (diff + offset);
    }

    public void OnUseSkill(MovementMethod method, float time)
    {
        hSpeed = method.lookSpeedLimit;
        vSpeed = method.lookSpeedLimit;
    }

    public void ReturnOrigin()
    {
        vSpeed = verticalSpeed;
        hSpeed = horizontalSpeed;
        canRotateCharacter = true;
    }

    public void OnStun()
    {
        vSpeed = 0;
        hSpeed = 0;
        canRotateCharacter = false;
    }
}
