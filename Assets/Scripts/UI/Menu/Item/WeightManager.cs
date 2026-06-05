using UnityEngine;

public class WeightManager
{
    public static int Max
    {
        get
        {
            int max = 0;
            var party = SaveManager.CurrentSave.currentParty;
            foreach (var p in party)
            {
                max += DataTableManager.CharacterTable.Get(p).carry;
            }
            return max;
        }
    }

    public static float Sum
    {
        get
        {
            float sum = 0;
            var items = SaveManager.CurrentSave.itemInCharacter;
            foreach (var i in items)
            {
                var item = DatabaseManager.FindItem(i.Key);
                if (item == null)
                {
                    Debug.LogWarning($"[WeightManager] 아이템을 찾을 수 없음: {i.Key}");
                    continue;
                }
                sum += item.weight;
            }
            return sum;
        }
    }
}
