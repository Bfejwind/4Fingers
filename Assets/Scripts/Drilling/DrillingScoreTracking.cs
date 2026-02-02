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
        float timeOnTarget = Time.time - hoverInstance;
        timeOnTarget = Mathf.Round(timeOnTarget * 100f) / 100f; // Round to 2 decimal places
        hitScore += timeOnTarget;
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
    public float GetCurrentTotalScore()
    {
        float finalScore = hitScore;
        if (onTarget)
        {
            // Add the time spent since the last HoverEnter until right now
            float activeTime = Time.time - hoverInstance;
            finalScore += activeTime;
        }
        return Mathf.Round(finalScore * 100f) / 100f;
    }
}
