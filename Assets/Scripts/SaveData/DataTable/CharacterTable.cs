using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class CharacterTable : DataTable<CharacterData>
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
            if (cols.Length < 6) continue;

            string key = cols[0].Trim();
            bool isParseFail = !(float.TryParse(cols[1], out float attack)
                & float.TryParse(cols[2], out float critical)
                & float.TryParse(cols[3], out float health)
                & float.TryParse(cols[4], out float defense)
                & float.TryParse(cols[5], out float dodgy)
                & int.TryParse(cols[6], out int carry));

            if (isParseFail)
                throw new System.InvalidOperationException("캐릭터 기본 데이터 파싱 실패");

            table[key] = new CharacterData(attack, critical, health, defense, dodgy, carry);
        }
    }
}

public class CharacterData
{
    public CharacterData(float a, float c, float h, float d, float dg, int ca)
    {
        attack = a;
        critical = c;
        health = h;
        defense = d;
        dodgy = dg;
        carry = ca;
    }

    public float attack;
    public float critical;
    public float health;
    public float defense;
    public float dodgy;
    public int carry;
} 