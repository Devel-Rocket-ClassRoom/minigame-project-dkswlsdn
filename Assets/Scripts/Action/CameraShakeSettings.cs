using UnityEngine;

// 카메라 흔들기 1회분 설정. 감쇠 사인파 기반.
//   offset(t) = direction * amplitude * sin(2π * oscillations * t) * envelope(t)
//   (t는 0~1로 정규화된 진행도)
// 재사용 가능한 직렬화 타입 — 인스펙터 필드로 두거나 스킬 데이터에 넣어 데이터 주도로 쓸 수 있다.
[System.Serializable]
public class CameraShakeSettings
{
    [Tooltip("흔들림 방향(화면 평면, x=오른쪽 y=위, 자동 정규화). 기본값 우상단 60°(π/3)")]
    public Vector2 direction = new Vector2(0.5f, 0.8660254f); // (cos60°, sin60°)

    [Tooltip("세기 — 사인파 피크에서의 변위(월드 유닛)")]
    public float amplitude = 0.05f;

    [Tooltip("왕복 횟수 — 지속시간 동안의 사인 주기 수. 1 = 한 번 갔다 옴")]
    public float oscillations = 1f;

    [Tooltip("지속시간(초)")]
    public float duration = 0.1f;

    [Tooltip("진폭 배율 엔벨로프(가로축 0~1 정규화 시간). 평평=감쇠없음, 우하향=감쇠, 우상향=점점 강해짐")]
    public AnimationCurve envelope = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    // 정규화 시간 t(0~1)에서의 로컬 공간 오프셋. Vector2(x,y) → Vector3(x,y,0)
    public Vector3 Evaluate(float t)
    {
        float env = (envelope != null && envelope.length > 0) ? envelope.Evaluate(t) : 1f;
        float wave = Mathf.Sin(2f * Mathf.PI * oscillations * t);
        return (Vector3)(direction.normalized * (amplitude * wave * env));
    }

    // 해당 시점의 진폭 크기(사인 제외) — max 정책에서 흔들림 세기 비교용
    public float AmplitudeAt(float t)
    {
        float env = (envelope != null && envelope.length > 0) ? envelope.Evaluate(t) : 1f;
        return amplitude * Mathf.Abs(env);
    }
}
