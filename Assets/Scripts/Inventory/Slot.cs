using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class Slot : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;

    void Awake()
    {
        socket = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
    }

    void OnEnable()
    {
        socket.selectEntered.AddListener(OnItemPlaced);
    }

    void OnDisable()
    {
        socket.selectEntered.RemoveListener(OnItemPlaced);
    }

    void OnItemPlaced(SelectEnterEventArgs args)
    {
        Item item = args.interactableObject.transform
            .GetComponent<Item>();

        if (item != null)
        {
            InventoryManager.Instance.AddItem(item);
        }
    }
}
