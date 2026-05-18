using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShowWeaponInfo : MonoBehaviour
{
    public static ShowWeaponInfo instance;

    [Serializable]
    public class SkillSlot
    {
        public TextMeshProUGUI nameText;
        public Image cooldownBar;
    }

    public SkillSlot[] slots = new SkillSlot[7];

    private SkillExecuter executer;

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else Destroy(gameObject);
    }

    private void Start()
    {
        var executer = FindAnyObjectByType<PlayerSkillExecuter>();
        if (executer != null) Init(executer);
    }

    public void Init(SkillExecuter executer)
    {
        this.executer = executer;

        for (int i = 0; i < slots.Length; i++)
        {
            var skill = executer.GetSkill(i + 1);
            if (slots[i].nameText != null)
                slots[i].nameText.text = skill != null ? skill.skillName : string.Empty;
        }
    }

    private void Update()
    {
        if (executer == null) return; 

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].cooldownBar == null) continue;
            slots[i].cooldownBar.fillAmount = executer.GetCooldownRatio(i + 1);
        }
    }
}
