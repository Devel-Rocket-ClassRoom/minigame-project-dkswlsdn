using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemQuickSlot : MonoBehaviour
{
    private Character character;
    private CharacterCommander commander;
    public List<string> itemTypeList;
    public List<Item> itemList;
    [SerializeField] private bool isPlayerTeam;

    private void Awake()
    {
        character = GetComponent<Character>();
        commander = GetComponent<CharacterCommander>();
    }

    private void OnEnable()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            itemTypeList.Add(itemList[i].itemName);
        }
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
        if (isPlayerTeam)
        {
            for (int i = 0; i < itemTypeList.Count; i++)
            {
                if (itemTypeList[i] == item.itemName && itemList[i] == null)
                {
                    itemList[i] = item;
                    return true;
                }
                else
                {
                    SaveManager.instance.CurrentSave.itemInCharacter.Add(new ItemSaveEntry(item.itemName, DateTime.Now));
                }
            }
        }
        else
        {
            itemList.Add(item);
        }

        return false;
    }

    public void OnDead()
    {

    }
}
