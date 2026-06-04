using UnityEngine;

public class StartGameMenu : MonoBehaviour
{
    public int Slot { get; private set; }
    public MenuManager manager;
    public MenuPanel delete;

    public void SetSlot(int slot)
    {
        Slot = slot;
    }

    public void OnDeleteSave()
    {
        SaveManager.DeleteSave(Slot);
        manager.OpenPopup(delete);
    }

    public void OnStartGame(int slot)
    {
        GameSceneManager.StartGame(slot);
    }
}
