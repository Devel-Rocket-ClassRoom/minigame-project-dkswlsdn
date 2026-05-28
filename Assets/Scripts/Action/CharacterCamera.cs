using UnityEngine;

public abstract class CharacterCamera : MonoBehaviour
{
    protected SkillCaster caster;
    protected StateManager state;

    [SerializeField] protected bool canRotateCharacter;

    protected virtual void Awake()
    {
        state = GetComponent<StateManager>();
        caster = GetComponent<SkillCaster>();
    }

    private void OnEnable()
    {
        state.onIdle += ReturnOrigin;
        state.onWakeUp += ReturnOrigin;
        state.onHitstun += OnStun;
        state.onAirborne += OnStun;
        state.onKnockdown += OnStun;
        state.onGroggy += OnStun;
        state.onGrab += OnStun;
        state.onDead += OnStun;

        caster.onActionStart += OnUseSkill;
        caster.onSkillEnd += ReturnOrigin;
    }

    private void OnDisable()
    {
        state.onIdle -= ReturnOrigin;
        state.onWakeUp -= ReturnOrigin;
        state.onHitstun -= OnStun;
        state.onAirborne -= OnStun;
        state.onKnockdown -= OnStun;
        state.onGroggy -= OnStun;
        state.onGrab -= OnStun;
        state.onDead -= OnStun;

        caster.onActionStart += OnUseSkill;
        caster.onSkillEnd += ReturnOrigin;
    }

    public virtual void OnUseSkill(SkillAction action)
    {
        canRotateCharacter = true;
    }

    public virtual void ReturnOrigin()
    {
        canRotateCharacter = true;
    }

    public virtual void OnStun()
    {
        canRotateCharacter = false;
    }
}
