using UnityEngine;

public class WeightManager
{
    private static ItemDatabase _database;
    private static ItemDatabase Database
    {
        get
        {
            if (_database == null)
                _database = Resources.Load<ItemDatabase>("ItemDatabase");
            return _database;
        }
    }

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
            if (Database == null)
            {
                Debug.LogWarning("[WeightManager] Database가 null입니다. ShowWeight에서 할당했는지 확인하세요.");
                return 0;
            }

            float sum = 0;
            var items = SaveManager.CurrentSave.itemInCharacter;
            foreach (var i in items)
            {
                var item = Database.items.Find(x => x.itemName == i.Key);
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
