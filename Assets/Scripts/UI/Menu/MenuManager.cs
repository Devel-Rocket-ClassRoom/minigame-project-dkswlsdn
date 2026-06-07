
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    // 씬 종속 싱글톤: DontDestroyOnLoad 쓰지 않음.
    // 씬마다 배치된 매니저가 Awake에서 자기를 등록 → 씬 로드 시 자동으로 교체된다.
    public static MenuManager instance { get; private set; }

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
    private bool isAlreadyMenuOpened = false;
    private bool isGameOver = false;

    [SerializeField] private bool isToggle;
    [SerializeField] private bool isMainAlwaysOpen;
    [SerializeField] private MenuPanel gameOverPanel;


    private void Awake()
    {
        instance = this;   // 덮어쓰기(영속 싱글톤의 '중복이면 자살' 가드를 쓰면 안 됨)

        menuOpen = PlayerMovement.Action.Menu.MenuToggle;
        dialogNext = PlayerMovement.Action.Menu.DialogNext;
        playerAction = PlayerMovement.Action.Player;
    }

    private void OnDestroy()
    {
        // 내가 현재 인스턴스일 때만 비운다.
        // (씬 전환 시 새 매니저 Awake가 먼저 돌았다면 그 새 인스턴스를 날리지 않도록)
        if (instance == this) instance = null;
    }

    private void Update()
    {
        if (isGameOver) return;

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
        isAlreadyMenuOpened = isMenuOpen;
        isMenuOpen = true;
    }

    public void OpenDialog(string key, IDialogEndHandler handler = null)
    {
        dialogPanel.gameObject.SetActive(true);
        dialogPanel.StartDialog(key, handler);
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

        // 창을 닫을 때 저장된 선택(deploy)에 맞춰 플레이어 모델/무기 교체.
        // 선택이 바뀌지 않았으면 Swap 내부에서 무시되므로 매번 호출해도 무해하다.
        if (Character.CurrentPlayer != null)
            Character.CurrentPlayer.GetComponent<CharacterModelSwapper>()?.Swap();
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
        isMenuOpen = isAlreadyMenuOpened;
    }

    public void CursorLock(bool lockCursor)
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        OpenMenu(gameOverPanel);
    }
}
