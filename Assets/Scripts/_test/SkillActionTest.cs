using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class SkillActionTest : MonoBehaviour
{
    private SkillCaster caster;
    private PlayerInputAction action;
    public Skill skill1;
    public Skill skill2;
    public Skill skill3;
    public Skill skill4;
    public Skill skill5;

    private void Awake()
    {
        caster = GetComponent<SkillCaster>();
    }
    private void OnEnable()
    {
        action = PlayerMovement.Action;
        PlayerMovement.action.Player.SkillL.performed += LSkill;
    }

    public void LSkill(CallbackContext context)
    {
        caster.Cast(skill1);
    }
}
