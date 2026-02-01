using UnityEngine;

public class RepairBehaviour : MonoBehaviour
{
    private float originalRotation;
    private 
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wrench"))
        {
            
        }
    }
}
