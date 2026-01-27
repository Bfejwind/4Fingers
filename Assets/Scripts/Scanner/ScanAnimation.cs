using UnityEngine;
using UnityEngine.InputSystem;

public class ScanAnimation : MonoBehaviour
{
    private Animator animator;
    public string trigger = "ScanStart";
    public InputActionReference ActivateScanner;
    public bool ScannerReady;
    private float time;

    void Start()
    {
        animator = GetComponent<Animator>();
        ActivateScanner.action.started += ScannerStartTrigger;
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
    void ScannerStartTrigger(InputAction.CallbackContext context)
    {
        if (ScannerReady)
        {
            ScannerReady = false;
            StartSpinning();
        }
        else
        {
            Debug.Log("Scanner Anim cooldown");
        }
    }
    public void StartSpinning()
    {
        animator.SetTrigger(trigger);
    }

}
