using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Scanner2Script : MonoBehaviour
{
    public Material scanShader;
    public Transform scannerSpawn;
    public InputActionReference ActivateScanner;
    void Start()
    {
        ActivateScanner.action.started += ScanTerrain;
    }
    void ScanTerrain(InputAction.CallbackContext context)
    {
        StartCoroutine(ScreenScanner());
    }
    public IEnumerator ScreenScanner()
    {
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
}
