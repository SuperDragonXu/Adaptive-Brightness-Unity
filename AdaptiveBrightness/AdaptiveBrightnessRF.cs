using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AdaptiveBrightnessRF : ScriptableRendererFeature
{
    
    class CustomRenderPass : UnityEngine.Rendering.Universal.ScriptableRenderPass
    {                
        const string ProfilerTag = "Adaptive Brightness";
        ProfilingSampler sampler = new(ProfilerTag);
        RTHandle cameraColorTgt;

        RTHandle tempRT;        
        public Material material;

        public ComputeShader compute;
        
        string kernelName = "avgBrightness";
        int KernelHandle;
        float L_lastFrame=0;
        
        public AdaptiveBrightnessVolume volume;

        public void GetTempRT(in RenderingData data)
        {            
            RenderTextureDescriptor desc = data.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;            
            RenderingUtils.ReAllocateIfNeeded(ref tempRT, desc);            
        }
        public void SetUp(RTHandle r)
        {
            cameraColorTgt = r;
        }
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureInput(ScriptableRenderPassInput.Color);
            ConfigureTarget(cameraColorTgt);
            KernelHandle = compute.FindKernel(kernelName);            
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {            
            int width= renderingData.cameraData.cameraTargetDescriptor.width;
            int height = renderingData.cameraData.cameraTargetDescriptor.height;
            
            int length = width * height / 64;

            
            KernelHandle = compute.FindKernel(kernelName);

            ComputeBuffer computeRes = new ComputeBuffer(length, sizeof(float), ComputeBufferType.Append);

            CommandBuffer cmd = CommandBufferPool.Get(ProfilerTag);
            
            compute.SetBuffer(KernelHandle, "result", computeRes);
            compute.SetInt("width", 8);
            compute.SetInt("height", 8);
            compute.SetTexture(KernelHandle, "CameraColor", cameraColorTgt);
            compute.Dispatch(KernelHandle, width/8, height/8, 1);
            
            
            float[] res = new float[length];
            computeRes.GetData(res);

            float avgL=0;
            foreach (var item in res)
            {
                
                avgL += item;
            }
            avgL /= length;            
            float resL = (avgL + L_lastFrame) / 2;
            L_lastFrame = avgL;            
            //Debug.Log("avgL: " + avgL);


            using (new ProfilingScope(cmd, sampler))
            {
                material.SetFloat(avgL_ID, resL);
                material.SetFloat(speedL_ID, 1);
                material.SetFloat(targetL_ID, 0.3f);

                CoreUtils.SetRenderTarget(cmd, tempRT);
                Blitter.BlitTexture(cmd, cameraColorTgt, new Vector4(1, 1, 0, 0), material, 0);
                CoreUtils.SetRenderTarget(cmd, cameraColorTgt);
                Blitter.BlitTexture(cmd, cameraColorTgt, cameraColorTgt, material, 0);    // 将操作添加入命令缓冲区                          
            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            cmd.Dispose();
            computeRes?.Release();
            computeRes = null;
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            tempRT?.Release();            
        }

        public void OnDispose()
        {
            tempRT?.Release();            
        }
    }

    //____________________________________________________________________________
    public static readonly int avgL_ID = Shader.PropertyToID("avgL");
    public static readonly int speedL_ID = Shader.PropertyToID("_AdaptingSpeed");
    public static readonly int targetL_ID= Shader.PropertyToID("_TargetL");

    CustomRenderPass m_ScriptablePass;
    public RenderPassEvent InjectionPoint = RenderPassEvent.AfterRenderingTransparents;

    public Material material;
    public ComputeShader compute;

    VolumeStack stack;
    AdaptiveBrightnessVolume volume;
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass() { material = this.material, compute = this.compute };
        
        m_ScriptablePass.renderPassEvent = InjectionPoint;

        stack = VolumeManager.instance.stack;
        volume = stack.GetComponent<AdaptiveBrightnessVolume>();
        m_ScriptablePass.volume = volume;
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (!shouldExecute(renderingData))
            return;
        m_ScriptablePass.SetUp(renderer.cameraColorTargetHandle);
    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!shouldExecute(renderingData))
            return;        
        renderer.EnqueuePass(m_ScriptablePass);
        m_ScriptablePass.GetTempRT(in renderingData);
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


