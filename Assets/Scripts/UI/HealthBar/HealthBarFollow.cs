using UnityEngine;
using UnityEngine.UI;

// 적 머리 위에 따라다니는 체력바(Screen Space - Overlay 캔버스 자식).
// 위치 추적 + 체력 비율 표시만 담당한다. 생성/해제는 HealthBarManager가 시야와 연동해 처리.
public class HealthBarFollow : MonoBehaviour
{
    [SerializeField] private Image fill;                       // 체력 비율 이미지(Image Type = Filled)
    [SerializeField] private GameObject visual;                // 카메라 뒤일 때 끌 그래픽 루트(선택)
    [SerializeField] private Vector3 worldOffset = new(0f, 0.4f, 0f); // 머리 위 여유

    private Character target;
    private Transform head;
    private Camera cam;
    private RectTransform rect;

    private void Awake() => rect = (RectTransform)transform;

    public void Bind(Character character)
    {
        target = character;
        head = character.Anchor != null ? character.Anchor.head : character.transform;
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (target == null || head == null || cam == null) return;

        Vector3 sp = cam.WorldToScreenPoint(head.position + worldOffset);

        bool inFront = sp.z > 0f;                  // 카메라 뒤면 숨김
        if (visual != null) visual.SetActive(inFront);
        if (!inFront) return;

        rect.position = new Vector3(sp.x, sp.y, 0f);
        if (fill != null) fill.fillAmount = target.Stat.HPRatio;
    }
}
