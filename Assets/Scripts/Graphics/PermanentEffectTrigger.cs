using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Collider))]
public class LingeringFogVolume : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Volume fogVolume;

    [Header("Fog Fade")]
    [SerializeField] private float fadeInSpeed = 3f;
    [SerializeField] private float fadeOutTime = 3f;

    private bool playerInside = false;
    private float fogWeight = 0f;

    private void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        fogVolume.weight = 0f;
    }

    private void Update()
    {
        if (playerInside)
        {
            // Fade IN fast
            fogWeight = Mathf.MoveTowards(fogWeight, 1f, fadeInSpeed * Time.deltaTime);
        }
        else
        {
            // Fade OUT slowly after exit
            fogWeight = Mathf.MoveTowards(fogWeight, 0f, Time.deltaTime / fadeOutTime);
        }

        fogVolume.weight = fogWeight;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }
}
