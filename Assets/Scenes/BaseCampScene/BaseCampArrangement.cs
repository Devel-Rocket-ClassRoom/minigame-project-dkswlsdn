using Unity.VisualScripting;
using UnityEngine;

public class BaseCampArrangement : MonoBehaviour
{
    [SerializeField] private GameObject tutorialClear;

    private void Start()
    {
        if (!SaveManager.CurrentSave.isTutorialCleared)
        {
            tutorialClear.SetActive(true);
            Character.CurrentPlayer.State.ChangeState(CharacterState.Airborne);
            Character.CurrentPlayer.transform.position = tutorialClear.transform.position + Vector3.up * 6;
            SaveManager.CurrentSave.isTutorialCleared = true;
            SaveManager.SaveRequest();
        }
    }
}
