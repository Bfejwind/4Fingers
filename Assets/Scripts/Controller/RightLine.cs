using UnityEngine;

public class RightLine : MonoBehaviour
{
    public GameObject controllerLine;
    public void HoverOver()
    {
        controllerLine.SetActive(true);
    }
    public void HoverEnd()
    {
        controllerLine.SetActive(false);
    }
}
