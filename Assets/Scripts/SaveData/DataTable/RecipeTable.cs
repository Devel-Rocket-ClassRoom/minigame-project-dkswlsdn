// CSV 포맷 (헤더 1줄, #으로 시작하는 줄은 주석):
// id,(name,amount)
// ROPE, RADDER, 10
// ANCH, GOLD, 3, IRON, 3, WATER, 3
using System.Collections.Generic;

public class RecipeTable : DataTable<RecipeData> 
{
    public override void Load(string csv)
    {
        table.Clear();
        var lines = csv.Split('\n');

        bool headerSkipped = false;

        foreach (string line in lines)
        {
            string trimmed = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmed)) continue;
            if (trimmed.StartsWith("#")) continue;
            if (!headerSkipped) { headerSkipped = true; continue; }

            var cols = CsvUtil.SplitLine(trimmed);
            if (cols.Length < 3) continue;

            string id = cols[0].Trim();
            var ingredients = new List<RecipeIngredient>();

            for (int i = 1; i + 1 < cols.Length; i += 2)
            {
                string itemName = cols[i].Trim();
                if (!int.TryParse(cols[i + 1].Trim(), out int amount)) continue;
                ingredients.Add(new RecipeIngredient(itemName, amount));
            }

            table[id] = new RecipeData(ingredients);
        }
    }
}

public class RecipeData
{
    public List<RecipeIngredient> ingredients;

    public RecipeData(List<RecipeIngredient> ingredients)
    {
        this.ingredients = ingredients;
    }
}

public class RecipeIngredient
{
    public string itemName;
    public int amount;

    public RecipeIngredient(string itemName, int amount)
    {
        this.itemName = itemName;
        this.amount = amount;
    }
}
