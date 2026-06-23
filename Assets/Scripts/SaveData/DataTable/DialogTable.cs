using UnityEngine;

public class DialogTable : DataTable<DialogData>
{
    public override void Load(string csv)
    {
        table.Clear();
        var lines = csv.Split('\n');

        bool headerSkipped = false;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            if (string.IsNullOrWhiteSpace(line)) continue; // 빈 줄 스킵
            if (line.StartsWith("#")) continue;            // 주석 스킵
            if (!headerSkipped) { headerSkipped = true; continue; } // 헤더 스킵

            var cols = CsvUtil.SplitLine(line);
            if (cols.Length < 2) continue;

            string key = cols[0].Trim();
            string characterId = cols[1].Trim();
            string characterName = cols[2].Trim();
            string dialog = cols[3].Trim();
            table[key] = new DialogData(characterId, characterName, dialog);
        }
    }
}

public class DialogData
{
    public DialogData(string characterId, string characterName, string dialog)
    {
        this.characterId = characterId;
        this.characterName = characterName;
        this.dialog = dialog;
    }

    public string characterId;
    public string characterName;
    public string dialog;
}