using UnityEngine;

public class ToolFollow : MonoBehaviour
{
    public Transform cameraTransform;
    public Vector3 offset = new Vector3(0,0,1.0f);
    void LateUpdate()
    {
        if (cameraTransform != null)
        {
            // Follow position, ignore rotation
            transform.position = cameraTransform.position + offset;
        }
    }
}
