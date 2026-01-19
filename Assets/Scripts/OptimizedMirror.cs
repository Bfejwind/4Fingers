using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Camera))]
public class OptimizedMirror : MonoBehaviour
{
    public Camera mainCamera;
    public RenderTexture mirrorTexture;
    public LayerMask mirrorLayers; // Only render essential layers
    public int resolutionDivider = 2; // 1 = full, 2 = half, 4 = quarter

    private Camera mirrorCamera;

    void Start()
    {
        // Create optimized mirror camera
        GameObject camObj = new GameObject("MirrorCamera");
        camObj.transform.parent = transform;
        mirrorCamera = camObj.AddComponent<Camera>();

        // Copy main camera settings
        mirrorCamera.CopyFrom(mainCamera);

        // Transparent clear and no post-processing
        mirrorCamera.clearFlags = CameraClearFlags.SolidColor;
        mirrorCamera.backgroundColor = new Color(0, 0, 0, 0);
        mirrorCamera.cullingMask = mirrorLayers;

        // Disable Universal RP post-processing for this camera
        if (mirrorCamera.TryGetComponent<UniversalAdditionalCameraData>(out var camData))
        {
            camData.renderPostProcessing = false;
        }

        // Create RenderTexture at reduced resolution
        int width = Screen.width / resolutionDivider;
        int height = Screen.height / resolutionDivider;
        mirrorTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        mirrorTexture.name = "MirrorTexture";
        mirrorTexture.useMipMap = false;
        mirrorTexture.autoGenerateMips = false;

        mirrorCamera.targetTexture = mirrorTexture;

        // Assign texture to your ripple material
        Renderer rend = GetComponent<Renderer>();
        if (rend) rend.material.SetTexture("_MainTex", mirrorTexture);
    }

    void LateUpdate()
    {
        if (mirrorCamera)
        {
            // Position & rotate mirror camera to mimic main camera
            mirrorCamera.transform.position = mainCamera.transform.position;
            mirrorCamera.transform.rotation = mainCamera.transform.rotation;

            // Render manually (URP)
            mirrorCamera.Render();
        }
    }
}
