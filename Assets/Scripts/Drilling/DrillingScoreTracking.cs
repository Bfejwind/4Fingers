using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class DrillingScoreTracking : MonoBehaviour
{
    public XRRayInteractor rightRay;
    public float hitScore;
    // Update is called once per frame
    void Update()
    {
        if (rightRay.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            if (hit.collider.CompareTag("DrillTarget"))
            {
                hitScore += Time.deltaTime;
                Debug.Log(hitScore);
            }
        }
    }
    public void ClearHitScore()
    {
        hitScore = 0;
    }
}
