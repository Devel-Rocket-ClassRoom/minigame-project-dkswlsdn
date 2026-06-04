using Michsky.UI.ModernUIPack;
using UnityEngine;

public class DialogTrigger : MonoBehaviour, IDialogEndHandler
{
    [SerializeField] private string dialogKey;
    [SerializeField] private bool canRepeat = false;
    private bool repeat = true;

    public virtual void OnDialogEnd() { }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (repeat && other.gameObject.CompareTag("Player"))
        {
            repeat = canRepeat;
            MenuManager.instance.OpenDialog(dialogKey, this);
        }
    }
}
