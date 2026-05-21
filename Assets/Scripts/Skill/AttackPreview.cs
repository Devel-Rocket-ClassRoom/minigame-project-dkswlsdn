using UnityEngine;

public class AttackPreview : MonoBehaviour
{
    [SerializeField] private Attack[] hitboxes;

    public HitboxType type;
    public Vector3 positionOffset;
    public Vector3 rotation;
    public Vector3 scale = Vector3.one;

    public Attack GetPrefab(HitboxType hitboxType)
    {
        int idx = (int)hitboxType;
        if (hitboxType == HitboxType.None || idx >= hitboxes.Length) return null;
        return hitboxes[idx];
    }
}
