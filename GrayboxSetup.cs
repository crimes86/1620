using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor tool to quickly generate the graybox scene for "The Ambush"
/// Menu: Tools > 1620 > Generate Graybox Scene
/// </summary>
public class GrayboxSetup : EditorWindow
{
    // Scene dimensions (meters)
    private float sceneWidth = 200f;
    private float sceneDepth = 150f;
    private float riverWidth = 20f;
    private float treelineDepth = 30f;
    private float campWidth = 40f;
    private float campDepth = 30f;
    private int treeCount = 80;
    private int tentCount = 4;

    [MenuItem("Tools/1620/Generate Graybox Scene")]
    public static void ShowWindow()
    {
        GetWindow<GrayboxSetup>("1620 Graybox Setup");
    }

    void OnGUI()
    {
        GUILayout.Label("The Ambush - Graybox Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("Scene Dimensions", EditorStyles.boldLabel);
        sceneWidth = EditorGUILayout.FloatField("Scene Width (m)", sceneWidth);
        sceneDepth = EditorGUILayout.FloatField("Scene Depth (m)", sceneDepth);
        riverWidth = EditorGUILayout.FloatField("River Width (m)", riverWidth);
        treelineDepth = EditorGUILayout.FloatField("Treeline Depth (m)", treelineDepth);

        GUILayout.Space(10);
        GUILayout.Label("Camp Settings", EditorStyles.boldLabel);
        campWidth = EditorGUILayout.FloatField("Camp Width (m)", campWidth);
        campDepth = EditorGUILayout.FloatField("Camp Depth (m)", campDepth);
        tentCount = EditorGUILayout.IntSlider("Tent Count", tentCount, 2, 8);

        GUILayout.Space(10);
        GUILayout.Label("Environment", EditorStyles.boldLabel);
        treeCount = EditorGUILayout.IntSlider("Tree Count", treeCount, 30, 150);

        GUILayout.Space(20);

        if (GUILayout.Button("Generate Graybox Scene", GUILayout.Height(40)))
        {
            GenerateScene();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Clear Scene"))
        {
            if (EditorUtility.DisplayDialog("Clear Scene",
                "This will delete all graybox objects. Continue?", "Yes", "No"))
            {
                ClearScene();
            }
        }
    }

    void GenerateScene()
    {
        // Create parent objects
        GameObject environment = CreateOrGetParent("Environment");
        GameObject camp = CreateOrGetParent("Camp");
        GameObject spawnPoints = CreateOrGetParent("SpawnPoints");
        GameObject lighting = CreateOrGetParent("Lighting");

        // Materials
        Material groundMat = CreateMaterial("Ground", new Color(0.2f, 0.35f, 0.15f));
        Material riverMat = CreateMaterial("River", new Color(0.2f, 0.4f, 0.6f));
        Material treeTrunkMat = CreateMaterial("TreeTrunk", new Color(0.4f, 0.25f, 0.15f));
        Material treeCanopyMat = CreateMaterial("TreeCanopy", new Color(0.15f, 0.3f, 0.1f));
        Material tentMat = CreateMaterial("Tent", new Color(0.7f, 0.6f, 0.5f));
        Material canoeMat = CreateMaterial("Canoe", new Color(0.5f, 0.35f, 0.2f));

        // Ground plane
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.parent = environment.transform;
        ground.transform.localScale = new Vector3(sceneWidth / 10f, 1f, sceneDepth / 10f);
        ground.transform.position = Vector3.zero;
        ground.GetComponent<Renderer>().material = groundMat;

        // River
        GameObject river = GameObject.CreatePrimitive(PrimitiveType.Plane);
        river.name = "River";
        river.transform.parent = environment.transform;
        river.transform.localScale = new Vector3(sceneWidth / 10f, 1f, riverWidth / 10f);
        river.transform.position = new Vector3(0, -0.1f, -sceneDepth / 2f + riverWidth / 2f);
        river.GetComponent<Renderer>().material = riverMat;

        // Riverbank (sloped)
        GameObject riverbank = GameObject.CreatePrimitive(PrimitiveType.Cube);
        riverbank.name = "Riverbank";
        riverbank.transform.parent = environment.transform;
        riverbank.transform.localScale = new Vector3(sceneWidth, 2f, 8f);
        riverbank.transform.position = new Vector3(0, 0.5f, -sceneDepth / 2f + riverWidth + 4f);
        riverbank.transform.rotation = Quaternion.Euler(15f, 0, 0);
        riverbank.GetComponent<Renderer>().material = groundMat;

        // Trees parent
        GameObject trees = new GameObject("Trees");
        trees.transform.parent = environment.transform;

        // Generate trees in treeline (north side)
        for (int i = 0; i < treeCount; i++)
        {
            float x = Random.Range(-sceneWidth / 2f + 10f, sceneWidth / 2f - 10f);
            float z = Random.Range(sceneDepth / 2f - treelineDepth, sceneDepth / 2f - 5f);
            CreateTree(trees.transform, new Vector3(x, 0, z), treeTrunkMat, treeCanopyMat, i);
        }

        // Add some trees on the sides
        for (int i = 0; i < treeCount / 4; i++)
        {
            // Left side
            float xLeft = Random.Range(-sceneWidth / 2f, -sceneWidth / 2f + 20f);
            float zLeft = Random.Range(-sceneDepth / 4f, sceneDepth / 2f - treelineDepth);
            CreateTree(trees.transform, new Vector3(xLeft, 0, zLeft), treeTrunkMat, treeCanopyMat, treeCount + i);

            // Right side
            float xRight = Random.Range(sceneWidth / 2f - 20f, sceneWidth / 2f);
            float zRight = Random.Range(-sceneDepth / 4f, sceneDepth / 2f - treelineDepth);
            CreateTree(trees.transform, new Vector3(xRight, 0, zRight), treeTrunkMat, treeCanopyMat, treeCount + treeCount/4 + i);
        }

        // Camp - Tents
        float campCenterZ = 10f; // Camp is slightly north of center
        for (int i = 0; i < tentCount; i++)
        {
            GameObject tent = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tent.name = $"Tent_{i+1:D2}";
            tent.transform.parent = camp.transform;
            tent.transform.localScale = new Vector3(4f, 2.5f, 3f);

            float angle = (i / (float)tentCount) * Mathf.PI * 2f;
            float radius = campWidth / 4f;
            float x = Mathf.Cos(angle) * radius + Random.Range(-3f, 3f);
            float z = Mathf.Sin(angle) * radius + campCenterZ + Random.Range(-3f, 3f);

            tent.transform.position = new Vector3(x, 1.25f, z);
            tent.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            tent.GetComponent<Renderer>().material = tentMat;
        }

        // Campfire
        GameObject campfire = GameObject.CreatePrimitive(PrimitiveType.Cube);
        campfire.name = "Campfire";
        campfire.transform.parent = camp.transform;
        campfire.transform.localScale = new Vector3(1f, 0.3f, 1f);
        campfire.transform.position = new Vector3(0, 0.15f, campCenterZ);
        campfire.GetComponent<Renderer>().material = CreateMaterial("Fire", new Color(0.8f, 0.3f, 0.1f));

        // Campfire light
        GameObject fireLight = new GameObject("CampfireLight");
        fireLight.transform.parent = campfire.transform;
        fireLight.transform.localPosition = new Vector3(0, 2f, 0);
        Light light = fireLight.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.6f, 0.3f);
        light.intensity = 2f;
        light.range = 15f;

        // Canoes near river
        for (int i = 0; i < 2; i++)
        {
            GameObject canoe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            canoe.name = $"Canoe_{i+1:D2}";
            canoe.transform.parent = camp.transform;
            canoe.transform.localScale = new Vector3(1.5f, 0.5f, 6f);
            canoe.transform.position = new Vector3(-15f + i * 10f, 0.25f, -sceneDepth / 2f + riverWidth + 12f);
            canoe.transform.rotation = Quaternion.Euler(0, -20f + Random.Range(-10f, 10f), 0);
            canoe.GetComponent<Renderer>().material = canoeMat;
        }

        // Spawn points - Trappers (in camp)
        for (int i = 0; i < 4; i++)
        {
            GameObject spawn = new GameObject($"Spawn_Trapper_{i+1:D2}");
            spawn.transform.parent = spawnPoints.transform;
            spawn.transform.position = new Vector3(
                Random.Range(-campWidth/4f, campWidth/4f),
                0,
                campCenterZ + Random.Range(-campDepth/4f, campDepth/4f)
            );
            spawn.tag = "SpawnTrapper";
        }

        // Spawn points - Natives (in treeline)
        for (int i = 0; i < 8; i++)
        {
            GameObject spawn = new GameObject($"Spawn_Native_{i+1:D2}");
            spawn.transform.parent = spawnPoints.transform;
            spawn.transform.position = new Vector3(
                Random.Range(-sceneWidth/3f, sceneWidth/3f),
                0,
                sceneDepth/2f - Random.Range(5f, treelineDepth - 5f)
            );
            spawn.tag = "SpawnNative";
        }

        // Escape points (at canoes/river)
        GameObject escapePoint = new GameObject("EscapePoint_Canoes");
        escapePoint.transform.parent = spawnPoints.transform;
        escapePoint.transform.position = new Vector3(-10f, 0, -sceneDepth/2f + riverWidth + 10f);
        escapePoint.tag = "EscapePoint";

        // Lighting setup
        SetupLighting(lighting);

        Debug.Log("Graybox scene generated successfully!");
    }

    void CreateTree(Transform parent, Vector3 position, Material trunkMat, Material canopyMat, int index)
    {
        GameObject tree = new GameObject($"Tree_{index+1:D3}");
        tree.transform.parent = parent;
        tree.transform.position = position;

        // Trunk
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.parent = tree.transform;
        trunk.transform.localPosition = new Vector3(0, 4f, 0);
        trunk.transform.localScale = new Vector3(1f, 4f, 1f);
        trunk.GetComponent<Renderer>().material = trunkMat;

        // Canopy
        GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        canopy.name = "Canopy";
        canopy.transform.parent = tree.transform;
        canopy.transform.localPosition = new Vector3(0, 9f, 0);
        canopy.transform.localScale = new Vector3(6f, 6f, 6f);
        canopy.GetComponent<Renderer>().material = canopyMat;
    }

    void SetupLighting(GameObject lightingParent)
    {
        // Find or create directional light
        Light[] lights = FindObjectsOfType<Light>();
        Light sun = null;

        foreach (Light l in lights)
        {
            if (l.type == LightType.Directional)
            {
                sun = l;
                break;
            }
        }

        if (sun == null)
        {
            GameObject sunObj = new GameObject("Directional Light");
            sunObj.transform.parent = lightingParent.transform;
            sun = sunObj.AddComponent<Light>();
            sun.type = LightType.Directional;
        }

        // Dawn lighting
        sun.transform.rotation = Quaternion.Euler(15f, -30f, 0);
        sun.color = new Color(1f, 0.85f, 0.7f);
        sun.intensity = 0.8f;

        // Enable fog
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.015f;
        RenderSettings.fogColor = new Color(0.7f, 0.75f, 0.8f);

        // Ambient
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.3f, 0.35f, 0.5f);
        RenderSettings.ambientEquatorColor = new Color(0.4f, 0.35f, 0.4f);
        RenderSettings.ambientGroundColor = new Color(0.2f, 0.15f, 0.1f);
    }

    GameObject CreateOrGetParent(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj == null)
        {
            obj = new GameObject(name);
        }
        return obj;
    }

    Material CreateMaterial(string name, Color color)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = name;
        mat.color = color;

        // Save material to Assets
        string path = $"Assets/Materials/{name}.mat";
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }
        AssetDatabase.CreateAsset(mat, path);

        return mat;
    }

    void ClearScene()
    {
        string[] parents = { "Environment", "Camp", "SpawnPoints", "Lighting" };
        foreach (string name in parents)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        Debug.Log("Scene cleared.");
    }
}
