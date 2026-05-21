using System.Net;
using UnityEngine;

public class CharacterAim : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private LayerMask layer;
    [SerializeField] private LayerMask stiffLayer;
    [SerializeField] private Transform aim;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        aim.position = cam.ViewportToScreenPoint(new Vector3(0.5f, playerCamera.VerticalRate(), 0));
    }

    public float GetLookAtDistance(TargettingMethod method, float distance, out float y)
    {
        Vector3 targetPos = GetLookAtVector(method, transform, distance, out y);

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

    public Vector3 GetLookAtVector(TargettingMethod method, Transform origin, float distance, out float y)
{
    Ray aimRay = cam.ScreenPointToRay(aim.position);
    float current = transform.position.y;
    distance = distance + 10f;

    if (Physics.Raycast(aimRay, out RaycastHit aimHit, distance, layer))
    {
        if (method.useOnlyCamera)
        {
            y = aimHit.point.y - current;
            return aimHit.point;
        }
        else
        {
            // 맞은 지점에서 아래로 레이
            if (Physics.Raycast(aimHit.point + Vector3.up * 0.1f, Vector3.down, out RaycastHit downHit, Mathf.Infinity, layer))
            {
                if (method.isHighAngle)
                {
                    y = downHit.point.y - current;
                    return downHit.point;
                }
                else
                {
                    // origin에서 downHit 지점까지 레이
                    Vector3 dir = (downHit.point - origin.position + Vector3.up).normalized;
                    float dist = Vector3.Distance(origin.position + Vector3.up, downHit.point);

                    if (Physics.Raycast(origin.position + Vector3.up, dir, out RaycastHit charHit, dist, layer))
                    {
                        y = charHit.point.y - current;
                        return charHit.point;
                    }
                    else
                    {
                        y = downHit.point.y - current;
                        return downHit.point;
                    }
                }
            }
            else
            {
                throw new System.Exception("지형을 찾을 수 없음");
            }
        }
    }
    else
    {
        Vector3 endPoint = aimRay.origin + aimRay.direction * distance;

        if (method.useOnlyCamera)
        {
            y = endPoint.y - current;
            return endPoint;
        }
        else
        {
            if (Physics.Raycast(endPoint, Vector3.down, out RaycastHit downHit, Mathf.Infinity, layer))
            {
                if (method.isHighAngle)
                {
                    y = downHit.point.y - current;
                    return downHit.point;
                }
                else
                {
                    // origin에서 downHit 지점까지 레이
                    Vector3 dir = (downHit.point - origin.position + Vector3.up).normalized;
                    float dist = Vector3.Distance(origin.position + Vector3.up, downHit.point);

                    if (Physics.Raycast(origin.position + Vector3.up, dir, out RaycastHit charHit, dist, layer))
                    {
                        y = charHit.point.y - current;
                        return charHit.point;
                    }
                    else
                    {
                        y = downHit.point.y - current;
                        return downHit.point;
                    }
                }
            }
            else
            {
                throw new System.Exception("지형을 찾을 수 없음");
            }
        }
    }
}
}
