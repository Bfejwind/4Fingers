using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScannerBehaviour : MonoBehaviour
{
    public GameObject scannerPrefab;
    public float duration = 5f;
    public float size = 100f;
    public InputActionReference ActivateScanner;
    public GameObject scanSphere;
    public bool ScannerReady;
    private float time;
    private void Start()
    {
        ActivateScanner.action.started += ScanTerrain;
        scanSphere.SetActive(false);
        ScannerReady = true;
        time = 0;
    }
    void Update()
    {
        if (!ScannerReady)
        {
            time += Time.deltaTime;
        }
        if (time > 15.0f)
        {
            ScannerReady = true;
            time = 0;
        }
    }
    void ScanTerrain(InputAction.CallbackContext context)
    {
        if (ScannerReady)
        {
            ScannerReady = false;
            Invoke("ScanDelay",2.5f);
        }
        else
        {
            Debug.Log("Scanner is on cooldown");
        }
    }
    void ScanDelay()
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
        yield return new WaitForSeconds(3.0f);
        scanSphere.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        scanSphere.SetActive(false);
    }

}
