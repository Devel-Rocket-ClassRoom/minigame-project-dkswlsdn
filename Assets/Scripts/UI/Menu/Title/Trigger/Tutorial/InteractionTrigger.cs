using UnityEngine;

public class InteractionTrigger : TutorialTrigger
{
    protected override void OnTriggerEnter(Collider other)
    {
        if (!isTriggered && other.gameObject.CompareTag("Player"))
        {
            base.OnTriggerEnter(other);
            manager.ShowInteractionTutorial();
        }
    }
}
