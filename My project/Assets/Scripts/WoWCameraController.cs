using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// WoW-style third-person camera controller
/// - Right-click + drag to orbit camera
/// - Scroll wheel to zoom
/// - Character faces movement direction
/// - Camera collision with environment
/// </summary>
public class WoWCameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0, 1.5f, 0); // Shoulder height

    [Header("Distance")]
    [SerializeField] private float defaultDistance = 5f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 30f;
    [SerializeField] private float zoomSpeed = 15f;
    [SerializeField] private float zoomSmoothing = 50f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;

    [Header("Collision")]
    [SerializeField] private float collisionPadding = 0.2f;
    [SerializeField] private LayerMask collisionLayers = -1; // Everything

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothing = 15f;

    // Current state
    private float currentDistance;
    private float targetDistance;
    private float horizontalAngle = 0f;
    private float verticalAngle = 20f;
    private Vector3 currentVelocity;

    // Input state
    private bool isOrbiting = false;

    void Start()
    {
        currentDistance = defaultDistance;
        targetDistance = defaultDistance;

        // If no target set, try to find Player
        if (target == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        // Initial position behind target
        if (target != null)
        {
            horizontalAngle = target.eulerAngles.y;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleInput();
        UpdateCameraPosition();
    }

    void HandleInput()
    {
        if (Mouse.current == null) return;

        // Right-click to orbit
        isOrbiting = Mouse.current.rightButton.isPressed;

        if (isOrbiting)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            horizontalAngle += mouseDelta.x * rotationSpeed * 0.1f;
            verticalAngle -= mouseDelta.y * rotationSpeed * 0.1f;
            verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Scroll wheel zoom
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float scrollNormalized = scroll / 120f;
            targetDistance -= scrollNormalized * zoomSpeed;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }

        // Smooth zoom
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomSmoothing);
    }

    void UpdateCameraPosition()
    {
        // Calculate desired position
        Vector3 targetPosition = target.position + targetOffset;

        // Convert angles to direction
        Quaternion rotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0);
        Vector3 direction = rotation * Vector3.back;

        // Check for collision
        float adjustedDistance = currentDistance;
        RaycastHit hit;
        if (Physics.SphereCast(targetPosition, collisionPadding, direction, out hit, currentDistance, collisionLayers))
        {
            adjustedDistance = hit.distance - collisionPadding;
            adjustedDistance = Mathf.Max(adjustedDistance, minDistance * 0.5f);
        }

        // Calculate final position
        Vector3 desiredPosition = targetPosition + direction * adjustedDistance;

        // Smooth movement
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / positionSmoothing);

        // Always look at target
        transform.LookAt(targetPosition);
    }

    /// <summary>
    /// Get the camera's forward direction projected onto the XZ plane (for movement)
    /// </summary>
    public Vector3 GetFlatForward()
    {
        Vector3 forward = transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    /// <summary>
    /// Get the camera's right direction projected onto the XZ plane
    /// </summary>
    public Vector3 GetFlatRight()
    {
        Vector3 right = transform.right;
        right.y = 0;
        return right.normalized;
    }

    public float HorizontalAngle => horizontalAngle;
    public bool IsOrbiting => isOrbiting;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            horizontalAngle = target.eulerAngles.y;
            currentDistance = defaultDistance;
            targetDistance = defaultDistance;

            // IMMEDIATELY snap camera to correct position (no smoothing)
            Vector3 targetPosition = target.position + targetOffset;
            Quaternion rotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0);
            Vector3 direction = rotation * Vector3.back;
            Vector3 cameraPos = targetPosition + direction * currentDistance;

            transform.position = cameraPos;
            transform.LookAt(targetPosition);
            currentVelocity = Vector3.zero; // Reset velocity so smoothing starts fresh

            Debug.Log($"[WoWCamera] Target set to {target.name}, snapped camera to {cameraPos}");
        }
    }
}
