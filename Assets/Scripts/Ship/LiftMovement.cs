using System.Collections;
using UnityEngine;

public class LiftMovement : MonoBehaviour
{
    public Transform pointA;
    public GameObject player;
    public GameObject transitScreen;
    void Start()
    {
        transitScreen.SetActive(false);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StartCoroutine(TransitScene());
            other.transform.position = pointA.position;
        }
    }
    IEnumerator TransitScene()
    {
        transitScreen.SetActive(true);
        GameManager.Instance.PauseGame();
        yield return new WaitForSecondsRealtime(2.0f);
        GameManager.Instance.ResumeGame();
        transitScreen.SetActive(false);
    }
}
