using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    private MenuManager manager;

    [SerializeField] private List<MenuButton> buttons;
    [SerializeField] private Button close;
    [SerializeField] private Button back;
    [SerializeField] private Button closePopup;

    private void Awake()
    {
        manager = transform.root.GetComponent<MenuManager>();
        if (close != null) close.onClick.AddListener(manager.CloseMenu);
        if(back != null) back.onClick.AddListener(manager.BackMenu);
        if(closePopup != null) closePopup.onClick.AddListener(manager.ClosePopup);

        foreach (var button in buttons)
        {
            button.button.onClick.AddListener(() => manager.OpenMenu(button.menu));
        }
    }
}

[Serializable]
public class MenuButton
{
    public Button button;
    public MenuPanel menu;
}
