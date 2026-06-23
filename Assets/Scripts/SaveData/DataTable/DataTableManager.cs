using UnityEngine;

public static class DataTableManager
{
    public static StringTable StringTable;
    public static SpriteTable SpriteTable;
    public static DialogTable DialogTable;
    public static CharacterTable CharacterTable;
    public static SkillTable SkillTable;
    public static ItemTable ItemTable;
    public static RecipeTable RecipeTable; 

    static DataTableManager()
    {
        Init();
    }

    private static void Init()
    {
        StringTable = new StringTable();
        SpriteTable = new SpriteTable();
        DialogTable = new DialogTable();
        CharacterTable = new CharacterTable();
        SkillTable = new SkillTable();
        ItemTable = new ItemTable();
        RecipeTable = new RecipeTable();
        LoadAll();
    }

    private static void LoadAll()
    {
        StringTable.Load(LoadCSV("Tables/StringTable"));
        SpriteTable.Load(LoadCSV("Tables/SpriteTable"));
        DialogTable.Load(LoadCSV("Tables/DialogTable"));
        CharacterTable.Load(LoadCSV("Tables/CharacterTable"));
        SkillTable.Load(LoadCSV("Tables/SkillTable"));
        ItemTable.Load(LoadCSV("Tables/ItemTable"));
        RecipeTable.Load(LoadCSV("Tables/RecipyTable"));
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
