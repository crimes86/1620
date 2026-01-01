using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Network Manager for 1620 - Server-Authoritative Architecture
/// This is a stub/template - replace transport layer with your choice:
/// - Unity Netcode for GameObjects
/// - Mirror
/// - Fish-Net
/// - LiteNetLib (raw)
/// </summary>
public class NetworkManager1620 : MonoBehaviour
{
    [Header("Network Config")]
    [SerializeField] private int serverPort = 7777;
    [SerializeField] private int maxPlayers = 12;  // 4 trappers + 8 natives

    [Header("Runtime State")]
    [SerializeField] private bool isRunning = false;
    [SerializeField] private List<ulong> connectedClientIds = new List<ulong>();

    public static NetworkManager1620 Instance { get; private set; }

    public event Action OnServerStarted;
    public event Action OnServerStopped;
    public event Action<ulong> OnClientConnected;
    public event Action<ulong> OnClientDisconnected;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Auto-start server if in dedicated server mode
        if (DedicatedServerBootstrap.IsDedicatedServer)
        {
            StartServer();
        }
    }

    public void StartServer()
    {
        if (isRunning)
        {
            Debug.LogWarning("[Network] Server already running");
            return;
        }

        Debug.Log($"[Network] Starting server on port {serverPort}...");

        // TODO: Replace with actual transport initialization
        // Examples:
        //
        // Mirror:
        //   NetworkServer.Listen(maxPlayers);
        //
        // Unity Netcode:
        //   NetworkManager.Singleton.StartServer();
        //
        // Fish-Net:
        //   ServerManager.StartConnection();

        isRunning = true;
        OnServerStarted?.Invoke();
        Debug.Log("[Network] Server started");
    }

    public void StopServer()
    {
        if (!isRunning)
        {
            Debug.LogWarning("[Network] Server not running");
            return;
        }

        Debug.Log("[Network] Stopping server...");

        // TODO: Replace with actual transport shutdown

        isRunning = false;
        connectedClientIds.Clear();
        UpdatePlayerCount();
        OnServerStopped?.Invoke();
        Debug.Log("[Network] Server stopped");
    }

    /// <summary>
    /// Call this from your transport's OnClientConnected callback
    /// </summary>
    public void HandleClientConnected(ulong clientId)
    {
        if (connectedClientIds.Contains(clientId))
        {
            Debug.LogWarning($"[Network] Client {clientId} already connected");
            return;
        }

        connectedClientIds.Add(clientId);
        UpdatePlayerCount();
        OnClientConnected?.Invoke(clientId);
        Debug.Log($"[Network] Client {clientId} connected ({connectedClientIds.Count}/{maxPlayers})");
    }

    /// <summary>
    /// Call this from your transport's OnClientDisconnected callback
    /// </summary>
    public void HandleClientDisconnected(ulong clientId)
    {
        if (!connectedClientIds.Contains(clientId))
        {
            Debug.LogWarning($"[Network] Client {clientId} was not connected");
            return;
        }

        connectedClientIds.Remove(clientId);
        UpdatePlayerCount();
        OnClientDisconnected?.Invoke(clientId);
        Debug.Log($"[Network] Client {clientId} disconnected ({connectedClientIds.Count}/{maxPlayers})");
    }

    void UpdatePlayerCount()
    {
        // Notify the server bootstrap so it can switch to/from idle mode
        if (DedicatedServerBootstrap.Instance != null)
        {
            DedicatedServerBootstrap.Instance.SetPlayerCount(connectedClientIds.Count);
        }
    }

    public bool IsRunning => isRunning;
    public int PlayerCount => connectedClientIds.Count;
    public int MaxPlayers => maxPlayers;
}
