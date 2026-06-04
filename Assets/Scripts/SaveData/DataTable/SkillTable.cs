using UnityEngine;

// CSV 포맷 (헤더 1줄, #으로 시작하는 줄은 주석):
// key, description, opinion
// AXE_Passive, 공격력이 증가한다, 도끼는 역시 무겁군
//
// key = "{캐릭터ID}_{SkillKey}" 형태 (예: AXE_Passive, MAGIC_E)
public class SkillTable : DataTable<SkillData>
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
            if (cols.Length < 3) continue;

            string key = cols[0].Trim();
            string description = cols[1].Trim();
            string opinion = cols[2].Trim();

            table[key] = new SkillData(description, opinion);
        }
    }

    public SkillData Get(string characterId, SkillKey key)
    {
        return Get(BuildKey(characterId, key, false));
    }

    // 마법 전환 상태면 "{캐릭터ID}_{스킬키}_M" 키로 조회
    public SkillData Get(string characterId, SkillKey key, bool isMagic)
    {
        return Get(BuildKey(characterId, key, isMagic));
    }

    public bool TryGet(string characterId, SkillKey key, bool isMagic, out SkillData value)
    {
        return TryGet(BuildKey(characterId, key, isMagic), out value);
    }

    private static string BuildKey(string characterId, SkillKey key, bool isMagic)
    {
        return isMagic ? $"{characterId}_{key}_M" : $"{characterId}_{key}";
    }
}

public class SkillData
{
    public SkillData(string description, string characterOpinion)
    {
        this.description = description;
        this.characterOpinion = characterOpinion;
    }

    public string description;
    public string characterOpinion;
}
