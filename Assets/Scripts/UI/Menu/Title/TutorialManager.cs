using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup wasdGroup;
    [SerializeField] private CanvasGroup interactionGroup;
    [SerializeField] private CanvasGroup comboGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    [SerializeField] private CharacterCommander commander;
    [SerializeField] private TutorialInteractionManager interaction;
    private bool isCharacterReady;
    private bool wasdDone;
    private bool interactionDone;
    private bool interactionActive;
    private bool comboActive;

    private void Start()
    {
        interactionGroup.alpha = 0f;
        interactionGroup.gameObject.SetActive(false);

        comboGroup.alpha = 0f;
        comboGroup.gameObject.SetActive(false);

        StartCoroutine(FadeIn(wasdGroup));
        StartCoroutine(Wait(3f));
    }

    private void Update()
    {
        if (commander == null) return;

        if (isCharacterReady && !wasdDone && commander.MoveInput.magnitude > 0.1f)
        {
            wasdDone = true;
            StartCoroutine(FadeOut(wasdGroup));
        }

        if (!interactionDone && interaction.isInteracted)
        {
            interactionDone = true;
            StartCoroutine(FadeOut(interactionGroup));
        }

        if (comboActive && commander.GetInput(ConditionInput.SkillL))
        {
            comboActive = false;
            StartCoroutine(FadeOut(comboGroup));
        }
    }

    public void ShowInteractionTutorial()
    {
        if (interactionActive) return;
        interactionActive = true;
        StartCoroutine(FadeIn(interactionGroup));
    }

    public void ShowComboTutorial()
    {
        if (comboActive) return;
        comboActive = true;
        commander.
        StartCoroutine(FadeIn(comboGroup));
    }

    private IEnumerator FadeIn(CanvasGroup group)
    {
        group.gameObject.SetActive(true);
        float elapsed = 0f;
        float start = group.alpha;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, 1f, elapsed / fadeDuration);
            yield return null;
        }
        group.alpha = 1f;
    }

    private IEnumerator FadeOut(CanvasGroup group)
    {
        float elapsed = 0f;
        float start = group.alpha;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, 0f, elapsed / fadeDuration);
            yield return null;
        }
        group.alpha = 0f;
        group.gameObject.SetActive(false);
    }

    IEnumerator Wait(float duration)
    {
        yield return new WaitForSeconds(duration);
        isCharacterReady = true;
    }
}
