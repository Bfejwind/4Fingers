using UnityEngine;

public class Scanner2Script : MonoBehaviour
{
    public Renderer scanRenderer;
    public Material scanShader;
    void Start()
    {
        if (scanRenderer != null)
        {
            scanShader = scanRenderer.material;
        }
    }
}
