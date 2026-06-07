using UnityEngine;

// 캐릭터 교체: Character 루트는 그대로 두고 Model 자식(메시 + Animator + 부착 무기메시)만 갈아끼운다.
// 카메라 Follow 타겟 / 입력 / Character.CurrentPlayer / 적 AI 어그로 등 "루트"를 붙잡는 참조는
// 전부 유효하게 유지되므로 "캐릭터가 잠깐 사라지는" 문제가 없다.
//
// 스킬/마법 세팅은 PlayerSkillExecuter가 SaveManager.onSaveModified로 이미 자동 처리한다.
// 여기서는 비주얼(모델 + 손에 부착된 무기 메시)과, 교체로 끊기는 Animator 참조 재배선만 책임진다.
[RequireComponent(typeof(StateManager))]
public class CharacterModelSwapper : MonoBehaviour
{
    [Tooltip("새 모델이 들어갈 부모. 보통 \"Model\" 오브젝트. 비우면 자기 자신.")]
    [SerializeField] private Transform modelContainer;

    private StateManager state;
    private SkillAnimationHandler animationHandler;
    private CharacterAnchor anchor;
    private CharacterVisualStateHandler visualState;

    private string loadedCharacterId;

    private void Awake()
    {
        state = GetComponent<StateManager>();
        animationHandler = GetComponent<SkillAnimationHandler>();
        anchor = GetComponent<CharacterAnchor>();
        visualState = GetComponent<CharacterVisualStateHandler>();
        if (modelContainer == null) modelContainer = transform;
    }

    // 씬 진입 시 저장된 선택과 모델을 일치시킨다(스킬은 PlayerSkillExecuter가 이미 맞추므로 모델도 동기화).
    private void Start() => Swap();

    // 캐릭터 패널 창을 닫을 때 호출(MenuManager.CloseMenu).
    // 저장된 currentCharacterId가 직전에 적용한 것과 다를 때만 실제로 교체한다(중복 호출 무해).
    // 플레이어: 저장된 현재 출전 캐릭터로 동기화
    public void Swap()
    {
        SwapTo(SaveManager.CurrentSave.currentCharacterId);
        SaveManager.SaveRequest();
    }

    // 지정 캐릭터로 모델/무기 비주얼 교체. 플레이어/아군(AllySpawner) 공용.
    public void SwapTo(string characterId)
    {
        if (string.IsNullOrEmpty(characterId) || characterId == loadedCharacterId) return;

        // characterId -> weaponName 매핑은 PlayerSkillExecuter와 동일한 StringTable 규칙을 따른다.
        // 테이블 키는 캐릭터 ID 그대로다(예: "BAREHAND" -> "BareHand"). 접미사 없이 조회한다.
        var weaponName = DataTableManager.StringTable.Get(characterId);
        if (string.IsNullOrEmpty(weaponName)) return;

        var prefab = Resources.Load<GameObject>($"Model/{weaponName}");
        if (prefab == null)
        {
            Debug.LogWarning($"[CharacterModelSwapper] 모델을 찾지 못함: Resources/Model/{weaponName}");
            return;
        }

        loadedCharacterId = characterId;

        // 기존 모델 제거. Destroy는 프레임 끝에 처리되므로, 즉시 계층에서 분리(detach)해
        // 아래 재배선의 GetComponentsInChildren에 "파괴 예정인 옛 렌더러"가 섞여 잡히지 않게 한다.
        for (int i = modelContainer.childCount - 1; i >= 0; i--)
        {
            var old = modelContainer.GetChild(i);
            old.SetParent(null);
            Destroy(old.gameObject);
        }

        // 새 모델 생성 + 로컬 트랜스폼 정렬
        var instance = Instantiate(prefab, modelContainer);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        // 교체로 끊긴 Model 하위 참조 재배선 (콜라이더/스탯 등은 루트라 그대로 유효).
        var animator = instance.GetComponentInChildren<Animator>();
        state.SetAnimator(animator);
        animationHandler?.Rebind(animator);
        anchor?.Rebind(instance.transform);
        visualState?.Rebind(); // 새 모델의 렌더러로 아웃라인/무적 페이드 대상 재취득

        // 새 Animator 기준으로 Idle 포즈 복구
        state.ResetState();
    }
}
