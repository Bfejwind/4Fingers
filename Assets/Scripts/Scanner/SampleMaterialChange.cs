using System.Collections;
using UnityEngine;

public class SampleMaterialChange : MonoBehaviour
{
    private Material sampleOriginal;
    public Material sampleGlow;
    private Renderer sampleRenderer;
    void Start()
    {
        sampleRenderer = GetComponent<Renderer>();
        sampleOriginal = sampleRenderer.material;
    }
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided with " + other.name);

        if (other.CompareTag("Scanner"))
        {
            Debug.Log("Scanner detected");
            StartCoroutine(GlowFade());
        }
    }

    IEnumerator GlowFade()
    {
        sampleRenderer.material = sampleGlow;
        yield return new WaitForSeconds(3.0f);
        sampleRenderer.material = sampleOriginal;
    }
}
