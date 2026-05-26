using UnityEngine;

public class SetAnchor : MonoBehaviour
{
    [SerializeField] private Anchor anchor;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Ground")
        {
            Instantiate(anchor, transform.position, transform.rotation);
        }
    }
}
