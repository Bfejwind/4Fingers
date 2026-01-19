using UnityEngine;

public class XRWaterRipple : MonoBehaviour
{
    [SerializeField] private ParticleSystem ripple;
    [SerializeField] private float emitInterval = 0.2f;

    private bool inWater;
    private float timer;

    void Update()
    {   
        {
        Debug.DrawRay(transform.position, Vector3.up, Color.red);
        }
        if (!inWater) return;

        timer += Time.deltaTime;

        if (timer >= emitInterval)
        {
            ripple.transform.position = transform.position;
            ripple.Emit(1);
            timer = 0f;
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            inWater = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            inWater = false;
        }
    }
}
