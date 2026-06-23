using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 하나의 ImageContainer를 가지고, 클릭할 때마다 두 키(keyA/keyB)의
/// 스프라이트를 번갈아 표시하는 토글 버튼.
/// Image를 끄지 않고 스프라이트만 교체하므로 레이캐스트 타깃이 항상 유지된다.
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(ImageContainer))]
public class ToggleButton : MonoBehaviour
{
    [SerializeField] private string keyA;
    [SerializeField] private string keyB;
    [SerializeField] private bool startOnA = true;

    private bool isA;

    /// <summary>현재 A가 켜져 있으면 true</summary>
    public bool IsA => isA;

    private Button button;
    /// <summary>이 토글의 Button (읽기 전용)</summary>
    public Button Button => button != null ? button : (button = GetComponent<Button>());

    private ImageContainer container;
    private ImageContainer Container => container != null ? container : (container = GetComponent<ImageContainer>());

    private void Awake()
    {
        isA = startOnA;
        // 자동 토글하지 않는다. 표시 상태는 외부(SetState)가 세이브 기준으로 제어한다.
        // (클릭 자체는 CharacterSubWeaponGrid가 Button.onClick으로 따로 처리)
    }

    private void OnEnable()
    {
        // startOnA로 리셋하지 않는다. 외부(SetState)가 정한 상태를 유지하고 표시만 갱신.
        Apply();
    }

    public void Toggle()
    {
        isA = !isA;
        Apply();
    }

    /// <summary>외부에서 상태를 직접 지정 (토글 없이)</summary>
    public void SetState(bool a)
    {
        isA = a;
        Apply();
    }

    private void Apply()
    {
        Container.ChangeSprite(isA ? keyA : keyB);
    }
}
