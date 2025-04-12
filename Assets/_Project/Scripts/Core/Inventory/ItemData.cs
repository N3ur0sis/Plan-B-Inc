using UnityEngine;

[CreateAssetMenu(fileName = "NewInventoryItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName = "New Item";
    public Sprite icon;
    public GameObject prefab;

    [Header("Stacking")]
    public bool isStackable = false;
    [Min(1)] public int maxStack = 1;
}
