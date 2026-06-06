using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class DamageFadeout : MonoBehaviour
{
    [Header("공통 - 상승")]
    [Tooltip("위로 올라가는 거리(기준 해상도 픽셀, 캔버스 scaleFactor로 보정됨)")]
    [SerializeField] private float riseDistance = 60f;

    [Header("일반 공격")]
    [Tooltip("올라가기 전 제자리 대기 시간")]
    [SerializeField] private float normalHold = 0.3f;
    [Tooltip("위로 올라가며 사라지는 시간")]
    [SerializeField] private float normalRise = 0.7f;

    [Header("크리티컬 - 등장(스케일/페이드인)")]
    [SerializeField] private float critStartScale = 3f;
    [SerializeField] private float critEndScale = 1.3f;
    [SerializeField] private float critScaleDuration = 0.2f;

    [Header("크리티컬 - 흔들림/점멸")]
    [SerializeField] private float critShakeDuration = 0.3f;
    [Tooltip("흔들림 진폭(기준 해상도 픽셀, 캔버스 scaleFactor로 보정됨)")]
    [SerializeField] private float critShakeAmount = 8f;
    [Tooltip("점멸 1단계 길이(초). 이 간격마다 붉은색↔기본색 토글")]
    [SerializeField] private float critBlinkInterval = 0.05f;
    [SerializeField] private Color critBlinkColor = Color.red;

    [Header("크리티컬 - 상승")]
    [SerializeField] private float critRiseDuration = 0.5f;

    private TextMeshProUGUI text;
    private Vector3 basePosition;
    private Vector3 baseScale;
    private Color baseColor;
    private float canvasScale = 1f; // Scale With Screen Size 보정용

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    // 매니저가 위치를 잡은 뒤 호출한다.
    public void Play(bool crit)
    {
        if (text == null) text = GetComponent<TextMeshProUGUI>();
        basePosition = transform.position;
        baseScale = transform.localScale;
        baseColor = text.color;

        var canvas = GetComponentInParent<Canvas>();
        canvasScale = canvas != null ? canvas.scaleFactor : 1f;

        StopAllCoroutines();
        StartCoroutine(crit ? CoCrit() : CoNormal());
    }

    // 일반: 0.3초 제자리 → 0.7초 동안 위로 올라가며 페이드아웃
    private IEnumerator CoNormal()
    {
        transform.localScale = baseScale;
        SetAlpha(1f);

        yield return new WaitForSeconds(normalHold);
        yield return Rise(normalRise);

        Destroy(gameObject);
    }

    // 크리티컬: (스케일 3→1.3 + 페이드인) → (흔들림 + 붉은 점멸) → (위로 상승 + 페이드아웃)
    private IEnumerator CoCrit()
    {
        // 1) 0.2초: 스케일 3배 → 1.3배, 투명도 0 → 1
        float t = 0f;
        while (t < critScaleDuration)
        {
            t += Time.deltaTime;
            float r = Mathf.Clamp01(t / critScaleDuration);
            transform.localScale = baseScale * Mathf.Lerp(critStartScale, critEndScale, r);
            SetAlpha(r);
            yield return null;
        }
        transform.localScale = baseScale * critEndScale;
        SetAlpha(1f);

        // 2) 0.3초: 약간 흔들리며 붉은색으로 점멸
        t = 0f;
        while (t < critShakeDuration)
        {
            t += Time.deltaTime;

            Vector2 shake = Random.insideUnitCircle * (critShakeAmount * canvasScale);
            transform.position = basePosition + (Vector3)shake;

            bool red = Mathf.FloorToInt(t / critBlinkInterval) % 2 == 0;
            Color c = red ? critBlinkColor : baseColor;
            c.a = 1f;
            text.color = c;

            yield return null;
        }
        transform.position = basePosition;
        SetColorKeepAlpha(baseColor, 1f);

        // 3) 0.5초: 일반처럼 위로 올라가며 페이드아웃
        yield return Rise(critRiseDuration);

        Destroy(gameObject);
    }

    // 현재 위치에서 riseDistance(scaleFactor 보정) 만큼 위로 올라가며 투명도 1 → 0
    private IEnumerator Rise(float duration)
    {
        Vector3 start = transform.position;
        Vector3 end = start + Vector3.up * (riseDistance * canvasScale);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float r = Mathf.Clamp01(t / duration);
            transform.position = Vector3.Lerp(start, end, r);
            SetAlpha(1f - r);
            yield return null;
        }
    }

    private void SetAlpha(float a)
    {
        var c = text.color;
        c.a = a;
        text.color = c;
    }

    private void SetColorKeepAlpha(Color color, float a)
    {
        color.a = a;
        text.color = color;
    }
}
