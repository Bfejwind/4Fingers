using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Collider))]
public class PermanentEffectTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Volume globalEffectVolume;

    [Header("Damage")]
    [SerializeField] private float damagePerSecond = 10f;

    private bool playerInside = false;
    private bool effectActivated = false;

    private void Start()
    {
        GetComponent<Collider>().isTrigger = true;

        // Ensure effect starts OFF
        if (globalEffectVolume != null)
            globalEffectVolume.weight = 0f;
    }

    private void Update()
    {
        if (playerInside)
        {
            DealDamage(damagePerSecond * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;

        // Activate effect ONCE
        if (!effectActivated)
        {
            globalEffectVolume.weight = 1f; // permanent
            effectActivated = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false; // damage stops, effect stays
    }

    private void DealDamage(float amount)
    {
        // Hook into your health system
        Debug.Log($"Damage: {amount}");
    }
}
