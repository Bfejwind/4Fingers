using UnityEngine;

public class SpinForever : MonoBehaviour
{
    [SerializeField] float degreesPerSecond = 90f;
    [SerializeField] bool worldSpace = true;

    void Update()
    {
        float angle = degreesPerSecond * Time.deltaTime;
        if (worldSpace)
            transform.Rotate(0f, angle, 0f, Space.World);
        else
            transform.Rotate(0f, angle, 0f, Space.Self);
    }
}
