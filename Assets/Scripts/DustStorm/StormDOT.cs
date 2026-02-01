using System.Collections;
using UnityEngine;

public class StormDOT : MonoBehaviour
{
    public float damage;
    public float dotInterval;
    public bool playerInZone;
    private Coroutine dotInEffect;
    void Start()
    {
        playerInZone = false;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerInZone = true;
            if (dotInEffect == null)
            {
                dotInEffect = StartCoroutine(DOTHandler());
            }
        }
    }
    IEnumerator DOTHandler()
    {
        while (playerInZone)
        {
            yield return new WaitForSeconds(dotInterval);
            GameManager.Instance.TakeDamage(damage);
        }
    }
}
