using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FogVolume : MonoBehaviour
{
    [Header("Fog Settings")]
    public bool enableFog = true;
    public Color fogColor = Color.gray;
    public float fogDensity = 0.02f;

    private bool originalFog;
    private Color originalColor;
    private float originalDensity;

    void Start()
    {
        // Ensure this collider is a trigger
        GetComponent<Collider>().isTrigger = true;

        // Store original fog settings
        originalFog = RenderSettings.fog;
        originalColor = RenderSettings.fogColor;
        originalDensity = RenderSettings.fogDensity;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        RenderSettings.fog = enableFog;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Restore original fog settings
        RenderSettings.fog = originalFog;
        RenderSettings.fogColor = originalColor;
        RenderSettings.fogDensity = originalDensity;
    }
}
