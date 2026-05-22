using UnityEngine;

public class CharacterAim : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask stiffLayer;
    [SerializeField] private Transform aim;

    private Vector3 CAim { get { return transform.position + Vector3.up; } }

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        aim.position = cam.ViewportToScreenPoint(new Vector3(0.5f, playerCamera.VerticalRate(), 0));
    }

    public float GetLookAtDistance(DestinationTargettingMethod method, LayerMask targetLayer, float distance, out float y)
    {
        Vector3 targetPos = GetLookAtVector(method, targetLayer, distance, out y);

        Vector3 directionToTarget = targetPos - transform.position;
        directionToTarget.y = 0;

        float dot = Vector3.Dot(transform.forward, directionToTarget.normalized);

        if (dot > 0)
        {
            float horizontalDist = directionToTarget.magnitude;
            return Mathf.Min(horizontalDist, distance);
        }
        else
        {
            y = 0f;
            return 0f;
        }
    }

    public Vector3 GetLookAtVector(DestinationTargettingMethod method, LayerMask targetLayer, float distance, out float y)
    {
        Ray aimRay = cam.ScreenPointToRay(aim.position);
        Ray characterRay = new Ray(CAim, transform.forward);
        float current = transform.position.y;
        distance = distance + 10f;
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
                if (GetRayPoint(aimRay, distance + 10, groundLayer, out point, out ly))
                {
                    dir = point - CAim;
                    dir.Normalize();
                    GetRayPoint(new Ray(CAim, dir), distance, groundLayer | targetLayer, out point, out ly);
                    y = ly;
                }
                else
                {
                    if (GetRayPoint(new Ray(point, Vector3.down), Mathf.Infinity, groundLayer, out point, out ly))
                    {
                        y = ly;
                    }
                    else
                        throw new System.Exception("지형을 찾을 수 없음");
                }
                return point;
            case DestinationTargettingMethod.FromCamera:
                GetRayPoint(aimRay, distance + 10f, groundLayer, out point, out ly);

                dir = point - CAim;
                dir.Normalize();
                GetRayPoint(new Ray(CAim, dir), distance, groundLayer | targetLayer, out point, out ly);
                y = ly;
                return point;
            default:
                throw new System.Exception("타게팅 방법 오류");
        }
    }

    private bool GetRayPoint(Ray ray, float distance, LayerMask layer, out Vector3 point, out float y)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, distance, layer))
        {
            point = hit.point;
            y = point.y - ray.origin.y;
            return true;
        }
        else
        {
            point = ray.origin + ray.direction * distance;
            y = point.y - ray.origin.y;
            return false;
        }
    }
}
