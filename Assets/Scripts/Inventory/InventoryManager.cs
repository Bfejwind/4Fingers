using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public Transform slotSpawn;

    public List<Item> items = new();

    void Awake()
    {
        Instance = this;
    }

    public void AddItem(Item item)
    {
        items.Add(item);
        item.gameObject.SetActive(false);
    }

    public void RemoveItem(Item item, Transform spawnPoint)
{
    if (!items.Contains(item)) return;

    items.Remove(item);
    item.transform.position = spawnPoint.position;
    item.transform.rotation = spawnPoint.rotation;
    item.gameObject.SetActive(true);
}

}
