using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Underwater : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material material;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;

        [Header("Underwater Settings")]
        public Color color = Color.cyan;
        public float distance = 10f;

        [Range(0, 1)]
        public float alpha = 0.5f;

        public float refraction = 0.1f;
        public Texture normalmap;
        public Vector4 UV = new Vector4(1, 1, 0.2f, 0.1f);
    }

    public Settings settings = new Settings();

    class UnderwaterPass : ScriptableRenderPass
    {
        private Settings settings;

        private RTHandle source;
        private RTHandle tempTexture;

        public UnderwaterPass(Settings settings)
        {
            this.settings = settings;
        }

        public void Setup(RTHandle source)
        {
            this.source = source;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor desc)
        {
            RenderingUtils.ReAllocateIfNeeded(
                ref tempTexture,
                desc,
                FilterMode.Bilinear,
                TextureWrapMode.Clamp,
                name: "_UnderwaterTempTexture"
            );

            ConfigureTarget(tempTexture);
            ConfigureClear(ClearFlag.None, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (settings.material == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("Underwater Effect");

            // Material properties
            settings.material.SetFloat("_dis", settings.distance);
            settings.material.SetFloat("_alpha", settings.alpha);
            settings.material.SetColor("_color", settings.color);
            settings.material.SetTexture("_NormalMap", settings.normalmap);
            settings.material.SetFloat("_refraction", settings.refraction);
            settings.material.SetVector("_normalUV", settings.UV);

            // Copy camera color -> temp
            Blitter.BlitCameraTexture(cmd, source, tempTexture);

            // Apply underwater effect -> back to camera color
            Blitter.BlitCameraTexture(cmd, tempTexture, source, settings.material, 0);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            tempTexture?.Release();
        }
    }

    UnderwaterPass pass;

    public override void Create()
    {
        pass = new UnderwaterPass(settings);
        pass.renderPassEvent = settings.renderPassEvent;
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        pass.Setup(renderer.cameraColorTargetHandle);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }
}
