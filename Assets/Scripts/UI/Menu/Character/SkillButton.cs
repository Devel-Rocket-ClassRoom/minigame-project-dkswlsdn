using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SkillButton : MonoBehaviour
{
    [SerializeField] private SkillKey skillKey;
    [SerializeField] private Button button;
    [SerializeField] private TextContainer label;

    private Skill currentSkill;

    public SkillKey SkillKey => skillKey;
    public int MagicIndex => (int)skillKey;
    public Button Button => button;
    public Skill CurrentSkill => currentSkill;

    private void Reset()
    {
        button = GetComponent<Button>();
    }

    public void Init(Action<SkillButton> onSelected)
    {
        if (button == null) button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onSelected?.Invoke(this));
    }

    // 마법 전환 상태에 맞춰 전달된 스킬로 버튼을 초기화
    public void SetSkill(Skill skill)
    {
        currentSkill = skill;
        if (label != null) label.ChangeText(skill != null ? skill.skillId : string.Empty);
    }
}
