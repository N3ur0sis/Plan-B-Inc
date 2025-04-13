using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractionAction
{
    public void Execute(GameObject interactor)
    {
        if (interactor == null) return;

        var inventory = interactor.GetComponent<PlayerInventory>();
        var inventoryItem = GetComponent<InventoryItem>();

        if (inventory == null)
        {
            Debug.LogWarning("[PickupItem] PlayerInventory manquant sur l'interacteur.");
            return;
        }

        if (inventoryItem == null)
        {
            Debug.LogWarning("[PickupItem] InventoryItem manquant sur l'objet ramassable.");
            return;
        }

        var itemData = inventoryItem.GetData();
        if (itemData == null)
        {
            Debug.LogWarning("[PickupItem] Aucun ItemData assigné. Impossible de ramasser.");
            return;
        }

        bool added = inventory.AddItem(inventoryItem);

        if (added)
        {
            Debug.Log($"[{itemData.itemName}] picked up by {interactor.name}");
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Inventory plein. Objet non ramassé.");
        }
    }
}
