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
    private XRGrabInteractable grab;
    void Start()
    {
        slotImage = GetComponentInChildren<Image>();
        originalColor = slotImage.color;
        grab = GetComponent<XRGrabInteractable>();
        
    }
    void OnEnable()
    {
        if (grab != null)
        {
            grab.selectEntered.AddListener(OnGrabbed);
        }
    }

    void OnDisable()
    {
        if (grab != null)
        {
            grab.selectEntered.RemoveListener(OnGrabbed);
        }
    }
    void OnGrabbed(SelectEnterEventArgs args)
    {
        ItemRetrieved();
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
            Instantiate(itemInSlot,currentSlotPosition.position,Quaternion.identity);
            gameObject.GetComponentInParent<Slot>().itemInSlot = null;
            gameObject.transform.parent = null;
            gameObject.GetComponent<Item>().inSlot = false;
            ResetColor();
            gameObject.GetComponent<Item>().currentSlot = null;
            
        }

    }
}
