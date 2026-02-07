using System.Collections;
using UnityEngine;

public class StormMovement : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2.0f;
    void Update()
    {
        float distPerFrame = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position,pointA.position,distPerFrame);
    }
}
