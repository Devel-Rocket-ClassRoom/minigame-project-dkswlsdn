using UnityEngine;

public class ItemInstance : Interactor
{
    [SerializeField] private Item item;

    public override bool OnDetected(Character character)
    {
        character.QuickSlot.GetIntoQuickSlot(item);
        Destroy(gameObject);
        return true;
    }
}
