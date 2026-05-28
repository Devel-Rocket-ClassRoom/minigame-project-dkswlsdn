using UnityEngine;

public class ComboTrigger : TutorialTrigger
{
    [SerializeField] private GameObject hud;
    [SerializeField] private GameObject aim;

    protected override void OnTriggerEnter(Collider other)
    {
        if (!isTriggered && other.gameObject.CompareTag("Player"))
        {
            base.OnTriggerEnter(other);
            manager.ShowComboTutorial();
            hud.SetActive(true);
            aim.SetActive(true);
        }
    }
}
