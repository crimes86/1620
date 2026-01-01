using UnityEngine;
using System;

/// <summary>
/// Dedicated Server Bootstrap for 1620
/// Attach to a GameObject in your startup scene (or use RuntimeInitializeOnLoadMethod)
/// Prevents high CPU usage by properly configuring headless server mode
/// </summary>
public class DedicatedServerBootstrap : MonoBehaviour
{
    [Header("Server Settings")]
    [SerializeField] private int serverTickRate = 20;        // Hz - 20 is fine for melee/bow
    [SerializeField] private int idleTickRate = 1;           // Hz when no players connected
    [SerializeField] private int physicsTickRate = 20;       // Hz - matches server tick

    [Header("Runtime Info")]
    [SerializeField] private int connectedPlayers = 0;
    [SerializeField] private bool isIdling = false;

    public static DedicatedServerBootstrap Instance { get; private set; }
    public static bool IsServer { get; private set; }
    public static bool IsDedicatedServer { get; private set; }

    public int ConnectedPlayers => connectedPlayers;

    public event Action<int> OnPlayerCountChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoInitialize()
    {
        // Detect if we're running as dedicated server
        #if UNITY_SERVER
            IsDedicatedServer = true;
            IsServer = true;
        #else
            // Check command line for -batchmode (manual server builds)
            string[] args = Environment.GetCommandLineArgs();
            foreach (string arg in args)
            {
                if (arg.ToLower() == "-batchmode" || arg.ToLower() == "-server")
                {
                    IsDedicatedServer = true;
                    IsServer = true;
                    break;
                }
            }
        #endif

        if (IsDedicatedServer)
        {
            Debug.Log("[1620 Server] Dedicated server mode detected");

            // Create bootstrap object
            GameObject bootstrapObj = new GameObject("ServerBootstrap");
            DontDestroyOnLoad(bootstrapObj);
            bootstrapObj.AddComponent<DedicatedServerBootstrap>();
        }
    }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (IsDedicatedServer)
        {
            ConfigureServerMode();
        }
    }

    void ConfigureServerMode()
    {
        Debug.Log("[1620 Server] Configuring server mode...");

        // === CRITICAL: Prevent CPU spin ===

        // 1. Cap frame rate (THIS IS THE BIG ONE)
        Application.targetFrameRate = serverTickRate;
        QualitySettings.vSyncCount = 0;  // Disable VSync (useless on server)

        // 2. Configure physics
        Time.fixedDeltaTime = 1f / physicsTickRate;
        Physics.autoSimulation = true;  // Let Unity handle it at our fixed rate

        // 3. Disable rendering systems
        #if UNITY_SERVER
            // These are already disabled in server builds, but belt and suspenders
        #endif

        // 4. Reduce quality settings
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.shadowResolution = ShadowResolution.Low;
        QualitySettings.skinWeights = SkinWeights.OneBone;
        QualitySettings.lodBias = 0.1f;
        QualitySettings.maximumLODLevel = 3;
        QualitySettings.particleRaycastBudget = 0;

        // 5. Disable audio
        AudioListener.volume = 0f;
        AudioListener.pause = true;

        // 6. GC settings for server (less frequent, larger collections)
        // Unity doesn't expose GC settings directly, but we can hint
        System.GC.Collect();

        Debug.Log($"[1620 Server] Configured: {serverTickRate}Hz tick, {physicsTickRate}Hz physics");
        Debug.Log($"[1620 Server] Idle mode: {idleTickRate}Hz when no players");

        // Start in idle mode
        EnterIdleMode();
    }

    void Update()
    {
        if (!IsDedicatedServer) return;

        // Check if we should switch between active/idle mode
        bool shouldIdle = connectedPlayers == 0;

        if (shouldIdle && !isIdling)
        {
            EnterIdleMode();
        }
        else if (!shouldIdle && isIdling)
        {
            ExitIdleMode();
        }
    }

    void EnterIdleMode()
    {
        isIdling = true;
        Application.targetFrameRate = idleTickRate;
        Time.fixedDeltaTime = 1f / idleTickRate;  // Slow physics too
        Debug.Log($"[1620 Server] Entering idle mode ({idleTickRate}Hz)");
    }

    void ExitIdleMode()
    {
        isIdling = false;
        Application.targetFrameRate = serverTickRate;
        Time.fixedDeltaTime = 1f / physicsTickRate;
        Debug.Log($"[1620 Server] Exiting idle mode ({serverTickRate}Hz)");
    }

    /// <summary>
    /// Call this when players connect/disconnect
    /// </summary>
    public void SetPlayerCount(int count)
    {
        int oldCount = connectedPlayers;
        connectedPlayers = Mathf.Max(0, count);

        if (oldCount != connectedPlayers)
        {
            Debug.Log($"[1620 Server] Player count: {connectedPlayers}");
            OnPlayerCountChanged?.Invoke(connectedPlayers);
        }
    }

    public void PlayerConnected()
    {
        SetPlayerCount(connectedPlayers + 1);
    }

    public void PlayerDisconnected()
    {
        SetPlayerCount(connectedPlayers - 1);
    }

    void OnApplicationQuit()
    {
        Debug.Log("[1620 Server] Server shutting down...");
    }

    #if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/1620/Simulate Server Mode")]
    static void SimulateServerMode()
    {
        IsDedicatedServer = true;
        IsServer = true;
        Debug.Log("[1620 Server] Server mode simulation enabled (editor only)");
    }

    [UnityEditor.MenuItem("Tools/1620/Log Server Stats")]
    static void LogServerStats()
    {
        Debug.Log($"[1620 Server] IsServer: {IsServer}");
        Debug.Log($"[1620 Server] IsDedicatedServer: {IsDedicatedServer}");
        Debug.Log($"[1620 Server] Target FPS: {Application.targetFrameRate}");
        Debug.Log($"[1620 Server] Fixed DeltaTime: {Time.fixedDeltaTime}");
        if (Instance != null)
        {
            Debug.Log($"[1620 Server] Players: {Instance.connectedPlayers}");
            Debug.Log($"[1620 Server] Idling: {Instance.isIdling}");
        }
    }
    #endif
}
