using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class DrillingScoreTracking : MonoBehaviour
{
    public float hitScore;
    public float hoverInstance;
    public bool onTarget = false;
    void Awake()
    {
        hitScore = 0;
        hoverInstance = 0;
        onTarget = false;
    }
    public void HoverEnter()
    {
        hoverInstance = Time.time;
        onTarget = true;
    }
    public void HoverExit()
    {
        hitScore += Time.time - hoverInstance;
        onTarget = false;
    }
    void Update()
    {
        // if (onTarget)
        // {
            
        // }
    }
    public void ClearHitScore()
    {
        Debug.Log(hitScore);
        hitScore = 0;
    }
}
