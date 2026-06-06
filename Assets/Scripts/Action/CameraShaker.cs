using UnityEngine;

// CinemachineBrain이 Main Camera 포즈를 확정한 "뒤"에 흔들림 오프셋을 더한다.
// 실행 순서를 Brain보다 뒤로 두어야 흔들림이 덮어써지지 않는다.
// 정책: 단일 활성 흔들림 + max — 새 흔들림의 세기가 진행 중인 것의 현재 세기보다 크면 교체, 아니면 무시.
[DefaultExecutionOrder(10000)]
public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance { get; private set; }

    private CameraShakeSettings current;
    private float elapsed;

    private void Awake() => Instance = this;
    private void OnDestroy() { if (Instance == this) Instance = null; }

    public void Shake(CameraShakeSettings settings)
    {
        if (settings == null || settings.duration <= 0f) return;

        // max: 진행 중인 흔들림의 "현재" 세기보다 새 흔들림 피크가 약하면 무시
        if (current != null)
        {
            float t = elapsed / current.duration;
            if (settings.amplitude < current.AmplitudeAt(t)) return;
        }

        current = settings;
        elapsed = 0f;
    }

    private void LateUpdate()
    {
        if (current == null) return;

        elapsed += Time.deltaTime;
        float t = elapsed / current.duration;
        if (t >= 1f)
        {
            current = null;
            return;
        }

        // Brain이 매 프레임 베이스 포즈를 새로 써주므로, 더하기만 해도 누적되지 않는다.
        Vector3 offset = current.Evaluate(t);
        if (offset != Vector3.zero)
            transform.position += transform.rotation * offset; // 카메라 로컬 공간
    }
}
