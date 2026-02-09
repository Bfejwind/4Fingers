using System.Collections;
using UnityEngine;

public class SampleMaterialChange : MonoBehaviour
{
    // private Material sampleOriginal;
    // public Material sampleGlow;
    // private Renderer sampleRenderer;
    Renderer rend;
    MaterialPropertyBlock mpb;
    Color baseColor;
    public Color glowColor = Color.blue;
    public float glowIntensity = 2.6f;
    public float fadeDuration = 2.0f;
    public float holdTime = 2.0f;
    static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    void Awake()
    {
        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();

        // Read starting color from material
        baseColor = rend.sharedMaterial.GetColor(BaseColorID);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Scanner"))
        {
            StopAllCoroutines();
            StartCoroutine(GlowRoutine());
        }
    }

    IEnumerator GlowRoutine()
    {
        // Fade IN
        yield return Fade(0f, 1f);

        yield return new WaitForSeconds(holdTime);

        // Fade OUT
        yield return Fade(1f, 0f);
    }
    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        Color emissionTarget = glowColor * glowIntensity;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.Lerp(from, to, t / fadeDuration);

            mpb.SetColor(BaseColorID, Color.Lerp(baseColor, glowColor, lerp));
            mpb.SetColor(EmissionColorID, Color.Lerp(Color.black, emissionTarget, lerp));

            rend.SetPropertyBlock(mpb);
            yield return null;
        }
    }
}
