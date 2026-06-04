using UnityEngine;

public class DialogOpenner : Interactor
{
    [SerializeField] private string key;

    public override bool OnDetected(Character character)
    {
        var handler = GetComponent<IDialogEndHandler>();
        MenuManager.instance.OpenDialog(key, handler);
        return true;
    }
}
