using UnityEngine;
using UnityEngine.InputSystem;

public class ScannerBehaviour : MonoBehaviour
{
    public GameObject ScannerPrefab;
    public float duration = 10f;
    public float size = 500f;
    public InputActionReference ActivateScanner;
    private void Start()
    {
        ActivateScanner.action.started += ScanTerrain;
    }
    void ScanTerrain(InputAction.CallbackContext context)
    {
        GameObject ScannerSpawn = Instantiate(ScannerPrefab, gameObject.transform.position, Quaternion.identity) as GameObject;
        ParticleSystem PulsesParticles = ScannerSpawn.transform.GetChild(0).GetComponent<ParticleSystem>();
        if (ScannerSpawn != null)
        {
            var main = PulsesParticles.main;
            main.startLifetime = duration;
            main.startSize = size;
        }
        else
        {
            Debug.Log("No particle system found in first child");
        }
        Destroy(ScannerSpawn,duration+1);
    }

}
