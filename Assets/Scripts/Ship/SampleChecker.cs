using UnityEngine;

public class SampleChecker : MonoBehaviour
{
    private string correctSample;
    private string inputSample;
    private GameObject inputSampleObject;
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
            return;
        }
        else
        {
            Destroy(inputSampleObject);
        }
    }
}
