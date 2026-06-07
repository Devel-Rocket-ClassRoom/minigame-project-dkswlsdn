using UnityEngine;

[RequireComponent(typeof(CharacterStat))]
public class CharacterVisualStateHandler : MonoBehaviour
{
    private static readonly int OutlineColorID = Shader.PropertyToID("_OutlineColor");

    [Header("Outline")]
    [SerializeField] private Renderer[] outlineRenderers; // Outline 오브젝트의 Renderer들
    [SerializeField] private Color fullSuperArmorColor  = Color.white;
    [SerializeField] private Color longSuperArmorColor  = Color.green;
    [SerializeField] private Color shortSuperArmorColor = Color.blue;

    [Header("Immune - Transparency")]
    [SerializeField] private float immuneAlpha     = 0.4f;
    [SerializeField] private float immuneFadeSpeed = 8f;

    private CharacterStat stat;
    private Renderer[] renderers;
    private WallFadeTarget[] fadeTargets;
    private Material[] outlineMats;

    private ArmorType prevArmor  = (ArmorType)(-1);
    private bool      prevImmune = false;

    private void Awake()
    {
        stat = GetComponent<CharacterStat>();
        AcquireBodyRenderers();
        AcquireOutline(outlineRenderers); // 초기엔 인스펙터에 직렬화된 참조 사용
    }

    // 모델 교체(CharacterModelSwapper) 후 호출. 새 모델의 렌더러로 페이드/아웃라인 대상을 다시 잡는다.
    // 기존 캐시는 파괴되어 MissingReferenceException을 유발하므로 반드시 재취득해야 한다.
    public void Rebind()
    {
        // 이전 아웃라인 머티리얼 인스턴스 정리
        if (outlineMats != null)
            foreach (var m in outlineMats)
                if (m != null) Destroy(m);

        AcquireBodyRenderers();
        AcquireOutline(FindOutlineRenderers()); // 새 모델엔 직렬화 참조가 없으므로 이름으로 재탐색

        // 다음 Update에서 현재 상태를 새 렌더러에 강제로 다시 적용하도록 캐시 무효화
        prevArmor  = (ArmorType)(-1);
        prevImmune = false;
    }

    // 무적 페이드용 WallFadeTarget을 모든 자식 렌더러에 확보
    private void AcquireBodyRenderers()
    {
        renderers = GetComponentsInChildren<Renderer>(includeInactive: true);

        fadeTargets = new WallFadeTarget[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            var ft = renderers[i].GetComponent<WallFadeTarget>();
            if (ft == null) ft = renderers[i].gameObject.AddComponent<WallFadeTarget>();
            ft.Configure(immuneAlpha, immuneFadeSpeed);
            fadeTargets[i] = ft;
        }
    }

    // 아웃라인 머티리얼 인스턴스 생성 + 평소엔 끄기
    private void AcquireOutline(Renderer[] outlines)
    {
        outlineRenderers = outlines ?? new Renderer[0];
        outlineMats = new Material[outlineRenderers.Length];
        for (int i = 0; i < outlineRenderers.Length; i++)
        {
            if (outlineRenderers[i] == null) continue;
            outlineMats[i] = outlineRenderers[i].material;
            outlineRenderers[i].gameObject.SetActive(false);
        }
    }

    // 새 모델 계층에서 이름에 "outline"이 들어간 렌더러를 아웃라인으로 간주해 찾는다.
    // (모델 프리팹에 아웃라인 메시가 없으면 빈 배열 → 아웃라인 없이 정상 동작)
    private Renderer[] FindOutlineRenderers()
    {
        var list = new System.Collections.Generic.List<Renderer>();
        foreach (var r in renderers)
        {
            if (r == null) continue;
            if (r.gameObject.name.IndexOf("outline", System.StringComparison.OrdinalIgnoreCase) >= 0)
                list.Add(r);
        }
        return list.ToArray();
    }

    private void OnDestroy()
    {
        foreach (var m in outlineMats)
            if (m != null) Destroy(m);
    }

    private void Update()
    {
        ArmorType currentArmor  = stat.Armor;
        bool      currentImmune = stat.IsImmune;

        if (currentArmor == prevArmor && currentImmune == prevImmune) return;

        prevArmor  = currentArmor;
        prevImmune = currentImmune;

        ApplyOutline(currentArmor);
        ApplyImmuneFade(currentImmune);
    }

    private void ApplyOutline(ArmorType armor)
    {
        Color outlineColor = GetOutlineColor(armor);
        bool hasArmor = outlineColor != Color.clear;

        for (int i = 0; i < outlineRenderers.Length; i++)
        {
            if (outlineRenderers[i] == null) continue;
            outlineRenderers[i].gameObject.SetActive(hasArmor);
            if (hasArmor && outlineMats[i] != null)
                outlineMats[i].SetColor(OutlineColorID, outlineColor);
        }
    }

    private void ApplyImmuneFade(bool immune)
    {
        foreach (var ft in fadeTargets)
            if (ft != null) ft.SetHidden(immune);
    }

    private Color GetOutlineColor(ArmorType armor)
    {
        if ((armor & ArmorType.Full)  != 0) return fullSuperArmorColor;
        if ((armor & ArmorType.Long)  != 0) return longSuperArmorColor;
        if ((armor & ArmorType.Short) != 0) return shortSuperArmorColor;
        return Color.clear;
    }
}