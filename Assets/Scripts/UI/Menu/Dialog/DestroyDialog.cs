using UnityEngine;

public class DestroyDialog : MonoBehaviour, IDialogEndHandler
{
    [SerializeField] private GameObject wall;

    public void OnDialogEnd()
    {
        Destroy(wall.gameObject);
        Destroy(gameObject);
    }
}
