using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] protected TutorialManager manager;
    protected bool isTriggered;

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (isTriggered || !other.CompareTag("Player")) return;
        isTriggered = true;
    }
}
