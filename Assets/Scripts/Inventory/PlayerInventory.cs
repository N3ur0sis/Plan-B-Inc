using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private int maxSlots = 8;

    private readonly List<InventoryItem> items = new();
    private int currentSlotIndex = -1;

    public bool IsFull => items.Count >= maxSlots;
    public InventoryItem CurrentItem => (currentSlotIndex >= 0 && currentSlotIndex < items.Count) ? items[currentSlotIndex] : null;

    public bool AddItem(InventoryItem item)
    {
        if (IsFull)
        {
            Debug.LogWarning("Inventory is full.");
            return false;
        }

        items.Add(item);
        Debug.Log($"[Inventory] Added item: {item.GetData().itemName}");

        if (currentSlotIndex == -1)
        {
            currentSlotIndex = 0;
            EquipItem(currentSlotIndex);
        }

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

        Debug.Log($"[Inventory] Equipped item in slot {slotIndex}: {CurrentItem?.GetData().itemName}");
    }

    public void DropCurrentItem()
    {
        if (CurrentItem == null) return;

        InventoryItem itemToDrop = CurrentItem;
        items.RemoveAt(currentSlotIndex);

        // Respawn dans le monde
        Instantiate(itemToDrop.GetData().prefab, transform.position + transform.forward, Quaternion.identity);
        Destroy(itemToDrop.gameObject);

        currentSlotIndex = items.Count > 0 ? 0 : -1;

        if (currentSlotIndex != -1)
            EquipItem(currentSlotIndex);

        Debug.Log("[Inventory] Dropped item.");
    }

    public List<InventoryItem> GetAllItems() => new(items);
}
