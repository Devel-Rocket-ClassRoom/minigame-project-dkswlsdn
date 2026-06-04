using UnityEngine;

public class OpenPopupPanel : Interactor
{
    [SerializeField] private MenuPanel panel;

    public override bool OnDetected(Character character)
    {
        MenuManager.instance.OpenPopup(panel);
        return true;
    }
}
