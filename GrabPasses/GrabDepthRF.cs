using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GrabDepthRF : ScriptableRendererFeature
{
    
    GrabDepthPass m_ScriptablePass;
    public RenderPassEvent InjectionPoint = RenderPassEvent.AfterRenderingTransparents;
    public override void Create()
    {
        m_ScriptablePass = new GrabDepthPass();
        m_ScriptablePass.OnCreate();
        m_ScriptablePass.renderPassEvent = InjectionPoint;
    }
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (!shouldExecute(renderingData))
            return;
        m_ScriptablePass.SetUp(renderer.cameraDepthTargetHandle);
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

public class GrabDepthPass : UnityEngine.Rendering.Universal.ScriptableRenderPass
{
    ProfilingSampler sampler = new("GrabDepth");
    RTHandle cameraDepth;
    RTHandle GrabDepthTex;
    Material mat;
    public void OnCreate()
    {
        mat = CoreUtils.CreateEngineMaterial("Hidden/Universal Render Pipeline/CopyDepth");
    }

    public void SetUp(RTHandle r)
    {
        cameraDepth = r;
    }
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;

        desc.depthBufferBits = 32;
        desc.colorFormat = RenderTextureFormat.Depth;
        
        desc.msaaSamples = 1;
        desc.bindMS = false;
        
        RenderingUtils.ReAllocateIfNeeded(ref GrabDepthTex, desc);
        cmd.SetGlobalTexture("_GrabbedDepthTex", GrabDepthTex.nameID);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get("GrabDepthPass");
        using(new ProfilingScope(cmd, sampler))
        {
            mat.DisableKeyword("_DEPTH_MSAA_2");
            mat.DisableKeyword("_DEPTH_MSAA_4");
            mat.DisableKeyword("_DEPTH_MSAA_8");
            mat.EnableKeyword("_OUTPUT_DEPTH");
            Blitter.BlitCameraTexture(cmd, cameraDepth, GrabDepthTex, mat, 0);
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
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            if (mat != null)
            {
                UnityEngine.Object.Destroy(mat);
            }
        }
        else
        {
            if (mat != null)
            {
                UnityEngine.Object.DestroyImmediate(mat);
            }
        }
#else
        if (mat != null)
            {
                UnityEngine.Object.Destroy(mat);
            }
#endif
    }
}

