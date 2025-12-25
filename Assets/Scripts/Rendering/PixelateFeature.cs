using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelateFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class PixelateSettings
    {
        [Header("Material")]
        public Material pixelateMaterial = null;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        
        [Header("Pixelate Settings")]
        [Range(1, 64)]
        public float pixelSize = 8f;
        [Range(0f, 1f)]
        public float intensity = 1.0f;
        
        [Header("Quality")]
        public bool preserveAspect = true;
        
        [Header("Edge Smoothing")]
        public bool softEdges = true;
        [Range(0f, 0.3f)]
        public float edgeSoftness = 0.08f;
        
        [Header("Sample Quality")]
        [Range(1, 3)]
        public int sampleCount = 2;
    }

    public PixelateSettings settings = new PixelateSettings();

    class PixelatePass : ScriptableRenderPass
    {
        private Material pixelateMaterial;
        private int tempRTId = Shader.PropertyToID("_TempPixelateRT");
        private PixelateFeature feature;

        public PixelatePass(Material material, PixelateFeature feature)
        {
            pixelateMaterial = material;
            this.feature = feature;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (pixelateMaterial == null)
                return;

            // Получаем cameraColorTargetHandle внутри Execute
            RTHandle cameraColorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;

            CommandBuffer cmd = CommandBufferPool.Get("PixelatePass");

            // Передаем параметры в шейдер
            var settings = feature.settings;
            pixelateMaterial.SetFloat("_PixelSize", settings.pixelSize);
            pixelateMaterial.SetFloat("_Intensity", settings.intensity);
            pixelateMaterial.SetFloat("_PreserveAspect", settings.preserveAspect ? 1f : 0f);
            pixelateMaterial.SetFloat("_SoftEdges", settings.softEdges ? 1f : 0f);
            pixelateMaterial.SetFloat("_EdgeSoftness", settings.edgeSoftness);
            pixelateMaterial.SetInt("_SampleCount", settings.sampleCount);

            RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            // Создаём временный RT с FilterMode.Point
            cmd.GetTemporaryRT(tempRTId, desc, FilterMode.Point);

            // Blit: camera -> tempRT
            cmd.Blit(cameraColorTargetHandle.rt, tempRTId, pixelateMaterial);

            // Blit обратно: tempRT -> camera
            cmd.Blit(tempRTId, cameraColorTargetHandle.rt);

            // Освобождаем временный RT
            cmd.ReleaseTemporaryRT(tempRTId);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    private PixelatePass pixelatePass;

    public override void Create()
    {
        if (settings.pixelateMaterial != null)
        {
            pixelatePass = new PixelatePass(settings.pixelateMaterial, this);
            pixelatePass.renderPassEvent = settings.renderPassEvent;
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.pixelateMaterial != null)
        {
            renderer.EnqueuePass(pixelatePass);
        }
    }
}
