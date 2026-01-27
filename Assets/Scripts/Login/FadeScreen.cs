using UnityEngine;
using System.Collections;

/// <summary>
/// Manages screen fade in and fade out transitions.
/// </summary>
public class FadeScreen : MonoBehaviour
{   
    public bool fadeOnStart = true; // Whether to fade in on start
    public float fadeDuration = 2f; // Duration of the fade effect in seconds
    public Color fadeColor; // Color to fade to/from
    private Renderer rend; // Renderer component for the fade screen

    /// <summary>
    /// Initializes the fade screen and starts fade-in if specified.
    /// </summary>
    void Start()
    {
        rend = GetComponent<Renderer>();
        
        // Set alpha to 0 immediately so it's invisible
        Color initColor = fadeColor;
        initColor.a = 0;
        rend.material.SetColor("_BaseColor", initColor);

        if (fadeOnStart)
        {
            FadeIn(); // This fades from 1 to 0 (making it invisible if it wasn't)
        }
    }

    /// <summary>
    /// Initiates a fade-in effect.
    /// </summary>
    public void FadeIn()
    {
        Fade(1, 0);
    }

    /// <summary>
    /// Initiates a fade-out effect.
    /// </summary>
    public void FadeOut()
    {
        Fade(0, 1);
    }

    /// <summary>
    /// Starts the fade coroutine with specified alpha values.
    /// </summary>
    /// <param name="alphaIn">Starting alpha value (0-1)</param>
    /// <param name="alphaOut">Ending alpha value (0-1)</param>
    public void Fade(float alphaIn, float alphaOut)
    {
        StartCoroutine(FadeRoutine(alphaIn, alphaOut));
    }

    /// <summary>
    /// Coroutine to handle the fade effect over time.
    /// </summary>
    /// <param name="alphaIn">Starting alpha value (0-1)</param>
    /// <param name="alphaOut">Ending alpha value (0-1)</param>
    /// <returns>IEnumerator for coroutine execution</returns>
    public IEnumerator FadeRoutine(float alphaIn, float alphaOut)
    {   
        float timer = 0; // Timer to track fade progress
        
        // Fade over the specified duration
        while (timer <= fadeDuration)
        {   
            Color newColor = fadeColor; // Current color to modify
            newColor.a = Mathf.Lerp(alphaIn, alphaOut, timer / fadeDuration);

            rend.material.SetColor("_BaseColor", newColor); // Update material color

            timer += Time.deltaTime;  // Increment timer
            yield return null;
        }

        // Ensure final alpha is set
        Color newColor2 = fadeColor;   
        newColor2.a = alphaOut;
        rend.material.SetColor("_BaseColor", newColor2);
    }
}