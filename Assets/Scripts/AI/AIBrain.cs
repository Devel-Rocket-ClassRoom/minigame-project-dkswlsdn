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
    private Coroutine chaseCoroutine;
    [SerializeField] private float maxChaseTime = 3f;

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
        SetAgrro();
        RotateCommand();
        MovementCommand();
        SkillCommand();
    }

    private void SetAgrro()
    {
        if (aggro == null && sight.visibleCharacters.Count > 0)
        {
            aggro = sight.FirstEncounter;
        }
        else if (sight.visibleCharacters.Count == 0 && chaseCoroutine == null)
        {
            chaseCoroutine = StartCoroutine(CoChase(maxChaseTime));
        }
    }

    private void RotateCommand()
    {
        if (aggro == null) return;

        cam.RotateTo(aggro.transform);
    }

    private void MovementCommand()
    {
        if (aggro == null) { command.SetMoveInput(new Vector2(0, 0)); return; }

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
        if (currentCombo != null) return;

        var w = executer.CurrentWeapon;
        bool canUse = true;
        foreach (var c in w.combo)
        {
            foreach (var condition in c.conditions)
            {
                if (!condition.IsMet(character, aggro))
                {
                    canUse = false;
                    break;
                }
            }

            if (canUse)
            {
                currentCombo = StartCoroutine(CoCombo(c));
                break;
            }
        }
    }

    IEnumerator CoCombo(Combo combo)
    {
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

            if (c.condtion.IsMet(character, aggro))
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

    IEnumerator CoChase(float duration)
    {
        yield return new WaitForSeconds(duration);
        aggro = null;
        chaseCoroutine = null;
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
