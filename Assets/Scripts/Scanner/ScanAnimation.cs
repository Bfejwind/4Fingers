using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class ScanAnimation : MonoBehaviour
{
    private Animator animator;
    private AudioSource audioSource;

    [Header("Animation")]
    public string trigger = "ScanStart";

    [Header("Input")]
    public InputActionReference ActivateScanner;

    [Header("Audio")]
    public AudioClip scanSound;
    public float audioDelay = 2f;

    [Header("Cooldown")]
    public bool ScannerReady;
    public float cooldownTime = 15f;

    private float time;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        ScannerReady = true;
        ActivateScanner.action.started += ScannerStartTrigger;
        time = 0;
    }

    void Update()
    {
        if (!ScannerReady)
        {
            time += Time.deltaTime;
            if (time > cooldownTime)
            {
                ScannerReady = true;
                time = 0;
            }
        }
    }

    void ScannerStartTrigger(InputAction.CallbackContext context)
    {
        if (!ScannerReady)
        {
            Debug.Log("Scanner Anim cooldown");
            return;
        }
        else
        {
            StartSpinning();
            StartCoroutine(PlayScanSoundDelayed());
            ScannerReady = false;
        }

    }

    public void StartSpinning()
    {
        animator.SetTrigger(trigger);
    }

    IEnumerator PlayScanSoundDelayed()
    {
        yield return new WaitForSeconds(audioDelay);

        if (scanSound != null)
        {
            audioSource.PlayOneShot(scanSound);
        }
    }
}
