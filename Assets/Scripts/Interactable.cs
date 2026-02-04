using UnityEngine;

public class Interactable : MonoBehaviour
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
