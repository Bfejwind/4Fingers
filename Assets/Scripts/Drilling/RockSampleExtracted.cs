using UnityEngine;
using UnityEngine.UIElements;

public class RockSampleExtracted : MonoBehaviour
{
    public GameObject sample;
    public float outForce = .1f;
    public Transform spawnOrigin;
    public ParticleSystem rockBreakVFX;
    public void SpawnSample()
    {
        rockBreakVFX.Play();
        GameObject sampleInstance = Instantiate(sample,spawnOrigin.position,Quaternion.identity);
        Rigidbody sampleRB = sampleInstance.GetComponent<Rigidbody>();
        if (sampleRB != null)
        {
            //random direction
            Vector3 launchDir = Random.onUnitSphere;
            //Make that direction upwards in y axis
            launchDir.y = Mathf.Abs(launchDir.y);
            //Shoot out
            sampleRB.AddForce(launchDir.normalized*outForce, ForceMode.VelocityChange);
        }
    }
}
