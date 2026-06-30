using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class NPCAim : CharacterAim
{
    public Transform lookAt;

    public override Vector3 GetLookAtVector(DestinationTargettingMethod method, LayerMask targetLayer, float distance, out float y, out Transform character)
    {
        Ray aimRay = new Ray(lookAt.position, lookAt.forward);
        Ray characterRay = new Ray(CAim, transform.forward);
        Vector3 point = Vector3.zero;
        Vector3 dir = Vector3.zero;
        float ly = 0;
        character = null;

        switch (method)
        {
            case DestinationTargettingMethod.LowAngle:
                GetRayPoint(characterRay, distance, targetLayer, out point, out ly, out character);
                y = ly - 1;
                return point;
            case DestinationTargettingMethod.HighAngle:
                if (GetRayPoint(aimRay, distance, groundLayer, out point, out ly, out _))
                {
                    GetRayPoint(aimRay, distance, groundLayer, out point, out ly, out character);
                    y = ly;
                }
                else
                {
                    if (GetRayPoint(new Ray(point, Vector3.down), Mathf.Infinity, groundLayer, out point, out ly, out _))
                        y = ly;
                    else
                        throw new System.Exception("지형을 찾을 수 없음");
                }
                return point;
            case DestinationTargettingMethod.FromCamera:
                GetRayPoint(aimRay, distance, targetLayer, out point, out ly, out character);
                y = ly;
                return point;
            default:
                throw new System.Exception("타게팅 방법 오류");
        }
    }
}
