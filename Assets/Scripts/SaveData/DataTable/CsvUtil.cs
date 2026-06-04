using System.Collections.Generic;
using System.Text;

// CSV 한 줄을 RFC 4180 규칙으로 분리한다.
// - 큰따옴표로 감싼 필드 안의 쉼표는 구분자로 보지 않는다:  a,"b,c",d  →  [a] [b,c] [d]
// - 따옴표 안에서 "" 는 따옴표 한 개로 해석한다:            "그가 ""안녕"" 했다"  →  그가 "안녕" 했다
public static class CsvUtil
{
    public static string[] SplitLine(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; } // "" → "
                    else inQuotes = false;
                }
                else sb.Append(c);
            }
            else
            {
                if (c == '"') inQuotes = true;
                else if (c == ',') { result.Add(sb.ToString()); sb.Clear(); }
                else sb.Append(c);
            }
        }
        result.Add(sb.ToString());
        return result.ToArray();
    }
}
