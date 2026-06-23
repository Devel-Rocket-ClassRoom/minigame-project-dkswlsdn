using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 벽 한 개의 페이드 상태를 관리한다. (CameraWallFader가 런타임에 자동으로 붙임)
///
/// 평소에는 원본 머티리얼(불투명)을 그대로 사용해 성능을 아끼고,
/// 가려질 때만 "양면 + 반투명" 복제본으로 교체한다.
/// 복제본에는 Cull Off(양면)를 강제로 켜기 때문에,
/// 카메라가 두꺼운 벽 안에 들어가도 안쪽 면이 그려져 "완전 투명"이 되지 않는다.
/// </summary>
[DisallowMultipleComponent]
public class WallFadeTarget : MonoBehaviour
{
    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorID = Shader.PropertyToID("_Color");
    private static readonly int SurfaceID = Shader.PropertyToID("_Surface");
    private static readonly int BlendID = Shader.PropertyToID("_Blend");
    private static readonly int SrcBlendID = Shader.PropertyToID("_SrcBlend");
    private static readonly int DstBlendID = Shader.PropertyToID("_DstBlend");
    private static readonly int ZWriteID = Shader.PropertyToID("_ZWrite");
    private static readonly int CullID = Shader.PropertyToID("_Cull");
    private static readonly int AlphaClipID = Shader.PropertyToID("_AlphaClip");

    private Renderer rend;
    private Material[] originalShared;  // 원본(불투명) 공유 머티리얼
    private Material[] fadeInstances;   // 양면+반투명 복제본 (지연 생성)

    private float currentAlpha = 1f;
    private float targetAlpha = 1f;
    private float fadedAlpha = 0.35f;
    private float fadeSpeed = 8f;
    private bool usingFadeMats;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend == null) rend = GetComponentInParent<Renderer>();
        if (rend != null) originalShared = rend.sharedMaterials;
        enabled = false;
    }

    public void Configure(float fadedAlpha, float fadeSpeed)
    {
        this.fadedAlpha = Mathf.Clamp01(fadedAlpha);
        this.fadeSpeed = Mathf.Max(0.01f, fadeSpeed);
    }

    public void SetHidden(bool hidden)
    {
        targetAlpha = hidden ? fadedAlpha : 1f;
        enabled = true; // 페이드 진행을 위해 Update 재개
    }

    private void Update()
    {
        if (rend == null) { enabled = false; return; }

        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

        bool shouldFade = currentAlpha < 0.999f;
        UseFadeMaterials(shouldFade);
        if (shouldFade) ApplyAlpha(currentAlpha);

        // 완전히 복구되면 원본으로 돌리고 Update 중단(유휴 비용 0)
        if (currentAlpha >= 0.999f && targetAlpha >= 0.999f)
            enabled = false;
    }

    private void UseFadeMaterials(bool use)
    {
        if (use == usingFadeMats || rend == null) return;

        if (use)
        {
            EnsureFadeInstances();
            rend.sharedMaterials = fadeInstances;
        }
        else
        {
            rend.sharedMaterials = originalShared;
        }
        usingFadeMats = use;
    }

    private void EnsureFadeInstances()
    {
        if (fadeInstances != null) return;

        fadeInstances = new Material[originalShared.Length];
        for (int i = 0; i < originalShared.Length; i++)
        {
            var src = originalShared[i];
            if (src == null) continue;
            var inst = new Material(src);
            MakeTransparentDoubleSided(inst);
            fadeInstances[i] = inst;
        }
    }

    private void ApplyAlpha(float a)
    {
        if (fadeInstances == null) return;
        foreach (var m in fadeInstances)
        {
            if (m == null) continue;
            if (m.HasProperty(BaseColorID))
            {
                var c = m.GetColor(BaseColorID); c.a = a; m.SetColor(BaseColorID, c);
            }
            if (m.HasProperty(ColorID))
            {
                var c = m.GetColor(ColorID); c.a = a; m.SetColor(ColorID, c);
            }
        }
    }

    /// <summary>URP Lit 머티리얼을 알파블렌딩 투명 + 양면(Cull Off)으로 전환.</summary>
    private static void MakeTransparentDoubleSided(Material m)
    {
        m.SetOverrideTag("RenderType", "Transparent");

        if (m.HasProperty(SurfaceID)) m.SetFloat(SurfaceID, 1f); // 1 = Transparent
        if (m.HasProperty(BlendID)) m.SetFloat(BlendID, 0f);     // 0 = Alpha
        if (m.HasProperty(SrcBlendID)) m.SetFloat(SrcBlendID, (float)BlendMode.SrcAlpha);
        if (m.HasProperty(DstBlendID)) m.SetFloat(DstBlendID, (float)BlendMode.OneMinusSrcAlpha);
        if (m.HasProperty(ZWriteID)) m.SetFloat(ZWriteID, 0f);
        if (m.HasProperty(AlphaClipID)) m.SetFloat(AlphaClipID, 0f);
        if (m.HasProperty(CullID)) m.SetFloat(CullID, (float)CullMode.Off); // 양면

        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.DisableKeyword("_ALPHATEST_ON");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");

        m.renderQueue = (int)RenderQueue.Transparent;
    }

    private void OnDestroy()
    {
        if (fadeInstances == null) return;
        foreach (var m in fadeInstances)
            if (m != null) Destroy(m);
    }
}
