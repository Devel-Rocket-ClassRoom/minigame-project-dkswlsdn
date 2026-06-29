using UnityEngine;

public class PopupTrigger : MonoBehaviour
{
    [SerializeField] private MenuPanel popup;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            MenuManager.instance.OpenPopup(popup);
        }
    }
}
