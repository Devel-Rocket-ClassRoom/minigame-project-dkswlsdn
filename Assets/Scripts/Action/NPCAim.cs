using Unity.Burst.Intrinsics;
using UnityEngine;

public class NPCAim : CharacterAim
{
    public Transform lookAt;

    public override Vector3 GetLookAtVector(DestinationTargettingMethod method, LayerMask targetLayer, float distance, out float y)
    {
        Ray aimRay = new Ray(lookAt.position, lookAt.forward);
        Ray characterRay = new Ray(CAim, transform.forward);
        Vector3 point = Vector3.zero;
        Vector3 dir = Vector3.zero;
        float ly = 0;

        switch (method)
        {
            case DestinationTargettingMethod.LowAngle:
                GetRayPoint(characterRay, distance, groundLayer | targetLayer, out point, out ly);
                y = ly - 1;
                return point;
            case DestinationTargettingMethod.HighAngle:
                if (GetRayPoint(aimRay, distance + 4, groundLayer, out point, out ly))
                {
                    dir = point - CAim;
                    dir.Normalize();
                    GetRayPoint(new Ray(CAim, dir), distance, groundLayer | targetLayer, out point, out ly);
                    y = ly;
                }
                else
                {
                    if (GetRayPoint(new Ray(point, Vector3.down), Mathf.Infinity, groundLayer, out point, out ly))
                        y = ly;
                    else
                        throw new System.Exception("지형을 찾을 수 없음");
                }
                return point;
            case DestinationTargettingMethod.FromCamera:
                GetRayPoint(aimRay, distance + 4, groundLayer, out point, out ly);
                dir = point - CAim;
                dir.Normalize();
                GetRayPoint(new Ray(CAim, dir), distance, groundLayer | targetLayer, out point, out ly);
                y = ly;
                return point;
            default:
                throw new System.Exception("타게팅 방법 오류");
        }
    }
}
