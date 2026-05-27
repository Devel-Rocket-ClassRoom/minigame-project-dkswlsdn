using UnityEngine;
using UnityEngine.TextCore.Text;

public class TutorialInteractionManager : InteractionManager
{
    public bool isInteracted;

    protected override void interaction()
    {
        var hits = Physics.OverlapSphere(transform.position, interactionRadius, interactionLayer);
        if (hits.Length == 0) return;

        var instance = hits[0].GetComponent<Interactor>();
        if (instance == null) return;

        isInteracted = instance.OnDetected(character);
    }
}
