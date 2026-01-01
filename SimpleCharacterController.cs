using UnityEngine;

/// <summary>
/// Simple First-Person Character Controller for graybox testing
/// Attach to a capsule with a camera as child
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
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private float verticalRotation = 0f;
    private bool isGrounded;

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
                // Create camera
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
        HandleLook();
        HandleMovement();
        HandleJump();
        ApplyGravity();

        // Unlock cursor with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
                ? CursorLockMode.None
                : CursorLockMode.Locked;
            Cursor.visible = Cursor.lockState != CursorLockMode.Locked;
        }
    }

    void HandleLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Horizontal rotation (player body)
        transform.Rotate(Vector3.up * mouseX);

        // Vertical rotation (camera only)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = transform.right * horizontal + transform.forward * vertical;
        direction = direction.normalized;

        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
        Vector3 move = direction * speed;

        velocity.x = move.x;
        velocity.z = move.z;

        controller.Move(velocity * Time.deltaTime);
    }

    void HandleJump()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            velocity.y = jumpForce;
        }
    }

    void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to keep grounded
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime;
        }
    }

    /// <summary>
    /// Editor helper: Creates a player prefab
    /// </summary>
    #if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/1620/Create Player Controller")]
    static void CreatePlayerController()
    {
        // Create player capsule
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0, 1, 0);

        // Remove default collider (CharacterController handles collision)
        DestroyImmediate(player.GetComponent<CapsuleCollider>());

        // Add CharacterController
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.5f;
        cc.center = Vector3.zero;

        // Add our controller script
        player.AddComponent<SimpleCharacterController>();

        // Create camera
        GameObject camObj = new GameObject("PlayerCamera");
        camObj.transform.parent = player.transform;
        camObj.transform.localPosition = new Vector3(0, 0.8f, 0);
        Camera cam = camObj.AddComponent<Camera>();
        cam.nearClipPlane = 0.1f;
        camObj.AddComponent<AudioListener>();

        // Select the player
        UnityEditor.Selection.activeGameObject = player;

        Debug.Log("Player controller created. Position it in the scene and hit Play!");
    }
    #endif
}
