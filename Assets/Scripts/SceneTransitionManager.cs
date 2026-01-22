using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Manages scene transitions with fade effects.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{   
    public FadeScreen fadeScreen; // Reference to the fade screen component

    /// <summary>
    /// Transitions to a new scene with a fade-out effect.
    /// </summary>
    /// <param name="sceneIndex">Index of the scene to load</param>
    public void GoToSceneAsync(int sceneIndex)
    {
        StartCoroutine(GoToSceneAsyncRoutine(sceneIndex));
    }

    /// <summary>
    /// Coroutine that handles the scene transition with fade timing.
    /// Fades out, loads the new scene asynchronously, then activates it after the fade completes.
    /// </summary>
    /// <param name="sceneIndex">Index of the scene to load</param>
    /// <returns>IEnumerator for coroutine execution</returns>
    IEnumerator GoToSceneAsyncRoutine(int sceneIndex)
    {
        fadeScreen.FadeOut();
        //Launch the new scene
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);
        operation.allowSceneActivation = false;

        float timer = 0f;

        // Wait for the fade duration before activating the scene
        while (timer <= fadeScreen.fadeDuration && !operation.isDone)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        operation.allowSceneActivation = true;
    }
}