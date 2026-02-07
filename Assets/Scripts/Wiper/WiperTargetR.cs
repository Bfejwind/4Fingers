using UnityEngine;

public class WiperTargetR : MonoBehaviour
{
    public bool wipedOn = false;
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided Head");
        if (other.gameObject.CompareTag("Wiper"))
        {
            wipedOn = true;
        }
    }
}
