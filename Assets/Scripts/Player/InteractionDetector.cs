using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactionMask;

    private InputSystem_Actions inputActions;
    private IInteractable currentTarget;
    private Camera mainCamera;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Player.Interact.performed += OnInteract;
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Interact.performed -= OnInteract;
        inputActions.Disable();
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {

        // Update camera reference if needed
        if (Camera.main != null && Camera.main != mainCamera)
            mainCamera = Camera.main;
        DetectInteraction();
    }

    private void DetectInteraction()
    {
        if (mainCamera == null) return;

        Ray ray = new(mainCamera.transform.position, mainCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionMask))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                if (currentTarget != interactable)
                {
                    if (currentTarget is InteractableObject previous)
                        previous.HidePrompt();

                    currentTarget = interactable;

                    if (currentTarget is InteractableObject current)
                        current.ShowPrompt();
                }

                return;
            }
        }

        // Aucun interactable valide détecté
        if (currentTarget is InteractableObject fallback)
        {
            fallback.HidePrompt();
            currentTarget = null;
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        currentTarget?.InteractWith(gameObject);
    }
}
