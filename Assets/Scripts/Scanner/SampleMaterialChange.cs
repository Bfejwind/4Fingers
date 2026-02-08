using System.Collections;
using UnityEngine;

public class SampleMaterialChange : MonoBehaviour
{
    private Material sampleOriginal;
    public Material sampleGlow;
    void Start()
    {
        sampleOriginal = GetComponent<Material>();
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
        this.GetComponent<Renderer>().material = sampleGlow;
        yield return new WaitForSeconds(3.0f);
        this.GetComponent<Renderer>().material = sampleOriginal;
    }
}
