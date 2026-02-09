using UnityEngine;

public class ScanTutorialScript : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.ScannerTutorial();
        }
    }
}
