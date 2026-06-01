using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class StringTable : DataTable<string>
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

            var cols = line.Split(',');
            if (cols.Length < 2) continue;

            string key = cols[0].Trim();
            string value = cols[1].Trim();
            table[key] = value;
        }
    }
}