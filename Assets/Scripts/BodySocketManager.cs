using UnityEngine;
[System.Serializable]
public class bodySocket
{
    public GameObject gameObject;
    [Range(0.01f,1f)]
    public float heightRatio;
}

public class BodySocketManager : MonoBehaviour
{
    public GameObject HMD;
    public bodySocket[] bodySockets;
    private Vector3 currentHMDPosition;
    private Quaternion currentHMDRotation;
    void Update()
    {
        currentHMDPosition = HMD.transform.position;
        currentHMDRotation = HMD.transform.rotation;
        foreach (var bodySocket in bodySockets)
        {
            UpdateBodySocketHeight(bodySocket);
        }
        UpdateSocketInventory();
    }
    private void UpdateBodySocketHeight(bodySocket bodySocket)
    {
        bodySocket.gameObject.transform.position = new Vector3(bodySocket.gameObject.transform.position.x, currentHMDPosition.y*bodySocket.heightRatio,bodySocket.gameObject.transform.position.z);
    }
    private void UpdateSocketInventory()
    {
        transform.position = new Vector3(currentHMDPosition.x,0,currentHMDPosition.z);
        transform.rotation = new Quaternion(transform.rotation.x,currentHMDRotation.y,transform.rotation.z,currentHMDRotation.w);
    }
}
