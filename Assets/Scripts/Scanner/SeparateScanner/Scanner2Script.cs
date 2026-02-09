using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Scanner2Script : MonoBehaviour
{
    public Material scanShader;
    public Transform scannerSpawn;
    public InputActionReference ActivateScanner;
    public GameObject scanSphere;
    public bool ScannerReady;
    public float cooldown;
    public float resetCooldown;
    void Start()
    {
        ActivateScanner.action.started += ScanTerrain;
        scanSphere.SetActive(false);
        ScannerReady = true;
        resetCooldown = 10.0f;
        cooldown = resetCooldown;
    }
    void Update()
    {
        if (!ScannerReady)
        {
            cooldown -= Time.deltaTime;
            cooldown = Mathf.Clamp(cooldown,0,resetCooldown);
        }
        if (cooldown == 0)
        {
            ScannerReady = true;
            cooldown = resetCooldown;
        }
    }
    void ScanTerrain(InputAction.CallbackContext context)
    {
        if (ScannerReady)
        {    
            StartCoroutine(ScreenScanner());
            StartCoroutine(ActivateScanSphere());
        }
    }
    public IEnumerator ScreenScanner()
    {
        yield return new WaitForSeconds(2.5f);
        ScannerReady = false;
        float timer = 0;
        float scanRange = 0;
        float opacity = 1;
        scanShader.SetVector("_position",scannerSpawn.position);
        while (true)
        {
            timer +=Time.deltaTime;
            if (timer <= 1)
            {
                scanRange = Mathf.Lerp(0,100,timer);
                opacity = Mathf.Lerp(1,0,timer);
                scanShader.SetFloat("_range",scanRange);
                scanShader.SetFloat("_opacity",opacity);
            }
            else
            {
                yield break;
            }
            yield return null;
        }
    }
    IEnumerator ActivateScanSphere()
    {
        yield return new WaitForSeconds(3.0f);
        scanSphere.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        scanSphere.SetActive(false);
    }

}
