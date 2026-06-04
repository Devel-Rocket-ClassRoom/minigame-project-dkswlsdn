using UnityEngine;

public class CreateButton : ConditionButton
{
    [SerializeField] private Item item;
    [SerializeField] private ShowCreateDescription desc;
    [SerializeField] private RecipyShower recipy;
    [SerializeField] private Transform grid;

    protected override void Awake()
    {
        base.Awake();
        image.ChangeSprite(item.name);

        var list = DataTableManager.RecipeTable.Get(item.itemName).ingredients;

        foreach (var rcp in list)
        {
            var r = Instantiate(recipy, grid);
            r.Init(rcp.itemName, rcp.amount);
        }

        button.onClick.AddListener(SetDesc);
    }

    private void SetDesc()
    {
        desc.Init(item);
    }
}
