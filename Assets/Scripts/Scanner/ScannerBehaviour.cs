using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScannerBehaviour : MonoBehaviour
{
    public GameObject scannerPrefab;
    public float duration = 10f;
    public float size = 500f;
    public InputActionReference ActivateScanner;
    public GameObject scanSphere;
    private void Start()
    {
        ActivateScanner.action.started += ScanTerrain;
        scanSphere.SetActive(false);
    }
    void ScanTerrain(InputAction.CallbackContext context)
    {
        GameObject ScannerSpawn = Instantiate(scannerPrefab, gameObject.transform.position, Quaternion.identity) as GameObject;
        ParticleSystem PulsesParticles = ScannerSpawn.transform.GetChild(0).GetComponent<ParticleSystem>();
        if (ScannerSpawn != null)
        {
            var main = PulsesParticles.main;
            main.startLifetime = duration;
            main.startSize = size;
            StartCoroutine(ActivateScanSphere());
        }
        else
        {
            Debug.Log("No particle system found in first child");
        }
        Destroy(ScannerSpawn,duration+1);
    }
    IEnumerator ActivateScanSphere()
    {
        scanSphere.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        scanSphere.SetActive(false);
    }

}
