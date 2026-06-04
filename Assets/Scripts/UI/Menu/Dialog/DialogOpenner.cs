using UnityEngine;

public class DialogOpenner : Interactor
{
    [SerializeField] private MenuManager manager;
    [SerializeField] private string key;

    public override bool OnDetected(Character character)
    {
        var handler = GetComponent<IDialogEndHandler>();
        manager.OpenDialog(key, handler);
        return true;
    }
}
