using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FogFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class FogSettings
    {
        public Material fogMaterial = null;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        public Color fogColor = Color.white;
        [Range(0.0f, 1.0f)] public float fogDensity = 0.5f;
        [Range(0.0f, 100.0f)] public float fogOffset = 0f;
    }

    public FogSettings settings = new FogSettings();

    class FogPass : ScriptableRenderPass
    {
        private Material fogMaterial;
        private FogSettings settings;
        private int tempRTID = Shader.PropertyToID("_TempFogRT");

        public FogPass(Material mat, FogSettings s)
        {
            fogMaterial = mat;
            settings = s;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (fogMaterial == null)
                return;

            var cmd = CommandBufferPool.Get("FogPass");

            var cameraData = renderingData.cameraData;
            RTHandle cameraColor = cameraData.renderer.cameraColorTargetHandle;

            if (cameraColor == null)
            {
                CommandBufferPool.Release(cmd);
                return;
            }

            fogMaterial.SetColor("_FogColor", settings.fogColor);
            fogMaterial.SetFloat("_FogDensity", settings.fogDensity);
            fogMaterial.SetFloat("_FogOffset", settings.fogOffset);

            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            cmd.GetTemporaryRT(tempRTID, desc, FilterMode.Bilinear);
            cmd.Blit(cameraColor.rt, tempRTID, fogMaterial);
            cmd.Blit(tempRTID, cameraColor.rt);
            cmd.ReleaseTemporaryRT(tempRTID);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    private FogPass fogPass;

    public override void Create()
    {
        if (settings.fogMaterial != null)
        {
            fogPass = new FogPass(settings.fogMaterial, settings);
            fogPass.renderPassEvent = settings.renderPassEvent;
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.fogMaterial != null)
            renderer.EnqueuePass(fogPass);
    }
}
