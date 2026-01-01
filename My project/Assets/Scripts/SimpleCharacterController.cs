using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple First-Person Character Controller for graybox testing
/// Uses new Input System
/// WASD + Mouse look + Shift to sprint
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class SimpleCharacterController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = 20f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private float verticalRotation = 0f;
    private bool isGrounded;

    // Input
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool sprintInput;
    private bool jumpInput;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Find or create camera
        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cameraTransform = cam.transform;
            }
            else
            {
                GameObject camObj = new GameObject("PlayerCamera");
                camObj.transform.parent = transform;
                camObj.transform.localPosition = new Vector3(0, 0.8f, 0);
                camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
                cameraTransform = camObj.transform;
            }
        }

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        ReadInput();
        HandleLook();
        HandleMovement();
        HandleJump();
        ApplyGravity();

        // Unlock cursor with Escape
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
                ? CursorLockMode.None
                : CursorLockMode.Locked;
            Cursor.visible = Cursor.lockState != CursorLockMode.Locked;
        }
    }

    void ReadInput()
    {
        // Keyboard input
        if (Keyboard.current != null)
        {
            moveInput = Vector2.zero;
            if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
            if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
            if (Keyboard.current.dKey.isPressed) moveInput.x += 1;
            if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;

            sprintInput = Keyboard.current.leftShiftKey.isPressed;
            jumpInput = Keyboard.current.spaceKey.wasPressedThisFrame;
        }

        // Mouse input
        if (Mouse.current != null)
        {
            lookInput = Mouse.current.delta.ReadValue();
        }
    }

    void HandleLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        // Horizontal rotation (player body)
        transform.Rotate(Vector3.up * mouseX);

        // Vertical rotation (camera only)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    void HandleMovement()
    {
        Vector3 direction = transform.right * moveInput.x + transform.forward * moveInput.y;
        direction = direction.normalized;

        float speed = sprintInput ? sprintSpeed : walkSpeed;
        Vector3 move = direction * speed;

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

    #if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/1620/Create Player Controller")]
    static void CreatePlayerController()
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0, 1, 0);

        DestroyImmediate(player.GetComponent<CapsuleCollider>());

        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.5f;
        cc.center = Vector3.zero;

        player.AddComponent<SimpleCharacterController>();

        GameObject camObj = new GameObject("PlayerCamera");
        camObj.transform.parent = player.transform;
        camObj.transform.localPosition = new Vector3(0, 0.8f, 0);
        Camera cam = camObj.AddComponent<Camera>();
        cam.nearClipPlane = 0.1f;
        camObj.AddComponent<AudioListener>();

        UnityEditor.Selection.activeGameObject = player;
        Debug.Log("Player controller created!");
    }
    #endif
}
