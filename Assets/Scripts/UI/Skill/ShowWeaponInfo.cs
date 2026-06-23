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
    private SkillCaster caster;
    private CharacterStat stat;

    // 화면 슬롯에 표시할 skills 인덱스 (L=기본공격 제외, 기존 순서에서 Q만 빠짐)
    private static readonly int[] slotSkillIndex =
    {
        (int)SkillKey.R,     // 1
        (int)SkillKey.SL,    // 2
        (int)SkillKey.LR,    // 3
        (int)SkillKey.E,     // 4
        (int)SkillKey.F,     // 5
        (int)SkillKey.Space, // 6
        (int)SkillKey.Q, // 7
    };

    private void Start()
    {
        Character.SubscribeToPlayer(Init);
    }


    public void Init(Character player)
    {
        // 이전 구독 정리 후 재구독 (플레이어 교체 대비)
        if (executer != null) executer.onWeaponChanged -= RefreshSlots;

        executer = player.Executer as PlayerSkillExecuter;
        caster = player.Caster;
        stat = player.Stat;

        // 무기/스킬(서브웨폰 포함)이 바뀔 때마다 슬롯 갱신
        if (executer != null) executer.onWeaponChanged += RefreshSlots;

        RefreshSlots();
    }

    private void OnDisable()
    {
        if (executer != null) executer.onWeaponChanged -= RefreshSlots;
    }

    private void OnDestroy()
    {
        Character.UnsubscribeFromPlayer(Init);
    }

    private void RefreshSlots()
    {
        if (executer == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            var skill = i < slotSkillIndex.Length ? executer.GetSkill(slotSkillIndex[i]) : null;
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
        if (caster == null || stat == null) return;

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
            slots[i].cooldownBar.fillAmount = i < slotSkillIndex.Length
                ? executer.GetCooldownRatio(slotSkillIndex[i])
                : 0f;
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
