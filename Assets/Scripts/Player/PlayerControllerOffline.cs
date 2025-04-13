// PlayerControllerOffline.cs (adapted from networked version)
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerOffline : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Air Control")]
    public float airControlMultiplier = 0.4f;

    [Header("Camera & View")]
    public Transform viewDirectionPivot;
    public Transform cameraHolder;
    public Transform lookTarget;
    public Transform visualRoot;
    public GameObject headMesh;

    [Header("Mouse Look")]
    public float mouseSensitivity = 0.23f;
    public float maxLookAngle = 80f;

    [Header("Turn-In-Place")]
    public float turnThreshold = 60f;
    public float turnSpeed = 4f;

    [Header("Headbob & Camera Effects")]
    public float headBobSpeed = 10f;
    public float headBobAmount = 0.05f;
    public float bobRotationAmount = 0.3f;
    public float landingStrength = 0.5f;
    public float landingDuration = 0f;

    private CharacterController controller;
    private Animator animator;
    private InputSystem_Actions input;

    private Vector2 moveInput;
    private Vector3 velocity;
    private float verticalLookRotation;
    private float currentHeadYaw = 0f;
    private bool isJumping = false;
    private Vector3 originalCamLocalPos;

    private float bobTimer = 0f;
    private bool wasGroundedLastFrame = true;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        originalCamLocalPos = cameraHolder.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (headMesh != null)
            headMesh.SetActive(false);

        input = new InputSystem_Actions();
        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += ctx => moveInput = Vector2.zero;
        input.Player.Jump.performed += ctx => Jump();
        input.Enable();

        LobbyBoardInteractable board = FindObjectOfType<LobbyBoardInteractable>();
        if (board != null)
            board.OnLocalPlayerChanged(Camera.main.transform, this);
    }

    private void OnDisable()
    {
        DisableInput();
    }

    void Update()
    {
        HandleMovement();
        HandleLook();
        ApplyHeadBobbing();
        UpdateJumpStates();
    }

    void UpdateJumpStates()
    {
        bool isAboutToLand = !controller.isGrounded && velocity.y < 0 &&
            Physics.Raycast(transform.position, Vector3.down, out _, 1.2f);

        bool isGrounded = controller.isGrounded || isAboutToLand;
        animator.SetBool("isGrounded", isGrounded);

        if (isJumping && isGrounded)
        {
            isJumping = false;
            animator.SetBool("isJumping", false);
        }

        if (!wasGroundedLastFrame && isGrounded)
            StartCoroutine(LandingBounce());

        wasGroundedLastFrame = isGrounded;
    }

    private void HandleMovement()
    {
        float speed = input.Player.Sprint.IsPressed() ? sprintSpeed : walkSpeed;
        if (!controller.isGrounded)
            speed *= airControlMultiplier;
        animator.SetBool("isSprinting", input.Player.Sprint.IsPressed());

        Vector3 moveDir = viewDirectionPivot.right * moveInput.x + viewDirectionPivot.forward * moveInput.y;
        moveDir.y = 0f;
        controller.Move(moveDir.normalized * speed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        float inputMagnitude = Mathf.Clamp01(moveInput.magnitude);
        Vector2 inputDir = moveInput.normalized * inputMagnitude;
        animator.SetFloat("MoveX", inputDir.x, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveY", inputDir.y, 0.1f, Time.deltaTime);
    }

    private void HandleLook()
    {
        Vector2 lookDelta = input.Player.Look.ReadValue<Vector2>();
        float mouseX = lookDelta.x * mouseSensitivity;
        float mouseY = lookDelta.y * mouseSensitivity;

        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -maxLookAngle, maxLookAngle);
        cameraHolder.localRotation = Quaternion.Euler(verticalLookRotation, 0f, 0f);

        currentHeadYaw += mouseX * mouseSensitivity;
        currentHeadYaw = Mathf.Clamp(currentHeadYaw, -80f, 80f);
        viewDirectionPivot.localRotation = Quaternion.Euler(0f, currentHeadYaw, 0f);

        bool isMoving = moveInput.magnitude > 0.1f;
        if (isMoving)
        {
            float targetYaw = transform.eulerAngles.y + viewDirectionPivot.localEulerAngles.y;
            float newYaw = Mathf.LerpAngle(transform.eulerAngles.y, targetYaw, Time.deltaTime * turnSpeed * 5f);

            float bodyTurnDelta = Mathf.DeltaAngle(transform.eulerAngles.y, newYaw);
            transform.rotation = Quaternion.Euler(0f, newYaw, 0f);

            currentHeadYaw -= bodyTurnDelta;
            viewDirectionPivot.localRotation = Quaternion.Euler(0f, currentHeadYaw, 0f);
            visualRoot.rotation = transform.rotation;
        }
        else if (Mathf.Abs(currentHeadYaw) > turnThreshold)
        {
            float turnDirection = Mathf.Sign(currentHeadYaw);
            float bodyTurnAmount = turnDirection * turnSpeed * Time.deltaTime * 100f;

            transform.Rotate(Vector3.up * bodyTurnAmount);
            currentHeadYaw -= bodyTurnAmount;
        }

        if (lookTarget != null)
        {
            Vector3 forwardLook = cameraHolder.position + cameraHolder.forward * 10f;
            lookTarget.position = forwardLook;
        }
    }

    private void ApplyHeadBobbing()
    {
        if (controller.isGrounded && moveInput.magnitude > 0.1f)
        {
            float bobOffset = Mathf.Sin(bobTimer) * headBobAmount;
            float tilt = Mathf.Sin(bobTimer * 0.5f) * bobRotationAmount;

            if (input.Player.Sprint.IsPressed())
            {
                bobOffset *= 1.5f;
                tilt *= 1.5f;
                bobTimer += Time.deltaTime * headBobSpeed * 1.5f;
            }
            else
            {
                bobTimer += Time.deltaTime * headBobSpeed;
            }

            cameraHolder.localPosition = originalCamLocalPos + new Vector3(0f, bobOffset, 0f);
            cameraHolder.localRotation = Quaternion.Euler(verticalLookRotation, 0f, tilt);
        }
        else
        {
            bobTimer = 0f;
            cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, originalCamLocalPos, Time.deltaTime * 5f);
            cameraHolder.localRotation = Quaternion.Lerp(cameraHolder.localRotation, Quaternion.Euler(verticalLookRotation, 0f, 0f), Time.deltaTime * 5f);
        }
    }

    private IEnumerator LandingBounce()
    {
        float elapsed = 0f;
        while (elapsed < landingDuration)
        {
            float t = elapsed / landingDuration;
            float offset = Mathf.Sin(Mathf.PI * t) * -landingStrength;
            cameraHolder.localPosition = originalCamLocalPos + new Vector3(0, offset, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cameraHolder.localPosition = originalCamLocalPos;
    }

    private void Jump()
    {
        if (controller.isGrounded && !isJumping)
        {
            isJumping = true;
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            animator.SetBool("isJumping", true);
        }
    }

    public void SetVisualsVisible(bool visible)
    {
        if (visualRoot != null)
            visualRoot.gameObject.SetActive(visible);
    }

    public void EnableInput()
    {
        input?.Enable();
    }

    public void DisableInput()
    {
        input?.Disable();
    }
}