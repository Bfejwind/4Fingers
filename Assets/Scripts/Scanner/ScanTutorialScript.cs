using UnityEngine;

public class ScanTutorialScript : MonoBehaviour
{
    private bool firstScan = true;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && firstScan)
        {
            GameManager.Instance.ScannerTutorial();
            firstScan = false;
        }
    }
}
