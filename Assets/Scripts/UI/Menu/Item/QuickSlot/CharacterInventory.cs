using UnityEngine;

public class CharacterInventory : MonoBehaviour
{
    [SerializeField] private ItemDatabase database;
    private Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        AutoConsumeItems();
    }

    private void AutoConsumeItems()
    {
        var inventory = SaveManager.instance.CurrentSave.itemInCharacter;

        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            var entry = inventory[i];
            var item = database.items.Find(x => x.itemName == entry.itemName);

            if (item == null || item.canUseInBattle || item.canUseInBaseCamp) continue;

            item.OnUse(character);
            inventory.RemoveAt(i);
        }

        SaveManager.instance.SaveRequest();
    }
}
