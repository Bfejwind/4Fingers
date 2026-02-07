using UnityEngine;

public class WiperTargetL : MonoBehaviour
{
    public bool wipedOn = false;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Wiper"))
        {
            wipedOn = true;
        }
    }
}
