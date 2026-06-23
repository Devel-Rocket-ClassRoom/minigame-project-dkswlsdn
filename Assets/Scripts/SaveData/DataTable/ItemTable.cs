// CSV 포맷 (헤더 1줄, #으로 시작하는 줄은 주석):
// id, name, description
// id = Item ScriptableObject의 itemName 값과 일치해야 함
// 체력 회복, 체력 회복, 체력을 100 회복한다
public class ItemTable : DataTable<ItemData>
{
    public override void Load(string csv)
    {
        table.Clear();
        var lines = csv.Split('\n');

        bool headerSkipped = false;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if (string.IsNullOrWhiteSpace(line)) continue;
            if (line.StartsWith("#")) continue;
            if (!headerSkipped) { headerSkipped = true; continue; }

            var cols = CsvUtil.SplitLine(line);
            if (cols.Length < 3) continue;

            string key = cols[0].Trim();
            string name = cols[1].Trim();
            string description = cols[2].Trim();

            table[key] = new ItemData(key, name, description);
        }
    }
}

public class ItemData
{
    public ItemData(string id, string name, string description)
    {
        this.id = id;
        this.name = name;
        this.description = description;
    }

    public string id;
    public string name;
    public string description;
}
