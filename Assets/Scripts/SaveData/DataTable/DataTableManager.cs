using UnityEngine;

public static class DataTableManager
{
    public static StringTable StringTable; 
    public static SpriteTable SpriteTable;
    public static DialogTable DialogTable;

    static DataTableManager()
    {
        Init();
    }

    private static void Init()
    {
        StringTable = new StringTable();
        SpriteTable = new SpriteTable();
        DialogTable = new DialogTable();
        LoadAll();
    }

    private static void LoadAll()
    {
        StringTable.Load(LoadCSV("Tables/StringTable"));
        SpriteTable.Load(LoadCSV("Tables/SpriteTable"));
        DialogTable.Load(LoadCSV("Tables/DialogTable"));
    }

    private static string LoadCSV(string path)
    {
        TextAsset csv = Resources.Load<TextAsset>(path);
        if (csv == null)
        {
            Debug.LogError($"[DataTableManager] CSV not found: {path}");
            return string.Empty;
        }
        return csv.text;
    }
}
