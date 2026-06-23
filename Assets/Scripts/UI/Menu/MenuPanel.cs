using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    [SerializeField] private List<MenuButton> buttons;
    [SerializeField] private Button close;
    [SerializeField] private Button back;
    [SerializeField] private Button closePopup;

    private void Awake()
    {
        // 리스너는 클릭 시점에 instance를 참조한다(Awake 실행 순서에 의존하지 않음)
        if (close != null) close.onClick.AddListener(() => MenuManager.instance.CloseMenu());
        if (back != null) back.onClick.AddListener(() => MenuManager.instance.BackMenu());
        if (closePopup != null) closePopup.onClick.AddListener(() => MenuManager.instance.ClosePopup());

        foreach (var button in buttons)
        {
            button.button.onClick.AddListener(() => MenuManager.instance.OpenMenu(button.menu));
        }
    }
}

[Serializable]
public class MenuButton
{
    public Button button;
    public MenuPanel menu;
}
