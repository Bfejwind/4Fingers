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
            //Grabbed link here
            InsertItem(obj);
        }
    }

    bool isItem(GameObject obj)
    {
        //Check that this gameobject has the Item script
        return obj.GetComponent<Item>();
    }
    void InsertItem(GameObject obj)
    {
        obj.GetComponent<Rigidbody>().isKinematic = true;
        obj.transform.SetParent(gameObject.transform,true);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localEulerAngles = obj.GetComponent<Item>().slotRotation;
        obj.GetComponent<Item>().inSlot = true;
        obj.GetComponent<Item>().currentSlot = this;
        itemInSlot = obj;
        slotImage.color = Color.white;
    }
    public void ResetColor()
    {
        slotImage.color = originalColor;
    }
    public void ItemRetrieved()
    {
        if(itemInSlot != null)
        {
            itemInSlot.transform.parent = null;
            itemInSlot.GetComponent<Item>().inSlot = false;
            ResetColor();
            itemInSlot.GetComponent<Item>().currentSlot = null;
            itemInSlot.GetComponent<Rigidbody>().isKinematic = false;
            itemInSlot = null;
        }
    }
}
