using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Unity.Netcode;

public class RadialInventory : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image wheelUI;
    [SerializeField] private GameObject blurBackground;
    [SerializeField] private List<Image> slotHighlights;
    [SerializeField] private List<Image> itemIcons;

    [Header("Input")]
    [SerializeField] private InputActionReference inventoryInput;

    private PlayerInventory playerInventory;
    private PlayerController playerController;
    private int selectedSlotIndex = -1;

    private float targetAlpha = 0f;
    private float fadeSpeed = 10f;

    private void Start() => HideImmediate();

    public void Initialize(PlayerInventory inventory)
    {
        playerInventory = inventory;
        playerController = inventory.GetComponent<PlayerController>();

        inventoryInput.action.started += OnInventoryStarted;
        inventoryInput.action.canceled += OnInventoryCanceled;
        inventoryInput.action.Enable();

        HideImmediate();
    }

    private void OnDestroy()
    {
        if (inventoryInput?.action != null)
        {
            inventoryInput.action.started -= OnInventoryStarted;
            inventoryInput.action.canceled -= OnInventoryCanceled;
        }
    }

    private void OnInventoryStarted(InputAction.CallbackContext ctx)
    {
        Show();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        playerController?.SetInteracting(true);
    }

    private void OnInventoryCanceled(InputAction.CallbackContext ctx)
    {
        if (selectedSlotIndex != -1)
        {
            playerInventory.EquipItem(selectedSlotIndex);
            Debug.Log($"[RadialInventory] Selected slot {selectedSlotIndex}: {playerInventory.CurrentItem?.GetData().itemName}");
        }

        Hide();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerController?.SetInteracting(false);
    }

    private void Show()
    {
        targetAlpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        if (blurBackground != null) blurBackground.SetActive(true);
        RefreshIcons();
    }

    private void Hide()
    {
        targetAlpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        if (blurBackground != null) blurBackground.SetActive(false);
        selectedSlotIndex = -1;
    }

    private void HideImmediate()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        if (blurBackground != null) blurBackground.SetActive(false);
    }

    private void Update()
    {
        // Fade UI
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

        if (canvasGroup.alpha < 0.01f || wheelUI == null || playerInventory == null) return;

        Vector2 center = RectTransformUtility.WorldToScreenPoint(null, wheelUI.rectTransform.position);
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 direction = mousePos - center;

        if (direction.magnitude < 20f) return;

        // Inverser Y pour corriger
        direction.y = -direction.y;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        float angleOffset = 135f;
        angle = (angle + angleOffset) % 360f;

        int segment = Mathf.FloorToInt(angle / (360f / slotHighlights.Count));
        if (segment != selectedSlotIndex)
        {
            selectedSlotIndex = segment;
            Debug.Log($"[RadialInventory] Hover slot {selectedSlotIndex}");
        }

        UpdateSlotHighlights();
    }

    private void UpdateSlotHighlights()
    {
        for (int i = 0; i < slotHighlights.Count; i++)
        {
            float target = (i == selectedSlotIndex) ? 1f : 0f;
            Color c = slotHighlights[i].color;
            c.a = Mathf.Lerp(c.a, target, Time.deltaTime * 15f);
            slotHighlights[i].color = c;
        }
    }

    public void RefreshIcons()
    {
        List<InventoryItem> items = playerInventory.GetAllItems();

        for (int i = 0; i < itemIcons.Count; i++)
        {
            if (i < items.Count && items[i].GetData().icon != null)
            {
                itemIcons[i].sprite = items[i].GetData().icon;
                Color c = itemIcons[i].color;
                c.a = 1f;
                itemIcons[i].color = c;
            }
            else
            {
                itemIcons[i].sprite = null;
                Color c = itemIcons[i].color;
                c.a = 0f;
                itemIcons[i].color = c;
            }
        }
    }
}
