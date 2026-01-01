using UnityEngine;
using Mirror;

/// <summary>
/// Custom NetworkManager for 1620
/// Handles dedicated server mode and player spawning
/// </summary>
public class GameNetworkManager : NetworkManager
{
    [Header("1620 Settings")]
    public Transform[] trapperSpawnPoints;
    public Transform[] nativeSpawnPoints;

    private int trapperCount = 0;
    private int nativeCount = 0;

    public override void Start()
    {
        base.Start();

        // Auto-start server in dedicated mode
        if (DedicatedServerBootstrap.IsDedicatedServer)
        {
            Debug.Log("[Server] Auto-starting dedicated server...");
            StartServer();
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("[Server] Started");
        FindSpawnPoints();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        trapperCount = 0;
        nativeCount = 0;
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // First 4 players are trappers, rest are natives
        bool isTrapper = trapperCount < 4;
        Transform spawn = GetSpawnPoint(isTrapper);

        GameObject player = Instantiate(playerPrefab, spawn.position, spawn.rotation);

        var netPlayer = player.GetComponent<NetworkedPlayer>();
        if (netPlayer != null)
        {
            netPlayer.isTrapper = isTrapper;
        }

        if (isTrapper) trapperCount++; else nativeCount++;

        NetworkServer.AddPlayerForConnection(conn, player);
        Debug.Log($"[Server] Player joined as {(isTrapper ? "Trapper" : "Native")}");

        // Update server bootstrap player count
        if (DedicatedServerBootstrap.Instance != null)
            DedicatedServerBootstrap.Instance.PlayerConnected();
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (conn.identity != null)
        {
            var netPlayer = conn.identity.GetComponent<NetworkedPlayer>();
            if (netPlayer != null)
            {
                if (netPlayer.isTrapper) trapperCount--; else nativeCount--;
            }
        }

        // Update server bootstrap player count
        if (DedicatedServerBootstrap.Instance != null)
            DedicatedServerBootstrap.Instance.PlayerDisconnected();

        base.OnServerDisconnect(conn);
    }

    Transform GetSpawnPoint(bool isTrapper)
    {
        Transform[] spawns = isTrapper ? trapperSpawnPoints : nativeSpawnPoints;
        if (spawns == null || spawns.Length == 0) return transform;

        int index = (isTrapper ? trapperCount : nativeCount) % spawns.Length;
        return spawns[index];
    }

    void FindSpawnPoints()
    {
        var trappers = GameObject.FindGameObjectsWithTag("SpawnTrapper");
        trapperSpawnPoints = new Transform[trappers.Length];
        for (int i = 0; i < trappers.Length; i++)
            trapperSpawnPoints[i] = trappers[i].transform;

        var natives = GameObject.FindGameObjectsWithTag("SpawnNative");
        nativeSpawnPoints = new Transform[natives.Length];
        for (int i = 0; i < natives.Length; i++)
            nativeSpawnPoints[i] = natives[i].transform;

        Debug.Log($"[Server] Found {trapperSpawnPoints.Length} trapper, {nativeSpawnPoints.Length} native spawns");
    }
}
