using Unity.Cinemachine;
using UnityEngine;

// 스킬 카메라 액션 전담. 별도 vcam 없이 게임플레이 vcam(PlayerVCam)을 직접 보간한다.
//   위치: baseOffset + localOffset * w   (Follow, LockToTarget)
//   회전: Recomposer Pan/Tilt/Dutch * w   (HardLookAt 위에 얹힘)
//   줌:   ZoomScale = lerp(1, zoom, w)
// w=0 == 평소 게임플레이 포즈 → 진입/복귀 seamless (vcam이 하나라 "두 vcam 일치" 문제 없음).
// HardLookAt은 끄지 않으므로 액션 중 플레이어가 회전해도 피벗을 계속 추적한다.
[DefaultExecutionOrder(-50)] // Brain(0)보다 먼저: vcam 파라미터를 Brain이 읽기 전에 갱신
public class CameraActionDirector : MonoBehaviour
{
    public static CameraActionDirector Instance { get; private set; }

    [SerializeField] private CinemachineCamera playerVcam; // 게임플레이 vcam (Recomposer 부착 필요)

    private CinemachineFollow follow;
    private CinemachineRecomposer recomposer;
    private Vector3 baseOffset;

    private CameraActionEntry current;
    private float elapsed;
    private StateManager casterState; // 시전자 상태. 역경직(IsFrozen) 동안 카메라 액션 진행을 멈추기 위해 참조한다.

    // 캔슬/스킬 종료 시 현재 포즈 → 중립 선형 복귀
    private const float ReturnDuration = 0.2f;
    private bool returning;
    private float returnElapsed;
    private float returnStartWeight;

    private void Awake()
    {
        Instance = this;

        if (playerVcam != null)
        {
            follow = playerVcam.GetComponent<CinemachineFollow>();
            recomposer = playerVcam.GetComponent<CinemachineRecomposer>();
            if (follow != null) baseOffset = follow.FollowOffset; // 평소(중립) 오프셋
        }
    }

    private void OnDestroy() { if (Instance == this) Instance = null; }

    private void OnDisable()
    {
        // 액션 중 비활성화되어도 카메라가 틀어진 채 남지 않도록 중립 복귀
        if (current != null) { Apply(null, 0f); current = null; }
        returning = false;
    }

    public void Play(CameraActionEntry entry, StateManager state = null)
    {
        if (entry == null || playerVcam == null) return;
        current = entry;
        casterState = state;
        elapsed = 0f;
        returning = false;
    }

    // 캔슬/스킬 종료 시 호출. 현재 가중치에서 중립(0)으로 0.2초 선형 복귀.
    public void Cancel()
    {
        if (current == null || returning) return;
        returnStartWeight = Weight(current, elapsed);
        returnElapsed = 0f;
        returning = true;
    }

    private void LateUpdate()
    {
        if (current == null) return;

        if (returning)
        {
            returnElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(returnElapsed / ReturnDuration);
            Apply(current, Mathf.Lerp(returnStartWeight, 0f, t)); // 선형 복귀
            if (t >= 1f) { Apply(null, 0f); current = null; returning = false; }
            return;
        }

        // 역경직(IsFrozen) 동안에는 스킬 타임라인이 멈추므로(WaitForSecondsUnfrozen) 카메라 액션도 멈춘다.
        // elapsed를 진행시키지 않고 현재 가중치의 포즈를 그대로 유지 → 스킬 연출과 싱크가 맞는다.
        if (casterState == null || !casterState.IsFrozen)
            elapsed += Time.deltaTime;

        Apply(current, Weight(current, elapsed));

        if (elapsed >= current.blendIn + current.hold + current.blendOut)
        {
            Apply(null, 0f);   // 중립 복귀
            current = null;
        }
    }

    // blendIn(0→1) → hold(1) → blendOut(1→0) 사다리꼴 가중치
    private float Weight(CameraActionEntry e, float t)
    {
        if (t < e.blendIn) return Eval(e, t / Mathf.Max(1e-4f, e.blendIn));
        if (t < e.blendIn + e.hold) return 1f;
        float outT = (t - e.blendIn - e.hold) / Mathf.Max(1e-4f, e.blendOut);
        return Eval(e, 1f - Mathf.Clamp01(outT));
    }

    private float Eval(CameraActionEntry e, float x)
        => (e.ease != null && e.ease.length > 0) ? e.ease.Evaluate(Mathf.Clamp01(x)) : Mathf.Clamp01(x);

    // entry가 null이면 중립(w=0과 동일)으로 복귀
    private void Apply(CameraActionEntry e, float w)
    {
        if (follow != null)
            follow.FollowOffset = baseOffset + (e != null ? e.localOffset * w : Vector3.zero);

        if (recomposer != null)
        {
            recomposer.Pan = e != null ? e.pan * w : 0f;
            recomposer.Tilt = e != null ? e.tilt * w : 0f;
            recomposer.Dutch = e != null ? e.dutch * w : 0f;
            recomposer.ZoomScale = e != null ? Mathf.Lerp(1f, e.zoom, w) : 1f;
        }
    }
}
