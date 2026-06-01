using UnityEngine;

public class DialogOpenner : Interactor
{
    [SerializeField] private MenuManager manager;
    [SerializeField] private string key;

    public override bool OnDetected(Character character)
    {
        manager.OpenDialog(key);
        return true;
    }
}
