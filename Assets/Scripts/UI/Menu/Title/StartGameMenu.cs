using UnityEngine;

public class StartGameMenu : MonoBehaviour
{
    public int Slot { get; private set; }
    public MenuPanel delete;

    public void SetSlot(int slot)
    {
        Slot = slot;
    }

    public void OnDeleteSave()
    {
        SaveManager.DeleteSave(Slot);
        MenuManager.instance.OpenPopup(delete);
        MenuManager.instance.ClosePopup();
    }

    public void OnStartGame(int slot)
    {
        if (!GameSceneManager.StartGame(slot))
        {
            Debug.Log("부활할 수 없습니다");
        }
    }
}
