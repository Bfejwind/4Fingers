using Unity.VisualScripting;
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
        if (GameManager.Instance.firstDrill)
        {
            return;
        }
        else
        {
            controllerLine.SetActive(false);
            GameManager.Instance.firstDrill = false;
        }
    }
}
