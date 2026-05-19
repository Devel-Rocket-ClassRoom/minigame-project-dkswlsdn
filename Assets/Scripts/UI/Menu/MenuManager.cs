
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private MenuPanel mainPanel;
    private Stack<MenuPanel> prePanels = new Stack<MenuPanel>();
    private MenuPanel current = null;
    private InputAction action;
    private PlayerInputAction.PlayerActions playerAction;
    private bool isMenuOpen = false;


    private void Awake()
    {
        action = PlayerMovement.Action.Menu.MenuToggle;
        playerAction = PlayerMovement.Action.Player;
    }

    private void Update()
    {
        if (action.IsPressed())
        {
            isMenuOpen = true;

            OpenMenu(mainPanel);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        if (isMenuOpen) playerAction.Disable();
        else playerAction.Enable();
    }

    public void OpenMenu(MenuPanel panel, bool isBack = false)
    {
        if (current != null)
        {
            if (!isBack) prePanels.Push(current);
            current.gameObject.SetActive(false);
        }
        panel.gameObject.SetActive(true);
        current = panel;
    }

    public void CloseMenu()
    {
        prePanels.Clear();
        current.gameObject.SetActive(false);
        current = null;
        isMenuOpen = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void BackMenu()
    {
        OpenMenu(prePanels.Pop(), true);
    }
}
