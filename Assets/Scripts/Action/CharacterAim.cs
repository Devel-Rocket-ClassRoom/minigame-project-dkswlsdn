using UnityEngine;

public abstract class CharacterAim : MonoBehaviour
{
    [SerializeField] protected LayerMask groundLayer;
    [SerializeField] protected LayerMask stiffLayer;

    protected Vector3 CAim => transform.position + Vector3.up * 1.6f;

    public float GetLookAtDistance(DestinationTargettingMethod method, LayerMask targetLayer, float distance, out float y)
    {
        Vector3 targetPos = GetLookAtVector(method, targetLayer, distance, out y, out _);

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

    public abstract Vector3 GetLookAtVector(DestinationTargettingMethod method, LayerMask targetLayer, float distance, out float y, out Transform character);

    protected bool GetRayPoint(Ray ray, float distance, LayerMask layer, out Vector3 point, out float y, out Transform character)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, distance, layer))
        {
            point = hit.point;
            y = point.y - ray.origin.y;
            character = hit.transform;
            Debug.Log(0);
            return true;
        }
        else
        {
            point = ray.origin + ray.direction * distance;
            y = point.y - ray.origin.y;
            character = null;
            return false;
        }
    }
}
