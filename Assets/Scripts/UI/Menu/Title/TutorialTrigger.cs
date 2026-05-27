using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public enum TutorialType { Interaction, Combo }

    [SerializeField] private TutorialManager manager;
    [SerializeField] private TutorialType type;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;

        if (type == TutorialType.Interaction) manager.ShowInteractionTutorial();
        else manager.ShowComboTutorial();
    }
}
