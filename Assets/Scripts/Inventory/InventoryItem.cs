using UnityEngine;

[DisallowMultipleComponent]
public class InventoryItem : MonoBehaviour
{
    [SerializeField] private ItemData itemData;

    public ItemData GetData()
    {
        if (itemData == null)
            Debug.LogError($"[InventoryItem] ItemData manquant sur {name}");
        return itemData;
    }

    public void SetData(ItemData data)
    {
        itemData = data;
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}
