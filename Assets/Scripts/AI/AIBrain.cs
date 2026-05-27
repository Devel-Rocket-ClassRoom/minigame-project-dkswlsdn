using System.Collections;
using UnityEngine;

public class AIBrain : MonoBehaviour
{
    private Character character;
    private NPCCommander command;
    private SkillExecuter executer;
    private CharacterStat stat;
    private SkillCaster caster;
    private NPCMovement movement;
    private StateManager state;
    private NPCCamera cam;
    private SightManager sight;

    private Coroutine currentCombo;

    [SerializeField] private float safeDistance;

    private Character aggro;

    private void Awake()
    {
        character = GetComponent<Character>();
        command = GetComponent<NPCCommander>();
        executer = GetComponent<SkillExecuter>();
        stat = GetComponent<CharacterStat>();
        caster = GetComponent<SkillCaster>();
        state = GetComponent<StateManager>();
        cam = GetComponent<NPCCamera>();
        sight = GetComponentInChildren<SightManager>();

        stat.onDamageTake += ChangeAggro;

        state.onAirborne += OnCancled;
        state.onDead += OnCancled;
        state.onHitstun += OnCancled;
        state.onGrab += OnCancled;
        state.onGroggy += OnCancled;
        state.onKnockdown += OnCancled;
    }

    private void Update()
    {
        RotateCommand();
        MovementCommand();
        SkillCommand();
    }

    private void RotateCommand()
    {
        if (aggro == null) return;

        cam.RotateTo(aggro.transform);
    }

    private void MovementCommand()
    {
        if (aggro == null) return;

        var t = transform.position;
        t.y = 0;
        var a = aggro.transform.position;
        a.y = 0;
        var sqrDist = (t - a).sqrMagnitude;

        if (sqrDist < (safeDistance - 0.5f) * (safeDistance - 0.5f))
        {
            command.SetMoveInput(new Vector2(0, -1));
        }
        else if (sqrDist > (safeDistance + 0.5f) * (safeDistance + 0.5f))
        {
            command.SetMoveInput(new Vector2(0, 1));
        }
        else
        {
            command.SetMoveInput(new Vector2(0, 0));
        }
    }

    private void SkillCommand()
    {
        var w = executer.CurrentWeapon;
        foreach (var c in w.combo)
        {
            if (c.condition.IsMet(character, caster.Context) && currentCombo == null)
            {
                currentCombo = StartCoroutine(CoCombo(c));
            }
        }
    }

    IEnumerator CoCombo(Combo combo)
    {
        // 첫 번째 press 인풋의 스킬 쿨타임이 끝날 때까지 대기
        foreach (var c in combo.comboInput)
        {
            if (!c.isPress) continue;
            while (!executer.IsSkillReady(c.input))
                yield return null;
            break;
        }

        foreach (var c in combo.comboInput)
        {
            if (c.isPress)
            {
                command.PressInput(c.input);
            }
            else
            {
                command.ReleaseInput(c.input);
            }

            if (c.condtion.IsMet(character, caster.Context))
            {
                yield return new WaitForSecondsUnfrozen(c.preDelay, state);
            }
            else
            {
                currentCombo = null;
                yield break;
            }
        }

        currentCombo = null;
    }

    private void ChangeAggro(Character character, AttackInfo info)
    {
        aggro = character;
    }

    private void OnCancled()
    {
        if (currentCombo != null)
        {
            StopCoroutine(currentCombo);
            currentCombo = null;
        }
    }
}
