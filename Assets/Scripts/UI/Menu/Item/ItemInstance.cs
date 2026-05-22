using UnityEngine;

public class ItemInstance : MonoBehaviour
{
    [SerializeField] private Item item;

    public Item GetItem()
    {
        return item;
    }
}
