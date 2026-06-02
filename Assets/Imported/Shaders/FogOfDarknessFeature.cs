using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FogOfDarknessFeature : ScriptableRendererFeature
{
    [SerializeField] private Shader shader;
    [SerializeField] private float darknessFactor = 0.4f;

    private Material material;
    private FogOfDarknessPass pass;

    public override void Create()
    {
        if (shader == null) return;
        material = new Material(shader);
        material.SetFloat("_DarknessFactor", darknessFactor);
        pass = new FogOfDarknessPass(material);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (pass == null) return;
        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing)
    {
        if (material != null)
            CoreUtils.Destroy(material);
    }
}