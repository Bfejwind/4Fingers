using UnityEngine;

public class SampleChecker : MonoBehaviour
{
    private string correctSample;
    private string inputSample;
    private GameObject inputSampleObject;
    public ParticleSystem correct;
    public ParticleSystem wrong;
    public AudioSource answerSFX;
    public AudioClip correctSFX;
    public AudioClip wrongSFX;
    void Awake()
    {
        correctSample = gameObject.name.Split("_")[1];
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("_"))
        {
            inputSample = other.gameObject.name.Split("_")[0];
            inputSampleObject = other.gameObject;
        }
    }
    public void CheckSample()
    {
        if (inputSample == correctSample)
        {
            correct.Play();
            answerSFX.PlayOneShot(correctSFX);
            GameManager.Instance.SampleChecker();
        }
        else
        {
            Destroy(inputSampleObject);
            wrong.Play();
            answerSFX.PlayOneShot(wrongSFX);
        }
    }
}
