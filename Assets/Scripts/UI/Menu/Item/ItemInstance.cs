using UnityEngine;

public class ItemInstance : Interactor
{
    [SerializeField] private Item item;

    public void Init(Item item)
    {
        this.item = item;
    }

    public override bool OnDetected(Character character)
    {
        character.QuickSlot.GetItem(item);
        Destroy(gameObject);
        return true;
    }
}
