using UnityEngine;

public class XRCameraWaterRipple : MonoBehaviour
{
    [SerializeField] private ParticleSystem ripple;
    [SerializeField] private float emitInterval = 0.2f;

    private bool inWater;
    private float timer;

    void Update()
    {
        if (!inWater) return;

        timer += Time.deltaTime;
        if (timer >= emitInterval)
        {
            Vector3 footPos = transform.position + Vector3.down * 1.2f;
            ripple.transform.position = footPos;
            ripple.Emit(1);
            timer = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
            inWater = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
            inWater = false;
    }
}
