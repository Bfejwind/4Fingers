using UnityEngine;

public class ShipLanding : MonoBehaviour
{
    public Transform pointA;
    public float speed = 2.0f;
    public bool playerInView = false;
    void Update()
    {
        if (playerInView)
        {
            LandShip();
        } 
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerInView = true;
        }
    }
    void LandShip()
    {
        float distPerFrame = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position,pointA.position,distPerFrame);
    }
}
