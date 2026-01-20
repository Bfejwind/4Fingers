using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Collider))]
public class UnderwaterTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerHead;      // usually the Main Camera inside XR Origin
    [SerializeField] private Volume normalVolume;      // global normal volume
    [SerializeField] private Volume underwaterVolume;  // box underwater volume

    [Header("Settings")]
    [SerializeField] private float waterSurfaceY = 0f; // Y position of water surface
    [SerializeField] private float blendSpeed = 1f;

    private bool playerInTrigger = false;

    private void Start()
    {
        // Ensure collider is a trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        // Initialize weights
        if (normalVolume != null) normalVolume.weight = 1f;
        if (underwaterVolume != null) underwaterVolume.weight = 0f;
    }

    private void Update()
    {
        if (normalVolume == null || underwaterVolume == null || playerHead == null)
            return;

        // Determine if player is underwater
        bool underwater = playerInTrigger && (playerHead.position.y < waterSurfaceY);

        // Smooth blend
        normalVolume.weight = Mathf.MoveTowards(normalVolume.weight, underwater ? 0f : 1f, blendSpeed * Time.deltaTime);
        underwaterVolume.weight = Mathf.MoveTowards(underwaterVolume.weight, underwater ? 1f : 0f, blendSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
        }
    }
}
