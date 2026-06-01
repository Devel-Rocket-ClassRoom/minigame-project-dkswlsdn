
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private MenuPanel mainPanel;
    private Stack<MenuPanel> prePanels = new Stack<MenuPanel>();
    private MenuPanel currentMenu = null;
    private MenuPanel currentPopup = null;
    [SerializeField] private DialogContainer dialogPanel;
    private InputAction menuOpen;
    private InputAction dialogNext;
    private PlayerInputAction.PlayerActions playerAction;
    private bool isMenuOpen = false;
    private bool isDialogOpen = false;

    [SerializeField] private bool isToggle;
    [SerializeField] private bool isMainAlwaysOpen;


    private void Awake()
    {
        menuOpen = PlayerMovement.Action.Menu.MenuToggle;
        dialogNext = PlayerMovement.Action.Menu.DialogNext;
        playerAction = PlayerMovement.Action.Player;
    }

    private void Update()
    {
        if (isToggle)
        {
            if (menuOpen.WasPressedThisFrame())
            {
                if (isMenuOpen) CloseMenu();
                else TitleOpen();
            }
        }
        else
        {
            if (menuOpen.WasPressedThisFrame()) TitleOpen();
            else if (menuOpen.WasReleasedThisFrame()) CloseMenu();
        }

        if (dialogPanel.isReady && dialogNext.WasPressedThisFrame())
        {
            if (dialogPanel != null && !dialogPanel.Next())
            {
                isDialogOpen = false;
            }
        }

        CursorLock(!isMenuOpen && !isDialogOpen);

        if (isMenuOpen || isDialogOpen) playerAction.Disable();
        else playerAction.Enable();
    }






    public void TitleOpen()
    {
        OpenMenu(mainPanel);
    }

    public void OpenMenu(MenuPanel panel, bool isBack = false)
    {
        if (isDialogOpen) return;

        if (currentMenu != null)
        {
            if (!isBack) prePanels.Push(currentMenu);

            if (isMainAlwaysOpen)
            {
                if (!currentMenu.Equals(mainPanel)) currentMenu.gameObject.SetActive(false);
            }
            else
            {
                currentMenu.gameObject.SetActive(false);
            }
        }
        panel.gameObject.SetActive(true);
        currentMenu = panel;
        isMenuOpen = true;
    }

    public void OpenPopup(MenuPanel panel)
    {
        if (currentPopup != null) currentPopup.gameObject.SetActive(false);
        panel.gameObject.SetActive(true);
        currentPopup = panel;
        isMenuOpen = true;
    }

    public void OpenDialog(string key)
    {
        dialogPanel.gameObject.SetActive(true);
        dialogPanel.StartDialog(key);
        isDialogOpen = true;
    }

    public void CloseMenu()
    {
        prePanels.Clear();
        currentMenu.gameObject.SetActive(false);
        currentMenu = null;
        isMenuOpen = isMainAlwaysOpen;

        if (currentPopup != null) currentPopup.gameObject.SetActive(false);
        
        if (isMainAlwaysOpen)
        {
            currentMenu = mainPanel;
            mainPanel.gameObject.SetActive(true);
        }
    }

    public void BackMenu()
    {
        if (prePanels.Count > 0)
        {
            OpenMenu(prePanels.Pop(), true);
        }
    }

    public void ClosePopup()
    {
        currentPopup.gameObject.SetActive(false);
        currentPopup = null;
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
