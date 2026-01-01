using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

/// <summary>
/// Networked player for 1620
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class NetworkedPlayer : NetworkBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float sprintSpeed = 12f;
    public float jumpForce = 7f;
    public float gravity = 20f;
    public float rotationSpeed = 10f;

    [Header("Team")]
    [SyncVar]
    public bool isTrapper = true;

    private CharacterController controller;
    private WoWCameraController cam;
    private Vector3 velocity;
    private Vector2 moveInput;
    private bool sprint, jump;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public override void OnStartLocalPlayer()
    {
        Debug.Log($"[1620] OnStartLocalPlayer called at position {transform.position}");

        // Find the WoWCamera specifically, or fall back to any camera
        Camera targetCam = null;

        // First, look for WoWCamera by name
        var wowCamObj = GameObject.Find("WoWCamera");
        if (wowCamObj != null)
        {
            targetCam = wowCamObj.GetComponent<Camera>();
            Debug.Log("[1620] Found WoWCamera by name");
        }

        // Fall back to Camera.main
        if (targetCam == null)
        {
            targetCam = Camera.main;
            if (targetCam != null)
                Debug.Log($"[1620] Using Camera.main: {targetCam.name}");
        }

        // Last resort: find any camera
        if (targetCam == null)
        {
            targetCam = FindFirstObjectByType<Camera>();
            if (targetCam != null)
                Debug.Log($"[1620] Using first camera found: {targetCam.name}");
        }

        if (targetCam != null)
        {
            // Disable any other cameras to avoid conflicts
            foreach (var otherCam in FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                if (otherCam != targetCam)
                {
                    otherCam.enabled = false;
                    Debug.Log($"[1620] Disabled extra camera: {otherCam.name}");
                }
            }

            cam = targetCam.GetComponent<WoWCameraController>();
            if (cam == null)
            {
                Debug.Log("[1620] Adding WoWCameraController to camera");
                cam = targetCam.gameObject.AddComponent<WoWCameraController>();
            }
            cam.SetTarget(transform);
            Debug.Log($"[1620] Camera {targetCam.name} target set to player");
        }
        else
        {
            Debug.LogError("[1620] No camera found in scene!");
        }

        // Color based on team
        var rend = GetComponentInChildren<Renderer>();
        if (rend != null)
            rend.material.color = isTrapper ? new Color(0.2f, 0.4f, 0.8f) : new Color(0.8f, 0.3f, 0.2f);
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        ReadInput();
        Move();
    }

    void ReadInput()
    {
        if (Keyboard.current == null) return;

        moveInput = Vector2.zero;
        if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
        if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
        if (Keyboard.current.dKey.isPressed) moveInput.x += 1;
        if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;

        sprint = Keyboard.current.leftShiftKey.isPressed;
        jump = Keyboard.current.spaceKey.wasPressedThisFrame;

        if (Mouse.current != null && Mouse.current.leftButton.isPressed && Mouse.current.rightButton.isPressed)
            moveInput.y = 1;
    }

    void Move()
    {
        if (cam == null) return;

        Vector3 forward = cam.GetFlatForward();
        Vector3 right = cam.GetFlatRight();
        Vector3 dir = (forward * moveInput.y + right * moveInput.x).normalized;

        if (dir.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), rotationSpeed * Time.deltaTime);
        }

        if (cam.IsOrbiting && moveInput.magnitude < 0.1f)
            transform.rotation = Quaternion.Euler(0, cam.HorizontalAngle, 0);

        float speed = moveInput.y > 0 ? (sprint ? sprintSpeed : runSpeed) : walkSpeed * 0.5f;
        if (moveInput.magnitude < 0.1f) speed = 0;

        velocity.x = dir.x * speed;
        velocity.z = dir.z * speed;

        if (controller.isGrounded)
        {
            velocity.y = jump ? jumpForce : -2f;
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }
}
