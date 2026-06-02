using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

public class FogOfDarknessPass : ScriptableRenderPass
{
    private Material material;

    public FogOfDarknessPass(Material material)
    {
        this.material = material;
        renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (material == null) return;

        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        if (resourceData.isActiveTargetBackBuffer) return;

        TextureHandle src = resourceData.activeColorTexture;

        RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;

        TextureHandle dst = UniversalRenderer.CreateRenderGraphTexture(
            renderGraph, desc, "_FogOfDarknessTemp", false);

        var para = new RenderGraphUtils.BlitMaterialParameters(src, dst, material, 0);
        renderGraph.AddBlitPass(para, passName: "FogOfDarkness");

        renderGraph.AddCopyPass(dst, src, passName: "FogOfDarkness_CopyBack");
    }
}