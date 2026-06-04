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
        stat      = GetComponent<CharacterStat>();
        renderers = GetComponentsInChildren<Renderer>(includeInactive: true);

        // 무적 페이드용 WallFadeTarget
        fadeTargets = new WallFadeTarget[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            var ft = renderers[i].GetComponent<WallFadeTarget>();
            if (ft == null) ft = renderers[i].gameObject.AddComponent<WallFadeTarget>();
            ft.Configure(immuneAlpha, immuneFadeSpeed);
            fadeTargets[i] = ft;
        }

        // 아웃라인 머티리얼 인스턴스
        outlineMats = new Material[outlineRenderers.Length];
        for (int i = 0; i < outlineRenderers.Length; i++)
        {
            if (outlineRenderers[i] == null) continue;
            outlineMats[i] = outlineRenderers[i].material;
            outlineRenderers[i].gameObject.SetActive(false);
        }
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
            ft.SetHidden(immune);
    }

    private Color GetOutlineColor(ArmorType armor)
    {
        if ((armor & ArmorType.Full)  != 0) return fullSuperArmorColor;
        if ((armor & ArmorType.Long)  != 0) return longSuperArmorColor;
        if ((armor & ArmorType.Short) != 0) return shortSuperArmorColor;
        return Color.clear;
    }
}