using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class LobbyBoardInteractable : MonoBehaviour
{
    [Header("UI & Prompt")]
    public Canvas uiCanvas;

    [Header("Camera Control")]
    public Transform cameraTargetPosition;
    public Transform lookAtTarget;
    public float cameraMoveSpeed = 4f;

    [Header("Input Action (Cancel / Escape)")]
    [SerializeField] private InputActionReference cancelAction;

    private bool interacting = false;
    private Transform playerCamera;

    private PlayerController playerController;
    private PlayerControllerOffline playerControllerOffline;

    private Vector3 originalCamPos;
    private Quaternion originalCamRot;

    private void OnEnable()
    {
        if (cancelAction != null)
            cancelAction.action.performed += OnCancelPerformed;
    }

    private void OnDisable()
    {
        if (cancelAction != null)
            cancelAction.action.performed -= OnCancelPerformed;
    }

    private void Start()
    {
        uiCanvas.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Auto rebind player reference if camera changes
        if (Camera.main != null && Camera.main.transform != playerCamera)
        {
            playerController = FindAnyObjectByType<PlayerController>();
            playerControllerOffline = FindAnyObjectByType<PlayerControllerOffline>();

            if (playerController != null)
                playerCamera = playerController.cameraHolder;
            else if (playerControllerOffline != null)
                playerCamera = playerControllerOffline.cameraHolder;

            if (interacting && playerController == null && playerControllerOffline == null)
            {
                Debug.LogWarning("[LOBBY BOARD] Player was replaced during interaction. Forcing board exit.");
                ExitBoardView(force: true);
            }
        }
    }

    private void OnCancelPerformed(InputAction.CallbackContext ctx)
    {
        if (interacting)
        {
            ExitBoardView();
        }
    }

    public void EnterBoardView()
    {
        interacting = true;
        playerCamera = Camera.main.transform;

        playerController = playerCamera.GetComponentInParent<PlayerController>();
        playerControllerOffline = playerCamera.GetComponentInParent<PlayerControllerOffline>();

        originalCamPos = playerCamera.localPosition;
        originalCamRot = playerCamera.localRotation;

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

        uiCanvas.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Vector3 localTargetPos = playerCamera.parent.InverseTransformPoint(cameraTargetPosition.position);
        Quaternion localTargetRot = Quaternion.Inverse(playerCamera.parent.rotation) * Quaternion.LookRotation(lookAtTarget.position - cameraTargetPosition.position);

        StartCoroutine(MoveCameraToTarget(localTargetPos, localTargetRot, null));
    }

    public void ExitBoardView(bool force = false)
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
        Vector3 startPos = playerCamera.localPosition;
        Quaternion startRot = playerCamera.localRotation;

        while (t < 1f)
        {
            t += Time.deltaTime * cameraMoveSpeed;
            playerCamera.localPosition = Vector3.Lerp(startPos, targetPos, t);
            playerCamera.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        onComplete?.Invoke();
    }

    public void OnLocalPlayerChanged(Transform newCamera, MonoBehaviour newPlayer)
    {
        if (interacting)
        {
            Debug.Log("[LOBBY BOARD] Detected player switch while board was open. Forcing proper cleanup.");
            ExitBoardView(force: true);
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