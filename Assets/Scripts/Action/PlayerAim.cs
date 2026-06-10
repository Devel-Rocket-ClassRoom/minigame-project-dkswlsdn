using UnityEngine;

public class PlayerAim : CharacterAim
{
    private Camera cam;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private Transform aim;
    [SerializeField] private Transform lookAt;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        aim.position = cam.ViewportToScreenPoint(new Vector3(0.5f, playerCamera.VerticalRate(), 0));
    }

    public override Vector3 GetLookAtVector(DestinationTargettingMethod method, LayerMask targetLayer, float distance, out float y, out Transform character)
    {
        float add = Vector3.Distance(lookAt.position, cam.ScreenToWorldPoint(aim.position));
        Ray aimRay = cam.ScreenPointToRay(aim.position);
        Ray characterRay = new Ray(CAim, transform.forward);
        Vector3 point = Vector3.zero;
        Vector3 dir = Vector3.zero;
        float ly = 0;
        character = null;

        switch (method)
        {
            case DestinationTargettingMethod.LowAngle:
                if (GetRayPoint(characterRay, distance, groundLayer, out point, out ly, out _))
                {
                    GetRayPoint(characterRay, distance, targetLayer, out point, out ly, out character);
                }
                y = ly - 1;
                return point;
            case DestinationTargettingMethod.HighAngle:
                if (GetRayPoint(aimRay, distance + add, groundLayer, out point, out ly, out _))
                {
                    GetRayPoint(aimRay, distance + add, targetLayer, out point, out ly, out character);
          
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
                GetRayPoint(aimRay, distance + add, groundLayer, out point, out ly, out _);
                dir = point - CAim;
                dir.Normalize();
                GetRayPoint(new Ray(CAim, dir), distance, targetLayer, out point, out ly, out character);
                y = ly;
                return point;
            default:
                throw new System.Exception("타게팅 방법 오류");
        }
    }
}
