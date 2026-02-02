using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Slot : MonoBehaviour
{
    public GameObject itemInSlot;
    public Image slotImage;
    Color originalColor;
    public Transform currentSlotPosition;
    private Vector3 originalScale;
    public Vector3 targetScale;
    void Start()
    {
        slotImage = GetComponentInChildren<Image>();
        originalColor = slotImage.color;
    }

    void OnTriggerStay(Collider other)
    {
        if (itemInSlot !=null) return;
        GameObject obj = other.gameObject;
        if (!isItem(obj)) return;
        else if (!obj.GetComponent<Item>().grabbed)
        {
            originalScale = obj.transform.lossyScale;
            //Grabbed link here
            InsertItem(obj);
        }
    }

    bool isItem(GameObject obj)
    {
        //Check that this gameobject has the Item script
        return obj.GetComponent<Item>();
    }
    async void InsertItem(GameObject obj)
    {
        obj.GetComponent<Rigidbody>().isKinematic = true;
        obj.transform.SetParent(gameObject.transform,false);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = targetScale;
        obj.transform.localEulerAngles = obj.GetComponent<Item>().slotRotation;
        obj.GetComponent<Item>().inSlot = true;
        obj.GetComponent<Item>().currentSlot = this;
        itemInSlot = obj;
        slotImage.color = Color.white;
        string currentTag = obj.tag;   
        Debug.Log($"[Slot] Detected Item Tag: '{currentTag}'");
        if (!string.IsNullOrEmpty(currentTag))
        {
            await DatabaseManager.Instance.AddInventoryItem(currentTag);
            Debug.Log("Firebase updated for: " + currentTag);
        }
        else 
        {
            Debug.LogError("[Slot] Item has NO TAG assigned in Inspector!");
        }
    }
    public void ResetColor()
    {
        slotImage.color = originalColor;
    }
    public void ItemRetrieved()
    {
        if(itemInSlot != null)
        {
            itemInSlot.transform.SetParent(null,true);
            itemInSlot.transform.localScale = originalScale;
            itemInSlot.GetComponent<Item>().inSlot = false;
            ResetColor();
            itemInSlot.GetComponent<Item>().currentSlot = null;
            itemInSlot.GetComponent<Rigidbody>().isKinematic = false;
            itemInSlot = null;
        }
    }
}
