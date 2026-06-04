using UnityEngine;
using UnityEngine.UI;

public class ConsumeButton : MonoBehaviour
{
    [SerializeField] private bool isInBattle;

    private Item item;
    private Character character;
    private ItemSaveEntry entry;
    private Button button;
    private bool isStorage;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void Init(Item item, Character character, ItemSaveEntry entry, bool isStorage)
    {
        if ((GameSceneManager.IsInBattle && item.canUseInBattle) || (!GameSceneManager.IsInBattle && item.canUseInBaseCamp))
        {
            gameObject.SetActive(true);

            this.item = item;
            this.character = character;
            this.entry = entry;
            this.isStorage = isStorage;

            button.onClick.AddListener(Consume);
        }
        else
        {
            gameObject.SetActive(false);
            return;
        }
    }

    private void OnDisable()
    {
        button.onClick.RemoveAllListeners();
    }

    private void Consume()
    {
        SaveManager.InventoryIO(item.itemName, -1, isStorage);
        item.OnUse(character);
        if (!GameSceneManager.IsInBattle) SaveManager.SaveRequest();
        button.onClick.RemoveAllListeners();
    }
}
