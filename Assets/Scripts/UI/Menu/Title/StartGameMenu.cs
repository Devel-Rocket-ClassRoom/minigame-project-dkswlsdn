using UnityEngine;

public class StartGameMenu : MonoBehaviour
{
    public int Slot { get; private set; }

    public void SetSlot(int slot)
    {
        Slot = slot;
    }

    public void OnDeleteSave()
    {
        SaveManager.instance.DeleteSave(Slot);
    }
}
