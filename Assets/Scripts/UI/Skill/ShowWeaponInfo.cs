using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShowWeaponInfo : MonoBehaviour
{
    [Serializable]
    public class SkillSlot
    {
        public TextContainer nameText;
        public Image cooldownBar;
    }

    public SkillSlot[] slots = new SkillSlot[7];

    public Image health;
    public Image cost;

    public Color hpFull;
    public Color hpZero;

    private PlayerSkillExecuter executer;
    [SerializeField] private SkillCaster caster;
    [SerializeField] private CharacterStat stat;

    private void Start()
    {
        executer = FindAnyObjectByType<PlayerSkillExecuter>();
        if (executer != null)
        {
            Init();
            executer.onWeaponChanged += Init;
        }
    }

    public void Init()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            var skill = executer.GetSkill(i + 1);
            if (slots[i].nameText != null)
            {
                var text = skill != null ? skill.skillId : string.Empty;
                slots[i].nameText.transform.parent.gameObject.SetActive(skill != null);
                slots[i].nameText.ChangeText(text);
            }
        }
    }

    private void Update()
    {
        SetCooldown();
        SetHPBar();
        SetCostBar();
    }

    private void SetCooldown()
    {
        if (executer == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].cooldownBar == null) continue;
            slots[i].cooldownBar.fillAmount = executer.GetCooldownRatio(i + 1);
        }
    }

    private void SetHPBar()
    {
        health.fillAmount = stat.HPRatio;
        health.color = Color.Lerp(hpZero, hpFull, stat.HPRatio);
    }

    private void SetCostBar()
    {
        float ratio = caster.CostRatio;
        if (ratio <= 0) cost.gameObject.SetActive(false);
        else
        {
            cost.gameObject.SetActive(true);
            cost.fillAmount = ratio;
        }
    }
}
