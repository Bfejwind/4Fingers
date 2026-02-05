using Unity.VisualScripting;
using UnityEngine;

public class RepairBehaviour : MonoBehaviour
{
    private float originalRotation;
    private float newRotation;
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wrench"))
        {
            originalRotation = transform.localEulerAngles.y;
        }
    }
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wrench"))
        {
            newRotation = transform.localEulerAngles.y;
            float angleRotated = newRotation - originalRotation;
            if (angleRotated>= 30f)
            {
                angleRotated = angleRotated-15;
                GameManager.Instance.RepairHealth(angleRotated);
            }
        }
    }
}
