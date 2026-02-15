using System.Collections;
using UnityEngine;
using TMPro;

public class IntroSequenceManager : MonoBehaviour
{
    [Header("Part 1: Warning Settings")]
    public GameObject warningPanel;      
    public CanvasGroup warningCanvasGroup; 
    
    public float warningDuration = 10.0f; 
    public float flickerSpeed = 0.08f;   

    [Header("Part 2: Subtitle Settings")]
    public TextMeshProUGUI subtitleText; 
    public float delayBeforeSubtitles = 2.0f; 
    public float typingSpeed = 0.05f;    
    public float delayBetweenLines = 2.0f; 

    [TextArea(3, 10)] 
    public string[] storyLines; 

    void Start()
    {
        if(subtitleText != null) subtitleText.text = "";
        StartCoroutine(PlayFullSequence());
    }

    IEnumerator PlayFullSequence()
    {
        // --- PART 1: WARNING ---
        warningPanel.SetActive(true);
        GameManager.Instance.PauseGame();
        float timer = 0f;

        while (timer < warningDuration)
        {
            // Flicker effect
            float randomAlpha = Random.Range(0.2f, 1.0f);
            if (warningCanvasGroup != null) warningCanvasGroup.alpha = randomAlpha;
            
            yield return new WaitForSecondsRealtime(flickerSpeed);
            timer += flickerSpeed;
        }

        warningPanel.SetActive(false); 

        // --- PART 2: SUBTITLES ---
        yield return new WaitForSecondsRealtime(delayBeforeSubtitles);

        foreach (string line in storyLines)
        {
            subtitleText.text = ""; 
            
            foreach (char letter in line.ToCharArray())
            {
                subtitleText.text += letter;
                yield return new WaitForSecondsRealtime(typingSpeed);
            }

            yield return new WaitForSecondsRealtime(delayBetweenLines);
        }

        subtitleText.text = "";
        Debug.Log("Intro sequence complete.");
        GameManager.Instance.ResumeGame();
    }
}