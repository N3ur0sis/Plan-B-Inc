using UnityEngine;
using Unity.Netcode;
using TMPro;

public enum InteractionPromptType
{
    WorldOnly,
    UIOnly,
    Both
}

public class InteractableObject : NetworkBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] private string interactionPrompt = "Press E to interact";
    [SerializeField] private InteractionPromptType promptType = InteractionPromptType.WorldOnly;

    [Header("World-Space Prompt")]
    [SerializeField] private GameObject worldUiPrefab;
    [SerializeField] private Vector3 worldUiOffset = new Vector3(0f, 0.5f, 0f);

    [Header("Outline")]
    [SerializeField] private OutlineHighlighter outline;

    private GameObject worldUiInstance;
    private TextMeshProUGUI worldPromptText;

    private ScreenInteractionUI screenPrompt;
    private IInteractionAction[] actions;

    private void Awake()
    {
        actions = GetComponents<IInteractionAction>();
        outline = GetComponent<OutlineHighlighter>();
    }

    private void Start()
    {
        if (worldUiPrefab != null)
        {
            worldUiInstance = Instantiate(worldUiPrefab);
            worldUiInstance.transform.position = transform.position + worldUiOffset;
            worldUiInstance.transform.SetParent(null);
            worldPromptText = worldUiInstance.GetComponentInChildren<TextMeshProUGUI>();
            worldUiInstance.SetActive(false);
        }

        if (promptType == InteractionPromptType.UIOnly || promptType == InteractionPromptType.Both)
        {
            screenPrompt = FindFirstObjectByType<ScreenInteractionUI>();
            screenPrompt?.ClearPrompt();
        }
    }

    public string GetInteractionPrompt() => interactionPrompt;

    public void Interact() => InteractWith(null);

    public void InteractWith(GameObject interactor)
    {
        foreach (var action in actions)
        {
            action.Execute(interactor);
        }

        HidePrompt();
    }

    public void ShowPrompt()
    {
        if ((promptType == InteractionPromptType.WorldOnly || promptType == InteractionPromptType.Both) && worldUiInstance != null)
        {
            if (worldPromptText != null)
                worldPromptText.text = interactionPrompt;

            if (worldUiInstance.TryGetComponent(out Transform _))
                worldUiInstance.SetActive(true);
        }

        if ((promptType == InteractionPromptType.UIOnly || promptType == InteractionPromptType.Both) && screenPrompt != null)
        {
            screenPrompt.SetPrompt(interactionPrompt);
        }

        outline?.EnableOutline();
    }

    public void HidePrompt()
    {
        if (worldUiInstance != null && worldUiInstance.activeSelf)
        {
            try { worldUiInstance.SetActive(false); }
            catch (MissingReferenceException) { }
        }

        screenPrompt?.ClearPrompt();
        outline?.DisableOutline();
    }

    private void OnDisable()
    {
        HidePrompt();
    }
}
