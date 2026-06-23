using TMPro;
using UnityEngine;

public class CharacterStatusPanel : MenuPanel
{
    [SerializeField] private CharacterPanelController panel;
    [SerializeField] private CharacterGridController controller;
    [SerializeField] private StatusBar atk;
    [SerializeField] private StatusBar crit;
    [SerializeField] private StatusBar hp;
    [SerializeField] private StatusBar def;
    [SerializeField] private StatusBar dodgy;
    [SerializeField] private StatusBar speed;
    [SerializeField] private TextMeshProUGUI carry;
    private string currentId;

    private void OnEnable()
    {
        SaveManager.onSaveModified += Load;
    }

    private void OnDisable()
    {
        SaveManager.onSaveModified -= Load;
    }

    public void Init(string id)
    {
        currentId = id;
        Load();
        panel.Init(id);
        panel.Init(controller);
    }

    private void Load()
    {
        var dict = SaveManager.CurrentSave.characterData;

        CharacterData originalStat = null;
        originalStat = DataTableManager.CharacterTable.Get(currentId);

        CharacterEntry additinalStat = null;
        if (!dict.TryGetValue(currentId, out additinalStat))
        {
            throw new System.Exception("해당 캐릭터의 데이터 없음");
        }

        atk.Init(originalStat.attack, 50, additinalStat.consumedStat[(int)StatType.Attack]);
        crit.Init(originalStat.critical, 30, additinalStat.consumedStat[(int)StatType.Critical]);
        hp.Init(originalStat.health, 1000, additinalStat.consumedStat[(int)StatType.Health]);
        def.Init(originalStat.defense, 25, additinalStat.consumedStat[(int)StatType.Defense]);
        dodgy.Init(originalStat.dodgy, 30, additinalStat.consumedStat[(int)StatType.Dodgy]);
        speed.Init(8, 1, additinalStat.consumedStat[(int)StatType.Speed]);
        carry.text = originalStat.carry.ToString();
    }
}
