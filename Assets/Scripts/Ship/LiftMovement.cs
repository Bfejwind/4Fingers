using UnityEngine;

public class LiftMovement : MonoBehaviour
{
    public Transform pointA;
    public GameObject player;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.transform.position = pointA.position;
        }
    }
}
