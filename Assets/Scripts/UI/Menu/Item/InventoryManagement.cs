using UnityEngine;

public class InventoryManagement : MonoBehaviour
{
    public void MoveToStorage()
    {
        var data = SaveManager.CurrentSave;
        var storage = data.itemInStorage;
        var inventory = data.itemInCharacter;

        //for (int i = inventory.Count - 1; i >= 0; i--)
        //{
        //    storage.Add(inventory[i]);
        //    inventory.RemoveAt(i);
        //}

        SaveManager.SaveRequest();
    }
}
