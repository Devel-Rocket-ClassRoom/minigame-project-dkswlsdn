using UnityEngine;

// 스킬 카메라 액션 1회분 데이터. 액션 vcam을 "중립(=게임플레이 포즈)"에서 목표 포즈로
// 가중치 w(0~1)로 보간한다. w는 blendIn→hold→blendOut 사다리꼴.
//   위치: baseOffset + localOffset * w   (피벗 로컬 프레임, LockToTarget)
//   회전: Recomposer Pan/Tilt/Dutch * w   (HardLookAt 위에 얹힘)
//   줌:   ZoomScale = lerp(1, zoom, w)
[System.Serializable]
public class CameraActionEntry
{
    public float preDelay;

    [Header("위치 (피벗 기준 오프셋)")]
    public Vector3 localOffset;

    [Header("회전 오프셋 (피벗을 보는 것 기준 추가)")]
    public float pan;       // 좌우 yaw
    public float tilt;      // 상하 pitch
    public float dutch;     // 기울임 roll
    public float zoom = 1f; // <1 줌인, >1 줌아웃

    [Header("보간")]
    public float blendIn = 0.1f;
    public float hold = 0.2f;
    public float blendOut = 0.2f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // 중립→목표 가중치 곡선
}
