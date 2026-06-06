using TMPro;
using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager instance;
    [SerializeField] private TextMeshProUGUI damagePrefab;
    [SerializeField] private TextMeshProUGUI critDamagePrefab;
    [SerializeField] private Canvas canvas;

    [Header("팝업 위치 분산 (head 기준 원 안에 랜덤 배치)")]
    [Tooltip("원 반지름 = 화면 높이 × 이 비율. 0이면 head 위치에 정확히 표시")]
    [Range(0f, 0.3f)]
    [SerializeField] private float radiusScreenRatio = 0.05f;
    [Tooltip("원 안쪽 빈 영역 비율(0~1). 0이면 중심 포함, 0.5면 바깥 절반 링에만 배치")]
    [Range(0f, 1f)]
    [SerializeField] private float innerHoleRatio = 0f;

    private Camera cam;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        cam = Camera.main;
    }

    public void Popup(float damage, bool crit, Vector3 head)
    {
        var g = crit ? Instantiate(critDamagePrefab, canvas.transform) : Instantiate(damagePrefab, canvas.transform);
        g.text = ((int)damage).ToString();

        var point = cam.WorldToScreenPoint(head);
        point.z = 0f;

        if (radiusScreenRatio > 0f)
        {
            float radius = Screen.height * radiusScreenRatio;
            Vector2 dir = Random.insideUnitCircle.normalized;
            float t = Mathf.Sqrt(Mathf.Lerp(innerHoleRatio * innerHoleRatio, 1f, Random.value));
            Vector2 offset = dir * (radius * t);
            point.x += offset.x;
            point.y += offset.y;
        }

        g.transform.position = point;

        g.GetComponent<DamageFadeout>().Play(crit);
    }
}
