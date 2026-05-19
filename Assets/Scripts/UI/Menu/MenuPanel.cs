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

    private void Awake()
    {
        manager = transform.root.GetComponent<MenuManager>();
        close.onClick.AddListener(manager.CloseMenu);
        back.onClick.AddListener(manager.BackMenu);

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
