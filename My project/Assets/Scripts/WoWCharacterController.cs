using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// WoW-style character controller
/// - Moves relative to camera direction
/// - Character rotates to face movement direction
/// - Works with WoWCameraController
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class WoWCharacterController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float sprintSpeed = 12f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = 20f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private bool rotateWithRightClick = true;

    [Header("References")]
    [SerializeField] private WoWCameraController cameraController;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    // Input
    private Vector2 moveInput;
    private bool sprintInput;
    private bool jumpInput;
    private bool autoRunInput;
    private bool isAutoRunning = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Find camera controller if not set
        if (cameraController == null)
        {
            cameraController = Camera.main?.GetComponent<WoWCameraController>();
            if (cameraController == null)
            {
                cameraController = FindFirstObjectByType<WoWCameraController>();
            }
        }
    }

    void Update()
    {
        ReadInput();
        HandleMovement();
        HandleJump();
        ApplyGravity();
    }

    void ReadInput()
    {
        if (Keyboard.current == null) return;

        // Movement input
        moveInput = Vector2.zero;
        if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
        if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
        if (Keyboard.current.dKey.isPressed) moveInput.x += 1;
        if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;

        // Sprint (shift)
        sprintInput = Keyboard.current.leftShiftKey.isPressed;

        // Jump
        jumpInput = Keyboard.current.spaceKey.wasPressedThisFrame;

        // Auto-run toggle (Num Lock or R)
        if (Keyboard.current.numLockKey.wasPressedThisFrame ||
            Keyboard.current.rKey.wasPressedThisFrame)
        {
            isAutoRunning = !isAutoRunning;
        }

        // Cancel auto-run on backward movement
        if (moveInput.y < 0)
        {
            isAutoRunning = false;
        }

        // Both mouse buttons = move forward (WoW style)
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.isPressed && Mouse.current.rightButton.isPressed)
            {
                moveInput.y = 1;
            }
        }

        // Apply auto-run
        if (isAutoRunning && moveInput.y >= 0)
        {
            moveInput.y = 1;
        }
    }

    void HandleMovement()
    {
        if (cameraController == null) return;

        // Get camera-relative directions
        Vector3 cameraForward = cameraController.GetFlatForward();
        Vector3 cameraRight = cameraController.GetFlatRight();

        // Calculate movement direction relative to camera
        Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x).normalized;

        // Rotate character to face movement direction
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Rotate with camera when right-clicking (WoW style)
        if (rotateWithRightClick && cameraController.IsOrbiting && moveInput.magnitude < 0.1f)
        {
            transform.rotation = Quaternion.Euler(0, cameraController.HorizontalAngle, 0);
        }

        // Calculate speed
        float speed = walkSpeed;
        if (moveInput.y > 0) // Only run/sprint when moving forward
        {
            speed = sprintInput ? sprintSpeed : runSpeed;
        }
        else if (moveInput.y < 0) // Slower when moving backward
        {
            speed = walkSpeed * 0.5f;
        }

        // Apply movement
        Vector3 move = moveDirection * speed;
        velocity.x = move.x;
        velocity.z = move.z;

        controller.Move(velocity * Time.deltaTime);
    }

    void HandleJump()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && jumpInput)
        {
            velocity.y = jumpForce;
        }
    }

    void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime;
        }
    }

    public bool IsAutoRunning => isAutoRunning;
}
