using UnityEngine;

public class OpenExitPanel : Interactor
{
    [SerializeField] private MenuPanel panel;

    public override bool OnDetected(Character character)
    {
        var manager = panel.transform.root.GetComponent<MenuManager>();
        manager.OpenMenu(panel);
        return true;
    }
}
