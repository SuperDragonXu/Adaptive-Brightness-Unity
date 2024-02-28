using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GrabColorRF : ScriptableRendererFeature
{
   
    GrabColorPass m_ScriptablePass;
    public RenderPassEvent InjectionPoint = RenderPassEvent.AfterRenderingOpaques;
    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new GrabColorPass();

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = InjectionPoint;
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        m_ScriptablePass.SetUp(renderer.cameraColorTargetHandle);
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!shouldExecute(renderingData))
            return;
        renderer.EnqueuePass(m_ScriptablePass);
    }
    bool shouldExecute(in RenderingData data)
    {
        if (data.cameraData.cameraType != CameraType.Game)
            return false;
        return true;
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        m_ScriptablePass.OnDispose();
    }
}

public class GrabColorPass : UnityEngine.Rendering.Universal.ScriptableRenderPass
{
    ProfilingSampler sampler = new("GrabColorPass");
    RTHandle cameraColorTgt;
    RTHandle GrabTex;
    public void SetUp(RTHandle r)
    {
        cameraColorTgt = r;
    }
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;
        RenderingUtils.ReAllocateIfNeeded(ref GrabTex, desc);
        cmd.SetGlobalTexture("_GrabbedColorTex", GrabTex.nameID);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get("GrabColorPass");
        using (new ProfilingScope(cmd, sampler))
        {
            Blitter.BlitCameraTexture(cmd, cameraColorTgt, GrabTex);
        }
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        cmd.Dispose();
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
    }

    public void OnDispose()
    {
        GrabTex?.Release();
    }
}
