using UnityEngine;

public abstract class CharacterCamera : MonoBehaviour
{
    protected SkillCaster caster;
    protected StateManager state;

    // 순수 런타임 플래그(OnStun/ReturnOrigin/OnUseSkill로만 제어). 직렬화하지 않아
    // 프리팹에서 실수로 false로 박혀 회전이 막히는 일을 방지한다.
    protected bool canRotateCharacter = true;

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
