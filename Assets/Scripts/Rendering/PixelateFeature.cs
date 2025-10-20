using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelateFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class PixelateSettings
    {
        public Material pixelateMaterial = null;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public PixelateSettings settings = new PixelateSettings();

    class PixelatePass : ScriptableRenderPass
    {
        private Material pixelateMaterial;
        private int tempRTId = Shader.PropertyToID("_TempPixelateRT");

        public PixelatePass(Material material)
        {
            pixelateMaterial = material;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (pixelateMaterial == null)
                return;

            // Получаем cameraColorTargetHandle внутри Execute
            RTHandle cameraColorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;

            CommandBuffer cmd = CommandBufferPool.Get("PixelatePass");

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
        pixelatePass = new PixelatePass(settings.pixelateMaterial);
        pixelatePass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.pixelateMaterial != null)
        {
            renderer.EnqueuePass(pixelatePass);
        }
    }
}
