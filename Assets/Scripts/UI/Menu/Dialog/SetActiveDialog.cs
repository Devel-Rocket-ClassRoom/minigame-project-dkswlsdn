using UnityEngine;

public class SetActiveDialog : MonoBehaviour, IDialogEndHandler
{
    [SerializeField] private GameObject go;

    public void OnDialogEnd()
    {
        go.SetActive(true);
        gameObject.SetActive(false);
    }
}
