using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Editor tool to generate "The Revenant" style trapping camp graybox
/// Keelboats on the Missouri, makeshift camp, skinning operations
/// </summary>
public class GrayboxSetup : EditorWindow
{
    // Scene dimensions (meters)
    private static float sceneWidth = 250f;
    private static float sceneDepth = 200f;
    private static float riverWidth = 40f;  // Wide Missouri River
    private static int keelboatCount = 2;
    private static int leantoCount = 5;
    private static int skinningFrameCount = 6;

    [MenuItem("Tools/1620/Generate Graybox Scene")]
    public static void ShowWindow()
    {
        GetWindow<GrayboxSetup>("1620 Graybox Setup");
    }

    [MenuItem("Tools/1620/Quick Generate (Revenant Style)")]
    public static void QuickGenerate()
    {
        if (EditorUtility.DisplayDialog("Generate Scene",
            "This will replace the current scene with a Revenant-style trapping camp. Continue?", "Yes", "No"))
        {
            GenerateSceneStatic();
            CreatePlayerController();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }

    public static void GenerateFromCommandLine()
    {
        Debug.Log("[1620] Generating Revenant-style graybox scene...");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        GenerateSceneStatic();
        CreatePlayerController();

        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Graybox_Ambush.unity");
        AssetDatabase.SaveAssets();

        Debug.Log("[1620] Scene saved to Assets/Scenes/Graybox_Ambush.unity");
    }

    static void CreatePlayerController()
    {
        // Remove default camera
        GameObject mainCam = GameObject.Find("Main Camera");
        if (mainCam != null) Object.DestroyImmediate(mainCam);

        GameObject existingPlayer = GameObject.Find("Player");
        if (existingPlayer != null) Object.DestroyImmediate(existingPlayer);

        GameObject existingCam = GameObject.Find("WoWCamera");
        if (existingCam != null) Object.DestroyImmediate(existingCam);

        // Remove AudioListeners
        foreach (var listener in Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
            Object.DestroyImmediate(listener);

        // Create player
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0, 1, -20); // Near the skinning area
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());

        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.5f;
        cc.center = Vector3.zero;

        var charScript = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/WoWCharacterController.cs");
        if (charScript != null)
        {
            var type = charScript.GetClass();
            if (type != null) player.AddComponent(type);
        }

        // Create camera
        GameObject camObj = new GameObject("WoWCamera");
        camObj.transform.position = new Vector3(0, 3, -25);
        Camera cam = camObj.AddComponent<Camera>();
        cam.nearClipPlane = 0.1f;
        camObj.AddComponent<AudioListener>();
        camObj.tag = "MainCamera";

        var camScript = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/WoWCameraController.cs");
        if (camScript != null)
        {
            var type = camScript.GetClass();
            if (type != null) camObj.AddComponent(type);
        }

        Debug.Log("[1620] Player and camera created");
    }

    static void GenerateSceneStatic()
    {
        // Clear existing
        ClearSceneStatic();

        // Parents
        GameObject environment = new GameObject("Environment");
        GameObject river = new GameObject("River");
        GameObject camp = new GameObject("Camp");
        GameObject workArea = new GameObject("WorkArea");
        GameObject spawnPoints = new GameObject("SpawnPoints");
        GameObject lighting = new GameObject("Lighting");

        // Materials
        Material groundMat = CreateMat("Ground", new Color(0.18f, 0.12f, 0.08f)); // Muddy brown
        Material riverMat = CreateMat("River", new Color(0.15f, 0.25f, 0.35f)); // Dark river
        Material muddyMat = CreateMat("MuddyGround", new Color(0.12f, 0.08f, 0.05f)); // Trampled mud
        Material woodMat = CreateMat("Wood", new Color(0.35f, 0.22f, 0.12f));
        Material canvasMat = CreateMat("Canvas", new Color(0.55f, 0.5f, 0.4f)); // Dirty canvas
        Material peltMat = CreateMat("Pelt", new Color(0.4f, 0.3f, 0.2f)); // Beaver pelts
        Material treeTrunkMat = CreateMat("TreeTrunk", new Color(0.3f, 0.2f, 0.12f));
        Material treeCanopyMat = CreateMat("TreeCanopy", new Color(0.12f, 0.22f, 0.08f));
        Material keelboatMat = CreateMat("Keelboat", new Color(0.3f, 0.2f, 0.1f));
        Material hillMat = CreateMat("DistantHill", new Color(0.1f, 0.15f, 0.08f));
        Material crateMat = CreateMat("Crate", new Color(0.45f, 0.35f, 0.2f));

        // === GROUND ===
        // Main ground - extended
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.parent = environment.transform;
        ground.transform.localScale = new Vector3(80f, 1f, 80f);
        ground.transform.position = Vector3.zero;
        ground.GetComponent<Renderer>().material = groundMat;

        // Muddy work area ground
        GameObject muddyArea = GameObject.CreatePrimitive(PrimitiveType.Plane);
        muddyArea.name = "MuddyWorkArea";
        muddyArea.transform.parent = environment.transform;
        muddyArea.transform.localScale = new Vector3(8f, 1f, 6f);
        muddyArea.transform.position = new Vector3(0, 0.02f, -15f);
        muddyArea.GetComponent<Renderer>().material = muddyMat;

        // === RIVER (South edge - wide Missouri) ===
        // River is well below ground level
        float riverY = -4f;  // Water surface
        float shoreY = -2f;  // Muddy shore where boats are moored
        float bankStartZ = -55f;  // Where slope starts (from camp side)
        float shoreZ = -70f;  // Where flat shore begins
        float waterZ = -85f;  // Center of water

        // Water surface (low, in the river channel)
        GameObject riverPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        riverPlane.name = "RiverSurface";
        riverPlane.transform.parent = river.transform;
        riverPlane.transform.localScale = new Vector3(sceneWidth / 10f, 1f, 4f);
        riverPlane.transform.position = new Vector3(0, riverY, waterZ);
        riverPlane.GetComponent<Renderer>().material = riverMat;

        // Flat muddy shore (where you walk to the boats)
        GameObject shore = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shore.name = "Shore";
        shore.transform.parent = river.transform;
        shore.transform.localScale = new Vector3(sceneWidth, 0.5f, 20f);
        shore.transform.position = new Vector3(0, shoreY, shoreZ + 5f);
        shore.GetComponent<Renderer>().material = muddyMat;

        // Bank slope from camp (Y=0) down to shore (Y=shoreY)
        // Simple approach: a rotated cube connecting the two levels
        float slopeLength = 18f;
        float slopeAngle = Mathf.Atan2(Mathf.Abs(shoreY), 15f) * Mathf.Rad2Deg;
        GameObject riverbank = GameObject.CreatePrimitive(PrimitiveType.Cube);
        riverbank.name = "Riverbank";
        riverbank.transform.parent = river.transform;
        riverbank.transform.localScale = new Vector3(sceneWidth, 0.5f, slopeLength);
        riverbank.transform.position = new Vector3(0, shoreY / 2f, bankStartZ - 7f);
        riverbank.transform.rotation = Quaternion.Euler(slopeAngle, 0, 0);
        riverbank.GetComponent<Renderer>().material = groundMat;

        // Far bank (other side of river - just visual)
        GameObject farBank = GameObject.CreatePrimitive(PrimitiveType.Cube);
        farBank.name = "FarBank";
        farBank.transform.parent = river.transform;
        farBank.transform.localScale = new Vector3(sceneWidth, 4f, 30f);
        farBank.transform.position = new Vector3(0, -2f, -110f);
        farBank.GetComponent<Renderer>().material = groundMat;

        // === KEELBOATS ===
        // Boats moored at shore, partially in water
        float boatZ = shoreZ - 5f;
        for (int i = 0; i < keelboatCount; i++)
        {
            CreateKeelboat(river.transform, new Vector3(-25f + i * 50f, shoreY + 0.5f, boatZ), keelboatMat, woodMat, i);
        }

        // Gangplanks from boats to shore
        for (int i = 0; i < keelboatCount; i++)
        {
            GameObject plank = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plank.name = $"Gangplank_{i + 1}";
            plank.transform.parent = river.transform;
            plank.transform.localScale = new Vector3(1.5f, 0.2f, 8f);
            plank.transform.position = new Vector3(-25f + i * 50f + 6f, shoreY + 0.8f, boatZ + 12f);
            plank.transform.rotation = Quaternion.Euler(8f, -15f, 0);
            plank.GetComponent<Renderer>().material = woodMat;
        }

        // === LEAN-TOS (Makeshift shelters) ===
        float campZ = -5f;
        for (int i = 0; i < leantoCount; i++)
        {
            float angle = (i / (float)leantoCount) * Mathf.PI + Mathf.PI * 0.2f;
            float radius = 25f + Random.Range(-5f, 5f);
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius * 0.6f + campZ;
            CreateLeanTo(camp.transform, new Vector3(x, 0, z), canvasMat, woodMat, i);
        }

        // === SKINNING FRAMES (Pelts drying) ===
        for (int i = 0; i < skinningFrameCount; i++)
        {
            float x = -20f + i * 8f + Random.Range(-2f, 2f);
            float z = -18f + Random.Range(-3f, 3f);
            CreateSkinningFrame(workArea.transform, new Vector3(x, 0, z), woodMat, peltMat, i);
        }

        // === WORK STATIONS ===
        // Skinning table
        GameObject skinTable = GameObject.CreatePrimitive(PrimitiveType.Cube);
        skinTable.name = "SkinningTable";
        skinTable.transform.parent = workArea.transform;
        skinTable.transform.localScale = new Vector3(4f, 0.8f, 2f);
        skinTable.transform.position = new Vector3(5f, 0.4f, -12f);
        skinTable.GetComponent<Renderer>().material = woodMat;

        // Pelts on table
        for (int i = 0; i < 3; i++)
        {
            GameObject pelt = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pelt.name = $"PeltOnTable_{i + 1}";
            pelt.transform.parent = workArea.transform;
            pelt.transform.localScale = new Vector3(0.8f, 0.1f, 1.2f);
            pelt.transform.position = new Vector3(4f + i * 1.2f, 0.85f, -12f);
            pelt.transform.rotation = Quaternion.Euler(0, Random.Range(-20f, 20f), 0);
            pelt.GetComponent<Renderer>().material = peltMat;
        }

        // === STACKED PELTS (ready for loading) ===
        CreatePeltStack(workArea.transform, new Vector3(-25f, 0, -30f), peltMat, "PeltStack_1");
        CreatePeltStack(workArea.transform, new Vector3(-20f, 0, -32f), peltMat, "PeltStack_2");
        CreatePeltStack(workArea.transform, new Vector3(20f, 0, -28f), peltMat, "PeltStack_3");

        // === SUPPLY CRATES AND BARRELS ===
        // Crates near boats
        for (int i = 0; i < 8; i++)
        {
            GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crate.name = $"Crate_{i + 1}";
            crate.transform.parent = camp.transform;
            float size = Random.Range(0.8f, 1.2f);
            crate.transform.localScale = new Vector3(size, size, size);
            crate.transform.position = new Vector3(
                Random.Range(-40f, 30f),
                size / 2f,
                -35f + Random.Range(-5f, 5f)
            );
            crate.transform.rotation = Quaternion.Euler(0, Random.Range(0, 45f), 0);
            crate.GetComponent<Renderer>().material = crateMat;
        }

        // Barrels
        for (int i = 0; i < 5; i++)
        {
            GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = $"Barrel_{i + 1}";
            barrel.transform.parent = camp.transform;
            barrel.transform.localScale = new Vector3(0.8f, 0.6f, 0.8f);
            barrel.transform.position = new Vector3(
                Random.Range(-35f, 35f),
                0.6f,
                Random.Range(-38f, -25f)
            );
            barrel.GetComponent<Renderer>().material = woodMat;
        }

        // === FIRE PITS ===
        CreateFirePit(camp.transform, new Vector3(0, 0, -8f), "MainFire");
        CreateFirePit(camp.transform, new Vector3(-15f, 0, 5f), "CookFire");
        CreateFirePit(camp.transform, new Vector3(18f, 0, -5f), "WorkFire");

        // === WEAPON RACK ===
        GameObject weaponRack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        weaponRack.name = "WeaponRack";
        weaponRack.transform.parent = camp.transform;
        weaponRack.transform.localScale = new Vector3(4f, 2f, 0.3f);
        weaponRack.transform.position = new Vector3(12f, 1f, 0f);
        weaponRack.GetComponent<Renderer>().material = woodMat;

        // Rifles on rack (simple cylinders)
        for (int i = 0; i < 4; i++)
        {
            GameObject rifle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rifle.name = $"Rifle_{i + 1}";
            rifle.transform.parent = camp.transform;
            rifle.transform.localScale = new Vector3(0.05f, 0.7f, 0.05f);
            rifle.transform.position = new Vector3(10.5f + i * 1f, 1.2f, 0.2f);
            rifle.transform.rotation = Quaternion.Euler(0, 0, 15f);
            rifle.GetComponent<Renderer>().material = woodMat;
        }

        // === TREES (Dense treeline to the north) ===
        GameObject trees = new GameObject("Trees");
        trees.transform.parent = environment.transform;

        // Dense treeline (Native spawn area)
        float treelineZ = 50f;
        for (float x = -120f; x <= 120f; x += 8f)
        {
            for (float z = treelineZ; z <= treelineZ + 40f; z += 10f)
            {
                float jx = x + Random.Range(-3f, 3f);
                float jz = z + Random.Range(-4f, 4f);
                CreateTree(trees.transform, new Vector3(jx, 0, jz), treeTrunkMat, treeCanopyMat);
            }
        }

        // Side trees (flanking positions)
        for (float z = -20f; z <= 60f; z += 12f)
        {
            // Left flank
            for (float x = -80f; x >= -120f; x -= 10f)
            {
                if (Random.value > 0.3f)
                    CreateTree(trees.transform, new Vector3(x + Random.Range(-3f, 3f), 0, z + Random.Range(-4f, 4f)), treeTrunkMat, treeCanopyMat);
            }
            // Right flank
            for (float x = 80f; x <= 120f; x += 10f)
            {
                if (Random.value > 0.3f)
                    CreateTree(trees.transform, new Vector3(x + Random.Range(-3f, 3f), 0, z + Random.Range(-4f, 4f)), treeTrunkMat, treeCanopyMat);
            }
        }

        // Scattered trees in distance
        for (int i = 0; i < 50; i++)
        {
            float x = Random.Range(-300f, 300f);
            float z = Random.Range(100f, 350f);
            CreateTree(trees.transform, new Vector3(x, 0, z), treeTrunkMat, treeCanopyMat);
        }

        // === DISTANT HILLS ===
        GameObject hills = new GameObject("DistantHills");
        hills.transform.parent = environment.transform;
        for (int i = 0; i < 10; i++)
        {
            GameObject hill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hill.name = $"Hill_{i + 1}";
            hill.transform.parent = hills.transform;
            float w = Random.Range(100f, 200f);
            float h = Random.Range(25f, 60f);
            hill.transform.localScale = new Vector3(w, h, 120f);
            hill.transform.position = new Vector3(
                Random.Range(-400f, 400f),
                h / 2f - 8f,
                Random.Range(200f, 400f)
            );
            hill.GetComponent<Renderer>().material = hillMat;
        }

        // === SPAWN POINTS ===
        // Trappers spawn in work area / near boats
        string[] trapperPositions = { "SkinningArea", "ByBoat1", "ByBoat2", "ByCrates" };
        Vector3[] trapperPos = {
            new Vector3(0, 0, -15f),
            new Vector3(-25f, 0, -35f),
            new Vector3(25f, 0, -35f),
            new Vector3(-10f, 0, -30f)
        };
        for (int i = 0; i < 4; i++)
        {
            GameObject spawn = new GameObject($"Spawn_Trapper_{trapperPositions[i]}");
            spawn.transform.parent = spawnPoints.transform;
            spawn.transform.position = trapperPos[i];
        }

        // Natives spawn in treeline
        for (int i = 0; i < 12; i++)
        {
            GameObject spawn = new GameObject($"Spawn_Native_{i + 1:D2}");
            spawn.transform.parent = spawnPoints.transform;
            float x = Random.Range(-100f, 100f);
            float z = treelineZ + Random.Range(5f, 30f);
            spawn.transform.position = new Vector3(x, 0, z);
        }

        // Escape points at keelboats (on shore near gangplanks)
        for (int i = 0; i < keelboatCount; i++)
        {
            GameObject escape = new GameObject($"EscapePoint_Keelboat_{i + 1}");
            escape.transform.parent = spawnPoints.transform;
            escape.transform.position = new Vector3(-25f + i * 50f, shoreY + 1f, shoreZ);
        }

        // === LIGHTING ===
        SetupLighting(lighting);

        Debug.Log("[1620] Revenant-style trapping camp generated");
    }

    static void CreateKeelboat(Transform parent, Vector3 position, Material hullMat, Material deckMat, int index)
    {
        GameObject boat = new GameObject($"Keelboat_{index + 1}");
        boat.transform.parent = parent;
        boat.transform.position = position;
        boat.transform.rotation = Quaternion.Euler(0, Random.Range(-15f, 15f), 0);

        // Hull (elongated box)
        GameObject hull = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hull.name = "Hull";
        hull.transform.parent = boat.transform;
        hull.transform.localPosition = Vector3.zero;
        hull.transform.localScale = new Vector3(5f, 2f, 18f); // ~60ft long, 15ft wide
        hull.GetComponent<Renderer>().material = hullMat;

        // Deck
        GameObject deck = GameObject.CreatePrimitive(PrimitiveType.Cube);
        deck.name = "Deck";
        deck.transform.parent = boat.transform;
        deck.transform.localPosition = new Vector3(0, 1.1f, 0);
        deck.transform.localScale = new Vector3(4.5f, 0.2f, 17f);
        deck.GetComponent<Renderer>().material = deckMat;

        // Cargo boxes on deck
        for (int i = 0; i < 4; i++)
        {
            GameObject cargo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cargo.name = $"Cargo_{i + 1}";
            cargo.transform.parent = boat.transform;
            cargo.transform.localPosition = new Vector3(
                Random.Range(-1.5f, 1.5f),
                1.5f,
                -5f + i * 3f
            );
            cargo.transform.localScale = new Vector3(
                Random.Range(0.8f, 1.2f),
                Random.Range(0.6f, 1f),
                Random.Range(0.8f, 1.2f)
            );
            cargo.GetComponent<Renderer>().material = deckMat;
        }

        // Mast (simple pole)
        GameObject mast = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        mast.name = "Mast";
        mast.transform.parent = boat.transform;
        mast.transform.localPosition = new Vector3(0, 5f, 2f);
        mast.transform.localScale = new Vector3(0.3f, 4f, 0.3f);
        mast.GetComponent<Renderer>().material = deckMat;
    }

    static void CreateLeanTo(Transform parent, Vector3 position, Material canvasMat, Material poleMat, int index)
    {
        GameObject leanto = new GameObject($"LeanTo_{index + 1}");
        leanto.transform.parent = parent;
        leanto.transform.position = position;
        leanto.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);

        // Back poles
        for (int i = 0; i < 2; i++)
        {
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = $"BackPole_{i + 1}";
            pole.transform.parent = leanto.transform;
            pole.transform.localPosition = new Vector3(-1.5f + i * 3f, 1f, -1f);
            pole.transform.localScale = new Vector3(0.1f, 1f, 0.1f);
            pole.GetComponent<Renderer>().material = poleMat;
        }

        // Front poles (taller)
        for (int i = 0; i < 2; i++)
        {
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = $"FrontPole_{i + 1}";
            pole.transform.parent = leanto.transform;
            pole.transform.localPosition = new Vector3(-1.5f + i * 3f, 1.2f, 1.5f);
            pole.transform.localScale = new Vector3(0.1f, 1.2f, 0.1f);
            pole.GetComponent<Renderer>().material = poleMat;
        }

        // Canvas roof (angled)
        GameObject canvas = GameObject.CreatePrimitive(PrimitiveType.Cube);
        canvas.name = "Canvas";
        canvas.transform.parent = leanto.transform;
        canvas.transform.localPosition = new Vector3(0, 2f, 0.25f);
        canvas.transform.localScale = new Vector3(3.5f, 0.1f, 3f);
        canvas.transform.localRotation = Quaternion.Euler(-20f, 0, 0);
        canvas.GetComponent<Renderer>().material = canvasMat;
    }

    static void CreateSkinningFrame(Transform parent, Vector3 position, Material woodMat, Material peltMat, int index)
    {
        GameObject frame = new GameObject($"SkinningFrame_{index + 1}");
        frame.transform.parent = parent;
        frame.transform.position = position;
        frame.transform.rotation = Quaternion.Euler(0, Random.Range(-30f, 30f), 0);

        // Vertical poles
        for (int i = 0; i < 2; i++)
        {
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = $"Pole_{i + 1}";
            pole.transform.parent = frame.transform;
            pole.transform.localPosition = new Vector3(-0.8f + i * 1.6f, 1.2f, 0);
            pole.transform.localScale = new Vector3(0.1f, 1.2f, 0.1f);
            pole.GetComponent<Renderer>().material = woodMat;
        }

        // Crossbar
        GameObject crossbar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        crossbar.name = "Crossbar";
        crossbar.transform.parent = frame.transform;
        crossbar.transform.localPosition = new Vector3(0, 2.2f, 0);
        crossbar.transform.localScale = new Vector3(0.08f, 1f, 0.08f);
        crossbar.transform.localRotation = Quaternion.Euler(0, 0, 90f);
        crossbar.GetComponent<Renderer>().material = woodMat;

        // Stretched pelt
        GameObject pelt = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pelt.name = "StretchedPelt";
        pelt.transform.parent = frame.transform;
        pelt.transform.localPosition = new Vector3(0, 1.3f, 0.05f);
        pelt.transform.localScale = new Vector3(1.2f, 1.5f, 0.05f);
        pelt.GetComponent<Renderer>().material = peltMat;
    }

    static void CreatePeltStack(Transform parent, Vector3 position, Material peltMat, string name)
    {
        GameObject stack = new GameObject(name);
        stack.transform.parent = parent;
        stack.transform.position = position;

        int count = Random.Range(8, 15);
        for (int i = 0; i < count; i++)
        {
            GameObject pelt = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pelt.name = $"Pelt_{i + 1}";
            pelt.transform.parent = stack.transform;
            pelt.transform.localPosition = new Vector3(
                Random.Range(-0.3f, 0.3f),
                i * 0.08f + 0.04f,
                Random.Range(-0.2f, 0.2f)
            );
            pelt.transform.localScale = new Vector3(
                Random.Range(0.6f, 0.9f),
                0.06f,
                Random.Range(0.8f, 1.1f)
            );
            pelt.transform.localRotation = Quaternion.Euler(0, Random.Range(-15f, 15f), 0);
            pelt.GetComponent<Renderer>().material = peltMat;
        }
    }

    static void CreateFirePit(Transform parent, Vector3 position, string name)
    {
        GameObject fire = new GameObject(name);
        fire.transform.parent = parent;
        fire.transform.position = position;

        // Stone ring
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "StoneRing";
        ring.transform.parent = fire.transform;
        ring.transform.localPosition = new Vector3(0, 0.15f, 0);
        ring.transform.localScale = new Vector3(1.5f, 0.15f, 1.5f);
        ring.GetComponent<Renderer>().material = CreateMat($"{name}_Stone", new Color(0.3f, 0.3f, 0.3f));

        // Fire (orange glow)
        GameObject flames = GameObject.CreatePrimitive(PrimitiveType.Cube);
        flames.name = "Flames";
        flames.transform.parent = fire.transform;
        flames.transform.localPosition = new Vector3(0, 0.3f, 0);
        flames.transform.localScale = new Vector3(0.8f, 0.4f, 0.8f);
        flames.GetComponent<Renderer>().material = CreateMat($"{name}_Flame", new Color(0.9f, 0.4f, 0.1f));

        // Light
        GameObject lightObj = new GameObject("FireLight");
        lightObj.transform.parent = fire.transform;
        lightObj.transform.localPosition = new Vector3(0, 1.5f, 0);
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.6f, 0.3f);
        light.intensity = 1.5f;
        light.range = 12f;
    }

    static void CreateTree(Transform parent, Vector3 position, Material trunkMat, Material canopyMat)
    {
        GameObject tree = new GameObject("Tree");
        tree.transform.parent = parent;
        tree.transform.position = position;

        float height = Random.Range(6f, 10f);
        float canopySize = Random.Range(4f, 7f);

        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Trunk";
        trunk.transform.parent = tree.transform;
        trunk.transform.localPosition = new Vector3(0, height / 2f, 0);
        trunk.transform.localScale = new Vector3(0.6f, height / 2f, 0.6f);
        trunk.GetComponent<Renderer>().material = trunkMat;

        GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        canopy.name = "Canopy";
        canopy.transform.parent = tree.transform;
        canopy.transform.localPosition = new Vector3(0, height + canopySize * 0.3f, 0);
        canopy.transform.localScale = new Vector3(canopySize, canopySize * 0.8f, canopySize);
        canopy.GetComponent<Renderer>().material = canopyMat;
    }

    static void SetupLighting(GameObject parent)
    {
        // Find or create sun
        Light sun = null;
        foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (l.type == LightType.Directional)
            {
                sun = l;
                sun.transform.parent = parent.transform;
                break;
            }
        }

        if (sun == null)
        {
            GameObject sunObj = new GameObject("Sun");
            sunObj.transform.parent = parent.transform;
            sun = sunObj.AddComponent<Light>();
            sun.type = LightType.Directional;
        }

        // Early morning, cold light from the east
        sun.transform.rotation = Quaternion.Euler(12f, -45f, 0);
        sun.color = new Color(0.95f, 0.85f, 0.75f);
        sun.intensity = 0.7f;

        // Heavy morning fog
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.012f;
        RenderSettings.fogColor = new Color(0.6f, 0.65f, 0.7f);

        // Cold ambient
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.25f, 0.3f, 0.4f);
        RenderSettings.ambientEquatorColor = new Color(0.35f, 0.3f, 0.35f);
        RenderSettings.ambientGroundColor = new Color(0.15f, 0.1f, 0.08f);
    }

    static void ClearSceneStatic()
    {
        string[] parents = { "Environment", "River", "Camp", "WorkArea", "SpawnPoints", "Lighting" };
        foreach (string name in parents)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null) Object.DestroyImmediate(obj);
        }
    }

    static Material CreateMat(string name, Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        Material mat = new Material(shader);
        mat.name = name;
        mat.SetColor("_BaseColor", color);

        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        string path = $"Assets/Materials/{name}.mat";

        // Check if material already exists
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            existing.SetColor("_BaseColor", color);
            return existing;
        }

        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    void OnGUI()
    {
        GUILayout.Label("The Revenant - Trapping Camp", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("Scene Settings", EditorStyles.boldLabel);
        sceneWidth = EditorGUILayout.FloatField("Scene Width (m)", sceneWidth);
        sceneDepth = EditorGUILayout.FloatField("Scene Depth (m)", sceneDepth);
        riverWidth = EditorGUILayout.FloatField("River Width (m)", riverWidth);

        GUILayout.Space(10);
        GUILayout.Label("Camp Elements", EditorStyles.boldLabel);
        keelboatCount = EditorGUILayout.IntSlider("Keelboats", keelboatCount, 1, 3);
        leantoCount = EditorGUILayout.IntSlider("Lean-tos", leantoCount, 3, 8);
        skinningFrameCount = EditorGUILayout.IntSlider("Skinning Frames", skinningFrameCount, 4, 10);

        GUILayout.Space(20);

        if (GUILayout.Button("Generate Scene", GUILayout.Height(40)))
        {
            GenerateSceneStatic();
            CreatePlayerController();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Clear Scene"))
        {
            if (EditorUtility.DisplayDialog("Clear Scene",
                "This will delete all graybox objects. Continue?", "Yes", "No"))
            {
                ClearSceneStatic();
            }
        }
    }
}
