using UnityEngine;

public class CharacterAnchor : MonoBehaviour
{
    public Transform anchor;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public Transform leftFoot;
    public Transform rightFoot;
    public Transform leftWeapon;
    public Transform rightWeapon;

    [Header("모델 교체 시 본 이름으로 재연결 (모든 캐릭터가 동일 리그라 이름 동일)")]
    [Tooltip("비워두면 해당 소켓은 교체 시 건드리지 않는다.")]
    [SerializeField] private string headBone;
    [SerializeField] private string leftHandBone;
    [SerializeField] private string rightHandBone;
    [SerializeField] private string leftFootBone;
    [SerializeField] private string rightFootBone;
    [SerializeField] private string leftWeaponBone;
    [SerializeField] private string rightWeaponBone;

    // 모델 교체 후 새 모델 계층에서 본을 이름으로 다시 찾는다.
    // 무기 비주얼 메시는 모델 프리팹 안 본에 직접 부착되어 모델과 함께 교체되므로,
    // 여기서 다시 잡는 소켓은 주로 스킬 이펙트 스폰 위치용이다.
    public void Rebind(Transform modelRoot)
    {
        if (modelRoot == null) return;

        if (!string.IsNullOrEmpty(headBone)) head = Find(modelRoot, headBone);
        if (!string.IsNullOrEmpty(leftHandBone)) leftHand = Find(modelRoot, leftHandBone);
        if (!string.IsNullOrEmpty(rightHandBone)) rightHand = Find(modelRoot, rightHandBone);
        if (!string.IsNullOrEmpty(leftFootBone)) leftFoot = Find(modelRoot, leftFootBone);
        if (!string.IsNullOrEmpty(rightFootBone)) rightFoot = Find(modelRoot, rightFootBone);
        if (!string.IsNullOrEmpty(leftWeaponBone)) leftWeapon = Find(modelRoot, leftWeaponBone);
        if (!string.IsNullOrEmpty(rightWeaponBone)) rightWeapon = Find(modelRoot, rightWeaponBone);
    }

    private static Transform Find(Transform root, string boneName)
    {
        if (root.name == boneName) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            var found = Find(root.GetChild(i), boneName);
            if (found != null) return found;
        }
        return null;
    }
}
