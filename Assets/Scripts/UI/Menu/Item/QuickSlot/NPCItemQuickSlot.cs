using System;
using System.Collections.Generic;
using UnityEngine;

public class NPCItemQuickSlot : ItemQuickSlot
{
    public List<Item> dropItemList;
    [SerializeField] private ItemInstance itemInstance;
    protected virtual void Update()
    {
        if (commander.GetInput(ConditionInput.Item1) && dropItemList.Count > 0 && enable)
        {
            dropItemList[0].OnUse(character);
            dropItemList.RemoveAt(0);
        }
    }

    public void SetItem(Item item)
    {
        dropItemList.Add(item);
    }

    public override void GetItem(Item item)
    {
        dropItemList.Add(item);
    }

    public override void OnDead()
    {
        base.OnDead();
        foreach (Item item in dropItemList)
        {
            var i = Instantiate(itemInstance, transform.position, Quaternion.identity);
            i.Init(item);
        }
        dropItemList.Clear();
    }
}
