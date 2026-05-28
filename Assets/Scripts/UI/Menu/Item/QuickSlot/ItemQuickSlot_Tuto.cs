using UnityEngine;

public class ItemQuickSlot_Tuto : PlayerItemQuickSlot
{
    protected override void Update()
    {
        if (commander.GetInput(ConditionInput.Item1) && itemTypeList.Count > 0 && enable)
        {
            var list = SaveManager.instance.CurrentSave.itemInCharacter;
            var selected = list.Find(entry => entry.itemName == itemTypeList[0]);
            if (selected != null)
            {
                var item = database.items.Find(itm => itm.itemName == itemTypeList[0]);
                item?.OnUse(character);
            }
        }
    }
}
