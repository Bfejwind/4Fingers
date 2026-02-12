using UnityEngine;

public class HideMarkerOnReach : MonoBehaviour
{
    [SerializeField] GameObject markerRoot;     
    [SerializeField] string playerTag = "Player";

    void Reset()
    {
        
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (markerRoot != null)
            markerRoot.SetActive(false);
        else
            Debug.LogWarning("markerRoot not assigned on HideMarkerOnReach.");
    }
}
