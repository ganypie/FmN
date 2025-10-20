using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DitherFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class DitherSettings
    {
        public Material ditherMaterial = null;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

        [Range(0.0f, 1.0f)]
        public float spread = 0.5f;

        [Range(2, 64)]
        public int redColorCount = 2;
        [Range(2, 64)]
        public int greenColorCount = 2;
        [Range(2, 64)]
        public int blueColorCount = 2;

        [Range(0, 2)]
        public int bayerLevel = 0;

        [Range(0, 8)]
        public int downSamples = 0;
        public bool pointFilterDown = false;
    }

    public DitherSettings settings = new DitherSettings();

    class DitherPass : ScriptableRenderPass
    {
        private Material ditherMaterial;
        private DitherSettings settings;
        private readonly int tempRT0 = Shader.PropertyToID("_TempDitherRT0");
        private readonly int tempRT1 = Shader.PropertyToID("_TempDitherRT1");

        public DitherPass(Material mat, DitherSettings s)
        {
            ditherMaterial = mat;
            settings = s;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (ditherMaterial == null)
                return;

            var cmd = CommandBufferPool.Get("DitherPass");

            var cameraData = renderingData.cameraData;
            RTHandle cameraColor = cameraData.renderer.cameraColorTargetHandle;

            if (cameraColor == null)
            {
                CommandBufferPool.Release(cmd);
                return; // Защита от null
            }

            ditherMaterial.SetFloat("_Spread", settings.spread);
            ditherMaterial.SetInt("_RedColorCount", settings.redColorCount);
            ditherMaterial.SetInt("_GreenColorCount", settings.greenColorCount);
            ditherMaterial.SetInt("_BlueColorCount", settings.blueColorCount);
            ditherMaterial.SetInt("_BayerLevel", settings.bayerLevel);
            ditherMaterial.SetInt("_DitherMode", 1);

            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            // Первый промежуточный RT
            cmd.GetTemporaryRT(tempRT0, desc, FilterMode.Bilinear);
            cmd.Blit(cameraColor.rt, tempRT0);

            // Downsample chain (ping-pong между tempRT0 и tempRT1)
            int currentSrc = tempRT0;
            int currentDst = tempRT1;
            int width = desc.width;
            int height = desc.height;

            for (int i = 0; i < settings.downSamples; i++)
            {
                width = Mathf.Max(width / 2, 2);
                height = Mathf.Max(height / 2, 2);

                cmd.GetTemporaryRT(currentDst, width, height, 0, settings.pointFilterDown ? FilterMode.Point : FilterMode.Bilinear, desc.colorFormat);
                cmd.Blit(currentSrc, currentDst, ditherMaterial, settings.pointFilterDown ? 1 : 0);
                cmd.ReleaseTemporaryRT(currentSrc);

                // swap
                int tmp = currentSrc;
                currentSrc = currentDst;
                currentDst = tmp;
            }

            // Финальный Blit на экран с дезерингом
            cmd.Blit(currentSrc, cameraColor.rt, ditherMaterial, 0);

            // Release last temp
            cmd.ReleaseTemporaryRT(currentSrc);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    private DitherPass ditherPass;

    public override void Create()
    {
        if (settings.ditherMaterial != null)
        {
            ditherPass = new DitherPass(settings.ditherMaterial, settings);
            ditherPass.renderPassEvent = settings.renderPassEvent;
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.ditherMaterial != null)
            renderer.EnqueuePass(ditherPass);
    }
}
