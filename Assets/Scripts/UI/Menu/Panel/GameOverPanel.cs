using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 게임오버 패널 연출.
// MenuManager.GameOver() → OpenMenu(gameOverPanel) 로 이 패널이 SetActive(true)되면 OnEnable에서 연출 시작.
//
// 연출 순서:
//  1) 배경 오버레이: 투명 → 검붉은색 으로 서서히 나타남(투명도 낮춤)
//  2) 배경 오버레이: 검붉은색 → 검은색 으로 색 전환
//  3) 게임오버 이미지 + 재시작/타이틀 버튼(content): 투명 → 불투명 으로 나타남
//
// 게임오버 시 Time.timeScale이 0이어도 동작하도록 unscaledTime을 사용한다.
public class GameOverPanel : MonoBehaviour
{
    [Header("배경 오버레이")]
    [SerializeField] private Image background;                       // 전체 화면 덮는 Image
    [SerializeField] private Color bloodColor = new(0.25f, 0f, 0f, 1f);  // 검붉은색
    [SerializeField] private Color endColor = Color.black;          // 최종 색(검은색)
    [SerializeField] private float fadeInDuration = 1.5f;           // 투명 → 검붉은색
    [SerializeField] private float colorShiftDuration = 1.0f;       // 검붉은색 → 검은색

    [Header("내용 (게임오버 이미지 + 버튼들)")]
    [SerializeField] private CanvasGroup content;                   // 이미지+버튼을 묶는 그룹
    [SerializeField] private float contentDelay = 0.2f;             // 검은색 전환 후 대기
    [SerializeField] private float contentFadeDuration = 1.0f;      // 내용 페이드 인

    [Header("버튼")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button titleButton;

    private Coroutine routine;

    private void Awake()
    {
        if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
        if (titleButton != null) titleButton.onClick.AddListener(OnTitle);
    }

    private void OnEnable()
    {
        // 시작 상태로 초기화(재활성화 시에도 처음부터 재생)
        if (background != null)
        {
            var c = bloodColor;
            c.a = 0f;
            background.color = c;
            background.raycastTarget = true;   // 뒤쪽 클릭 차단
        }
        if (content != null)
        {
            content.alpha = 0f;
            content.interactable = false;
            content.blocksRaycasts = false;
        }

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(CoPlay());
    }

    private void OnDisable()
    {
        if (routine != null) { StopCoroutine(routine); routine = null; }
    }

    private IEnumerator CoPlay()
    {
        int anch = SaveManager.CurrentSave.anchCount - SaveManager.CurrentSave.currentParty.Count;
        if (anch < 0) restartButton.interactable = false;

        // 1) 투명 → 검붉은색 (알파 0 → 1)
        yield return FadeBackgroundAlpha(bloodColor, 0f, 1f, fadeInDuration);

        // 2) 검붉은색 → 검은색 (색 보간, 알파 1 유지)
        yield return ShiftBackgroundColor(bloodColor, endColor, colorShiftDuration);

        // 3) 내용 페이드 인
        if (contentDelay > 0f) yield return new WaitForSecondsRealtime(contentDelay);
        yield return FadeContent(0f, 1f, contentFadeDuration);

        if (content != null)
        {
            content.interactable = true;
            content.blocksRaycasts = true;
        }

        routine = null;
    }

    // 배경 색은 rgb 고정, 알파만 from→to 로 보간
    private IEnumerator FadeBackgroundAlpha(Color rgb, float from, float to, float duration)
    {
        if (background == null) yield break;
        if (duration <= 0f) { SetBackground(rgb, to); yield break; }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            SetBackground(rgb, Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        SetBackground(rgb, to);
    }

    // 배경 색을 from→to 로 보간(알파는 1로 유지)
    private IEnumerator ShiftBackgroundColor(Color from, Color to, float duration)
    {
        if (background == null) yield break;
        from.a = 1f; to.a = 1f;
        if (duration <= 0f) { background.color = to; yield break; }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            background.color = Color.Lerp(from, to, t / duration);
            yield return null;
        }
        background.color = to;
    }

    private IEnumerator FadeContent(float from, float to, float duration)
    {
        if (content == null) yield break;
        if (duration <= 0f) { content.alpha = to; yield break; }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            content.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        content.alpha = to;
    }

    private void SetBackground(Color rgb, float alpha)
    {
        rgb.a = alpha;
        background.color = rgb;
    }

    private void OnRestart()
    {
        Time.timeScale = 1f;
        GameSceneManager.LoadScene(SceneName.BaseCamp);
    }

    private void OnTitle()
    {
        Time.timeScale = 1f;
        GameSceneManager.LoadScene(SceneName.TitleScene);
    }
}
