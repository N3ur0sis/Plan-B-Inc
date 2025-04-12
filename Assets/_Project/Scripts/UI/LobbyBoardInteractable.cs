using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class LobbyBoardInteractable : MonoBehaviour
{
    [Header("UI & Prompt")]
    public Canvas uiCanvas;
    public GameObject interactPrompt;
    public Transform promptTextChild;

    [Header("Camera Control")]
    public Transform cameraTargetPosition;
    public Transform lookAtTarget;
    public float cameraMoveSpeed = 4f;

    private bool playerNearby = false;
    private bool interacting = false;
    private Transform playerCamera;

    private PlayerController playerController;
    private PlayerControllerOffline playerControllerOffline;

    private Vector3 originalCamPos;
    private Quaternion originalCamRot;

    void Start()
    {
        uiCanvas.gameObject.SetActive(false);
        interactPrompt.SetActive(false);
    }

    void Update()
    {
        // Detect and rebind if player/camera changed
        if (Camera.main != null && Camera.main.transform != playerCamera)
        {
            playerCamera = Camera.main.transform;
            playerController = playerCamera.GetComponentInParent<PlayerController>();
            playerControllerOffline = playerCamera.GetComponentInParent<PlayerControllerOffline>();

            // If interaction was active, but controller is now gone — force exit
            if (interacting && playerController == null && playerControllerOffline == null)
            {
                Debug.LogWarning("[LOBBY BOARD] Player was replaced during interaction. Forcing board exit.");
                ExitBoardView(force: true);
            }
        }

        // Keep prompt facing camera
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
        playerControllerOffline = playerCamera.GetComponentInParent<PlayerControllerOffline>();

        originalCamPos = playerCamera.position;
        originalCamRot = playerCamera.rotation;

        // Disable input & visuals
        if (playerController != null)
        {
            playerController.DisableInput();
            playerController.SetVisualsVisible(false);
        }
        else if (playerControllerOffline != null)
        {
            playerControllerOffline.DisableInput();
            playerControllerOffline.SetVisualsVisible(false);
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

    void ExitBoardView(bool force = false)
    {
        interacting = false;
        uiCanvas.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (force || playerCamera == null)
            return;

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
                else if (playerControllerOffline != null)
                {
                    playerControllerOffline.SetVisualsVisible(true);
                    playerControllerOffline.EnableInput();
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
        if (other.CompareTag("Player") || other.CompareTag("OfflinePlayer"))
        {
            playerNearby = true;
            interactPrompt.SetActive(true);

            if (playerCamera == null)
                playerCamera = Camera.main.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("OfflinePlayer"))
        {
            playerNearby = false;
            if (!interacting)
            {
                interactPrompt.SetActive(false);
                uiCanvas.gameObject.SetActive(false);
            }
        }
    }

    public void OnLocalPlayerChanged(Transform newCamera, MonoBehaviour newPlayer)
    {
        if (interacting)
        {
            Debug.Log("[LOBBY BOARD] Detected player switch while board was open. Forcing proper cleanup.");

            ExitBoardView(force: true); // forcibly clean up UI and states
        }

        playerCamera = newCamera;

        if (newPlayer is PlayerController pc)
            playerController = pc;
        else
            playerController = null;

        if (newPlayer is PlayerControllerOffline pco)
            playerControllerOffline = pco;
        else
            playerControllerOffline = null;
    }
}
