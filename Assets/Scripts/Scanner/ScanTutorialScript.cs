using UnityEngine;

public class ScanTutorialScript : MonoBehaviour
{
    void OTriggerEnter(Collider other)
    {
        GameManager.Instance.ScannerTutorial();
    }
}
