using System.Collections;
using UnityEngine;

public class IntroSequenceManager : MonoBehaviour
{
    [Header("Part 1: Warning Settings")]
    public GameObject warningPanel;     // The parent object holding all your warning texts
    public CanvasGroup warningCanvasGroup; // Used to control the transparency (Alpha)
    
    public float warningDuration = 10.0f; // Total time the warning stays on screen
    public float flickerSpeed = 0.08f;   // How fast the text flickers (lower is faster)

    void Start()
    {
        // Start the sequence as soon as the game begins
        StartCoroutine(PlayWarningSequence());
    }

    IEnumerator PlayWarningSequence()
    {
        // --- STEP 1: WARNING PHASE ---
        
        warningPanel.SetActive(true); // Make sure it's visible
        float timer = 0f;

        while (timer < warningDuration)
        {
            // This creates a "glitch" effect by randomizing transparency
            // We keep it between 0.2 (dim) and 1.0 (full brightness) so it's readable but unstable
            float randomAlpha = Random.Range(0.2f, 1.0f);

            if (warningCanvasGroup != null)
            {
                warningCanvasGroup.alpha = randomAlpha;
            }

            // Wait for a tiny moment before flickering again
            yield return new WaitForSeconds(flickerSpeed);
            timer += flickerSpeed;
        }

        // Turn off the warning completely
        warningPanel.SetActive(false);
        
        // This is where we will add the code for Part 2 (Subtitles) later!
        Debug.Log("Warning sequence finished. Ready for subtitles.");
    }
}