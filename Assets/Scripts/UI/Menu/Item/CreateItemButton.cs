using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateItemButton : MonoBehaviour
{
    [SerializeField] private Character character;
    [SerializeField] private Image lockImage;
    private Button button;
    private Item item;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Create);
    }

    private void OnEnable()
    {
        SaveManager.onSaveModified += Load;
        Load();
    }

    private void OnDisable()
    {
        SaveManager.onSaveModified -= Load;
        item = null;
    }

    public void Load()
    {
        bool b = CheckRecipy();
        button.interactable = b;
        lockImage.gameObject.SetActive(!b);
    }

    public void Init(Item item)
    {
        this.item = item;
        Load();
    }

    private void Create()
    {
        if (item == null) return;

        var recipyList = DataTableManager.RecipeTable.Get(item.itemName).ingredients;
        var IIS = SaveManager.CurrentSave.itemInStorage;
        var IIC = SaveManager.CurrentSave.itemInCharacter;

        foreach (var recipy in recipyList)
        {
            IIC.TryGetValue(recipy.itemName, out int inCharacter);
            IIS.TryGetValue(recipy.itemName, out int inStorage);

            int remaining = recipy.amount;

            int fromCharacter = Mathf.Min(inCharacter, remaining);
            if (fromCharacter > 0) SaveManager.InventoryIO(recipy.itemName, -fromCharacter, false);
            remaining -= fromCharacter;

            int fromStorage = Mathf.Min(inStorage, remaining);
            if (fromStorage > 0) SaveManager.InventoryIO(recipy.itemName, -fromStorage, true);
        }

        if (item.isInstantUse)
        {
            item.OnUse(character);
        }
        else
        {
            SaveManager.InventoryIO(item.itemName, 1, true);
        }

        SaveManager.SaveRequest();
    }

    private bool CheckRecipy()
    {
        if (item == null) return false;

        var itemRecipy = DataTableManager.RecipeTable.Get(item.itemName);
        if (itemRecipy == null) return false;

        var recipyList = itemRecipy.ingredients;
        var IIS = SaveManager.CurrentSave.itemInStorage;
        var IIC = SaveManager.CurrentSave.itemInCharacter;
        List<ItemSaveEntry> entry = new();

        foreach (var recipy in recipyList)
        {
            if (IIS.ContainsKey(recipy.itemName) || IIC.ContainsKey(recipy.itemName))
            {
                IIS.TryGetValue(recipy.itemName, out int inStorage);
                IIC.TryGetValue(recipy.itemName, out int inCharacter);
                int amount = inStorage + inCharacter;

                if (amount < recipy.amount)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }
}
