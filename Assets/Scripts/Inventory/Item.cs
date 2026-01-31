using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Item : MonoBehaviour
{
    public bool inSlot;
    public Vector3 slotRotation = Vector3.zero;
    public Slot currentSlot;
    public bool grabbed = false;
    private XRGrabInteractable grab;
    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
    }
    void OnEnable()
    {
        grab.selectEntered.AddListener(OnGrabbed);
        grab.selectExited.AddListener(OnReleased);
    }

    void OnDisable()
    {
        grab.selectEntered.RemoveListener(OnGrabbed);
        grab.selectExited.RemoveListener(OnReleased);
    }
    void OnGrabbed(SelectEnterEventArgs args)
    {
        grabbed = true;
    }
    void OnReleased(SelectExitEventArgs args)
    {
        grabbed = false;
    }
}
