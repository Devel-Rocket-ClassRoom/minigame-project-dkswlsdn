using UnityEngine;

public class OpenMain : MonoBehaviour
{
    MenuManager manager;

    private void Awake()
    {
        manager = GetComponent<MenuManager>();
        manager.TitleOpen();
    }
}
