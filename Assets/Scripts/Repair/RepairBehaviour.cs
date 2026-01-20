using UnityEngine;

public class RepairBehaviour : MonoBehaviour
{
    [SerializeField]
    GameObject TestHPBar;
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wrench"))
        {
            TestHPBar.GetComponent<BatteryBehaviour>().TestRepair();
        }
    }
}
