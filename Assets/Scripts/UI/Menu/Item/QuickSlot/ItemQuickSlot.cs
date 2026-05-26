using UnityEngine;

public class ItemQuickSlot : MonoBehaviour
{
    private Character character;
    private CharacterCommander commander;
    public Item[] itemTypeList;
    public Item[] itemList;

    private void Awake()
    {
        character = GetComponent<Character>();
        commander = GetComponent<CharacterCommander>();
    }

    private void Update()
    {
        if (commander.GetInput(ConditionInput.Item1) && itemList[0] != null)
        {
            itemList[0].OnUse(character);
            itemList[0] = null;
        }
    }

    public bool GetIntoQuickSlot(Item item)
    {
        for (int i = 0; i < itemTypeList.Length; i++)
        {
            if (itemTypeList[i].itemName == item.itemName && itemTypeList[i] == null)
            {
                itemList[i] = item;
                return true;
            }
        }

        return false;
    }
}
