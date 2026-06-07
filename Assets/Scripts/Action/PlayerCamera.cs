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

    // 카메라 정규화(수평): RotationX(에임/논리 pitch)는 건드리지 않고,
    // 카메라 피벗에만 더하는 오프셋(pitchOffset)을 0.2초 선형 보간한다. 에임은 영향 없음.
    private const float NormalizeDuration = 0.2f;
    private float pitchOffset;        // 카메라 pitch에 더하는 오프셋. 0=정상, 액션 끝나면 0으로 복귀
    private float pitchOffsetStart;
    private float pitchOffsetTarget;
    private float pitchOffsetTimer;


    protected override void Awake()
    {
        base.Awake();
        
    }


    private void Start()
    {
        vSpeed = verticalSpeed;
        hSpeed = horizontalSpeed;
    }

    private void LateUpdate()
    {
        if (lookAtTransform == null) return;
        var mouseInput = PlayerMovement.Action.Player.Rotate.ReadValue<Vector2>();

        // 에임/논리 pitch — 항상 입력으로 구동(정규화의 영향을 받지 않음, 에임이 이 값 사용)
        RotationX -= mouseInput.y * vSpeed * Time.deltaTime;

        // 카메라 전용 정규화 오프셋 — 0.2초 선형 보간
        if (pitchOffsetTimer > 0f)
        {
            pitchOffsetTimer -= Time.deltaTime;
            float t = Mathf.Clamp01(1f - pitchOffsetTimer / NormalizeDuration);
            pitchOffset = Mathf.Lerp(pitchOffsetStart, pitchOffsetTarget, t);
        }

        // 피벗(카메라)에는 오프셋 적용, 에임은 RotationX 그대로 → 정규화에도 에임이 안 움직임
        lookAtTransform.localRotation = Quaternion.Euler(RotationX + pitchOffset, 0, 0);
        if (canRotateCharacter) playerTransform.Rotate(new Vector3(0, mouseInput.x, 0) * hSpeed * Time.deltaTime);
    }

    public override void Shake(CameraShakeSettings settings) => CameraShaker.Instance?.Shake(settings);

    public override void PlayCameraAction(CameraActionEntry entry) => CameraActionDirector.Instance?.Play(entry, state);

    public override void CancelCameraAction()
    {
        CameraActionDirector.Instance?.Cancel();
        // 정규화 해제: 카메라 pitch를 다시 에임(RotationX)에 맞춤 → 오프셋 0으로 복귀
        pitchOffsetStart = pitchOffset;
        pitchOffsetTarget = 0f;
        pitchOffsetTimer = NormalizeDuration;
    }

    public override void NormalizePivotPitch()
    {
        // 카메라를 수평으로: cameraPitch = RotationX + offset = 0 이 되도록 오프셋 목표 = -RotationX
        // (RotationX 자체는 안 건드리므로 에임 위치 불변)
        pitchOffsetStart = pitchOffset;
        pitchOffsetTarget = -RotationX;
        pitchOffsetTimer = NormalizeDuration;
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
