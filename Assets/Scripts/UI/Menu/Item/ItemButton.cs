using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    public static string selected { get; private set; }
    private Item item;
    private ShowItemDescription desc;
    private Character character;
    private ItemSaveEntry entry;
    private bool isStorage;

    public void Init(Item item, ShowItemDescription desc, Character character, ItemSaveEntry entry, bool isStorage)
    {
        this.item = item;
        this.desc = desc;
        this.character = character;
        this.entry = entry;
        this.isStorage = isStorage;
        GetComponent<Button>().onClick.AddListener(SetDescription);
        GetComponent<ImageContainer>().ChangeSprite(item.itemName);
    }

    public void SetDescription()
    {
        desc.Init(item, character, entry, isStorage);
    }
}
