using UnityEngine;

public class CharacterAim : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private LayerMask groundLayer;
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

    public float GetLookAtDistance(float distance, out float y)
    {
        Ray ray = cam.ScreenPointToRay(aim.position);

        if (Physics.Raycast(ray, out RaycastHit hit, distance * 2f, groundLayer))
        {
            Vector3 targetPos = hit.point;
            y = hit.point.y;

            Vector3 directionToTarget = (targetPos - transform.position);
            directionToTarget.y = 0;

            float dot = Vector3.Dot(transform.forward, directionToTarget.normalized);

            if (dot > 0)
            {
                float horizontalDist = directionToTarget.magnitude;

                if (horizontalDist < distance)
                {
                    return horizontalDist;
                }
                else
                {
                    return distance;
                }
            }
            else
            {
                y = transform.position.y;
                return 0f;
            }
        }
        else
        {
            Ray forwardRay = new Ray(transform.position, transform.forward);

            if (Physics.Raycast(forwardRay, out RaycastHit forwardHit, distance, stiffLayer))
            {
                y = forwardHit.point.y;
            }
            else
            {
                y = transform.position.y;
            }

            return distance;
        }
    }

    public Vector3 GetLookAtVector(MovementMethod method, Transform character, out float y)
    {
        Ray aimRay = cam.ScreenPointToRay(aim.position);

        if (Physics.Raycast(aimRay, out RaycastHit aimHit, method.distance * 2f, groundLayer))
        {
            Vector3 dir = (aimHit.point - character.position).normalized;
            Ray charRay = new Ray(character.position, dir);

            if (Physics.Raycast(charRay, out RaycastHit charHit, method.distance, groundLayer))
            {
                y = charHit.point.y;
                return charHit.point;
            }
            else
            {
                Vector3 endPoint = character.position + dir * method.distance;
                if (Physics.Raycast(endPoint, Vector3.down, out RaycastHit downHit, Mathf.Infinity, groundLayer))
                {
                    y = downHit.point.y;
                    return downHit.point;
                }
                y = endPoint.y;
                return endPoint;
            }
        }
        else
        {
            Vector3 endPoint = aimRay.origin + aimRay.direction * method.distance * 2f;
            if (Physics.Raycast(endPoint, Vector3.down, out RaycastHit downHit, Mathf.Infinity, groundLayer))
            {
                y = downHit.point.y;
                return downHit.point;
            }
            y = endPoint.y;
            return endPoint;
        }
    }
}
