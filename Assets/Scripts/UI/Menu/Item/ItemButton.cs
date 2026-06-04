using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    public static string selected { get; private set; }
    private Item item;
    private ShowItemDescription desc;
    private Character character;
    private bool isStorage;
    [SerializeField] private TextMeshProUGUI itemAmount;

    public void Init(Item item, int amount, ShowItemDescription desc, Character character, bool isStorage)
    {
        this.item = item;
        this.desc = desc;
        this.character = character;
        this.isStorage = isStorage;
        GetComponent<Button>().onClick.AddListener(SetDescription);
        GetComponent<ImageContainer>().ChangeSprite(item.itemName);
        itemAmount.text = amount.ToString();
        
    }

    public void SetDescription()
    {
        desc.Init(item, character, isStorage);
    }
}
