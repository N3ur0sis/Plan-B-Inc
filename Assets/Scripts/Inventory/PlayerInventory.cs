// --- PlayerInventory.cs ---
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerInventory : NetworkBehaviour
{
    [SerializeField] private int maxSlots = 8;
    [SerializeField] private GameObject radialInventoryPrefab;

    private readonly List<InventoryItem> items = new();
    private int currentSlotIndex = -1;

    private RadialInventory radialInventoryUI;

    public bool IsFull => items.Count >= maxSlots;
    public InventoryItem CurrentItem => (currentSlotIndex >= 0 && currentSlotIndex < items.Count) ? items[currentSlotIndex] : null;
    public List<InventoryItem> GetAllItems() => new(items);

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        if (radialInventoryPrefab != null)
        {
            GameObject instance = Instantiate(radialInventoryPrefab);
            instance.transform.SetParent(GameObject.Find("Canvas").transform, false);
            radialInventoryUI = instance.GetComponent<RadialInventory>();
            radialInventoryUI.Initialize(this);
        }
        else
        {
            Debug.LogError("[PlayerInventory] Radial inventory prefab not assigned.");
        }
    }

    public bool AddItem(InventoryItem item)
    {
        if (IsFull)
        {
            Debug.LogWarning("[Inventory] Inventory is full.");
            return false;
        }

        items.Add(item);
        item.transform.SetParent(transform);
        item.transform.localPosition = Vector3.zero;
        item.gameObject.SetActive(false);

        Debug.Log("[Inventory] Added item: " + item.GetData().itemName);

        if (currentSlotIndex == -1)
        {
            currentSlotIndex = 0;
            EquipItem(currentSlotIndex);
        }

        radialInventoryUI.RefreshIcons();


        return true;
    }

    public void EquipItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= items.Count) return;

        currentSlotIndex = slotIndex;

        for (int i = 0; i < items.Count; i++)
        {
            items[i].SetActive(i == currentSlotIndex);
        }

        Debug.Log("[Inventory] Equipped item in slot " + slotIndex + ": " + CurrentItem?.GetData().itemName);
    }
}
