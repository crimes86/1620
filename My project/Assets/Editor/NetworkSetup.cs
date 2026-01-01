using UnityEngine;
using UnityEditor;
using Mirror;

public class NetworkSetup
{
    [MenuItem("Tools/1620/Setup Networking")]
    public static void Setup()
    {
        CreateTags();
        CreatePlayerPrefab();
        AddNetworkManager();
        TagSpawnPoints();
        SetupNetworkCamera();
        Debug.Log("[1620] Networking setup complete! Hit Play and click Host.");
    }

    static void SetupNetworkCamera()
    {
        // Remove any single-player "Player" object (from graybox generator)
        var existingPlayer = GameObject.Find("Player");
        if (existingPlayer != null)
        {
            Object.DestroyImmediate(existingPlayer);
            Debug.Log("[1620] Removed single-player Player object");
        }

        // Ensure there's a camera tagged MainCamera
        var mainCam = Camera.main;
        if (mainCam == null)
        {
            // Look for any camera
            var anyCam = Object.FindFirstObjectByType<Camera>();
            if (anyCam != null)
            {
                anyCam.tag = "MainCamera";
                Debug.Log($"[1620] Tagged {anyCam.name} as MainCamera");
            }
            else
            {
                // Create a camera
                var camObj = new GameObject("NetworkCamera");
                var cam = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
                camObj.tag = "MainCamera";
                camObj.transform.position = new Vector3(0, 10, -20);
                camObj.transform.rotation = Quaternion.Euler(20, 0, 0);
                Debug.Log("[1620] Created NetworkCamera");
            }
        }
        else
        {
            Debug.Log($"[1620] MainCamera already exists: {mainCam.name}");
        }
    }

    static void CreateTags()
    {
        AddTag("SpawnTrapper");
        AddTag("SpawnNative");
        AddTag("EscapePoint");
    }

    static void AddTag(string tag)
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>("ProjectSettings/TagManager.asset"));
        var tags = tagManager.FindProperty("tags");

        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;

        tags.arraySize++;
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }

    static void CreatePlayerPrefab()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        string path = "Assets/Prefabs/NetworkedPlayer.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            Debug.Log("[1620] Player prefab already exists");
            return;
        }

        GameObject player = new GameObject("NetworkedPlayer");

        // Visual
        var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.name = "Body";
        capsule.transform.SetParent(player.transform);
        capsule.transform.localPosition = Vector3.up;
        Object.DestroyImmediate(capsule.GetComponent<Collider>());

        // Components
        var cc = player.AddComponent<CharacterController>();
        cc.center = Vector3.up;
        cc.height = 2f;
        cc.radius = 0.5f;

        player.AddComponent<NetworkIdentity>();
        player.AddComponent<NetworkTransformReliable>();
        player.AddComponent<NetworkedPlayer>();

        PrefabUtility.SaveAsPrefabAsset(player, path);
        Object.DestroyImmediate(player);
        Debug.Log("[1620] Created player prefab");
    }

    static void AddNetworkManager()
    {
        if (Object.FindFirstObjectByType<GameNetworkManager>() != null)
        {
            Debug.Log("[1620] NetworkManager already in scene");
            return;
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/NetworkedPlayer.prefab");

        GameObject nm = new GameObject("NetworkManager");
        var manager = nm.AddComponent<GameNetworkManager>();
        manager.playerPrefab = prefab;

        nm.AddComponent<kcp2k.KcpTransport>();

        Debug.Log("[1620] Added NetworkManager to scene");
    }

    static void TagSpawnPoints()
    {
        foreach (var obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (obj.name.Contains("Spawn_Trapper")) obj.tag = "SpawnTrapper";
            else if (obj.name.Contains("Spawn_Native")) obj.tag = "SpawnNative";
            else if (obj.name.Contains("EscapePoint")) obj.tag = "EscapePoint";
        }
        Debug.Log("[1620] Tagged spawn points");
    }
}
