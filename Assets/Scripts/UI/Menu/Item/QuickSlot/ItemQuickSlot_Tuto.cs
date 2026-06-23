using UnityEngine;

public class ItemQuickSlot_Tuto : PlayerItemQuickSlot
{
    protected override void Update()
    {
        if (commander.GetInput(ConditionInput.Item1) && itemTypeList.Count > 0 && enable)
        {
            var dict = SaveManager.CurrentSave.itemInCharacter;
            if (dict.ContainsKey(itemTypeList[0]))
            {
                var item = DatabaseManager.FindItem(itemTypeList[0]);
                item?.OnUse(character);

                if (dict[itemTypeList[0]] <= 0)
                {
                    dict.Remove(itemTypeList[0]);
                }
            }
        }
    }
}
