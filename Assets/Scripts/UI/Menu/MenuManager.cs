
using System;
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
    [SerializeField] private bool isToggle;
    [SerializeField] private bool isMainAlwaysOpen;


    private void Awake()
    {
        action = PlayerMovement.Action.Menu.MenuToggle;
        playerAction = PlayerMovement.Action.Player;
    }

    private void Update()
    {
        if (isToggle)
        {
            if (action.WasPressedThisFrame())
            {
                if (isMenuOpen) CloseMenu();
                else TitleOpen();
            }
        }
        else
        {
            if (action.WasPressedThisFrame()) TitleOpen();
            else if (action.WasReleasedThisFrame()) CloseMenu();
        }

        CursorLock(!isMenuOpen);

        if (isMenuOpen) playerAction.Disable();
        else playerAction.Enable();
    }

    public void TitleOpen()
    {
        OpenMenu(mainPanel);
    }

    public void OpenMenu(MenuPanel panel, bool isBack = false)
    {
        if (current != null)
        {
            if (!isBack) prePanels.Push(current);

            if (isMainAlwaysOpen)
            {
                if (!current.Equals(mainPanel)) current.gameObject.SetActive(false);
            }
            else
            {
                current.gameObject.SetActive(false);
            }
        }
        panel.gameObject.SetActive(true);
        current = panel;
        isMenuOpen = true;
    }

    public void CloseMenu()
    {
        prePanels.Clear();
        current.gameObject.SetActive(false);
        current = null;
        isMenuOpen = isMainAlwaysOpen;
        
        if (isMainAlwaysOpen)
        {
            current = mainPanel;
            mainPanel.gameObject.SetActive(true);
        }
    }

    public void BackMenu()
    {
        OpenMenu(prePanels.Pop(), true);
    }

    public void CursorLock(bool lockCursor)
    {
        if (lockCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
