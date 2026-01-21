using System.Collections;
using UnityEngine;

public class SampleMaterialChange : MonoBehaviour
{
    public Material sampleOriginal;
    public Material sampleGlow;
    // void OnTriggerEnter(Collider other)
    // {
    //     Debug.Log("Collided with " + other.name);

    //     if (other.CompareTag("Scanner"))
    //     {
    //         Debug.Log("Scanner detected");
    //         StartCoroutine(GlowFade());
    //     }
    // }
    void OnParticleCollision(GameObject other)
    {
        Debug.Log("Collided with " + other.gameObject.name);
    }
    IEnumerator GlowFade()
    {
        this.GetComponent<Renderer>().material = sampleGlow;
        yield return new WaitForSeconds(3.0f);
        this.GetComponent<Renderer>().material = sampleOriginal;
    }
}
