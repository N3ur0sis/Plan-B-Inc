using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class LobbyBoardInteractable : MonoBehaviour
{
    [Header("UI & Prompt")]
    public Canvas uiCanvas;
    public GameObject interactPrompt;         // Empty parent object
    public Transform promptTextChild;         // Assign the TMP/Text child here in Inspector!

    [Header("Camera Control")]
    public Transform cameraTargetPosition;    // Where to move the camera
    public Transform lookAtTarget;            // What the camera looks at
    public float cameraMoveSpeed = 4f;

    private bool playerNearby = false;
    private bool interacting = false;
    private Transform playerCamera;
    private PlayerController playerController;

    private Vector3 originalCamPos;
    private Quaternion originalCamRot;


    void Start()
    {
        uiCanvas.gameObject.SetActive(false);
        interactPrompt.SetActive(false);
    }

    void Update()
    {
        // Rotate the prompt text to face camera if it's present
        if (playerCamera != null && promptTextChild != null)
        {
            Vector3 dirToCam = (playerCamera.position - promptTextChild.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(-dirToCam);
            promptTextChild.rotation = Quaternion.Slerp(promptTextChild.rotation, lookRot, Time.deltaTime * 8f);
        }

        if (playerNearby && !interacting && Keyboard.current.eKey.wasPressedThisFrame)
        {
            EnterBoardView();
        }

        if (interacting && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ExitBoardView();
        }
    }

    void EnterBoardView()
    {
        interacting = true;

        playerCamera = Camera.main.transform;
        playerController = playerCamera.GetComponentInParent<PlayerController>();

        // Save original camera pos/rot
        originalCamPos = playerCamera.position;
        originalCamRot = playerCamera.rotation;

        // Disable player movement
        if (playerController != null)
        {
            playerController.DisableInput();
            playerController.SetVisualsVisible(false);
        }

        interactPrompt.SetActive(false);
        uiCanvas.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(MoveCameraToTarget(
            cameraTargetPosition.position,
            Quaternion.LookRotation(lookAtTarget.position - cameraTargetPosition.position),
            null
        ));
    }

    void ExitBoardView()
    {
        interacting = false;

        uiCanvas.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine(MoveCameraToTarget(
            originalCamPos,
            originalCamRot,
            () =>
            {
                if (playerController != null)
                {
                    playerController.SetVisualsVisible(true);
                    playerController.EnableInput();
                }
            }
        ));
    }

    IEnumerator MoveCameraToTarget(Vector3 targetPos, Quaternion targetRot, System.Action onComplete)
    {
        float t = 0f;
        Vector3 startPos = playerCamera.position;
        Quaternion startRot = playerCamera.rotation;

        while (t < 1f)
        {
            t += Time.deltaTime * cameraMoveSpeed;
            playerCamera.position = Vector3.Lerp(startPos, targetPos, t);
            playerCamera.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        onComplete?.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            interactPrompt.SetActive(true);

            if (playerCamera == null)
                playerCamera = Camera.main.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (!interacting)
            {
                interactPrompt.SetActive(false);
                uiCanvas.gameObject.SetActive(false);
            }
        }
    }
}
