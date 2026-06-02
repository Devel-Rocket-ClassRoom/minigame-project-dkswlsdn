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
        var inventory = isStorage ? SaveManager.instance.CurrentSave.itemInStorage : SaveManager.instance.CurrentSave.itemInCharacter;
        inventory.Remove(entry);
        item.OnUse(character);
        if (!GameSceneManager.IsInBattle) SaveManager.instance.SaveRequest();
        button.onClick.RemoveAllListeners();
    }
}
