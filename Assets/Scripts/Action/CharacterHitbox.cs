using UnityEngine;

public class CharacterHitbox : MonoBehaviour
{
    private CapsuleCollider c;

    private void Awake()
    {
        c = GetComponent<CapsuleCollider>();
    }


}
