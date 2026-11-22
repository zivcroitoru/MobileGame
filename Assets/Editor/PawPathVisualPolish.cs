using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// A reversible scene-only visual pass for the existing Paw Path Level.
/// The original physical platforms, their colliders, the cat and the camera
/// controller are never moved, scaled, deleted or reconfigured.
/// </summary>
public static class PawPathVisualPolish
{
    private const string AutoRootName = "PawPath_AutoVisuals";
    private const string EnvironmentRootName = "Environment_Visuals";
    private const string GeneratedMaterialsFolder = "Assets/KayKit/Platformer/PawPathGenerated";

    private static readonly string[] PlatformNames =
    {
        "Launch Pad", "Gap Recovery Floor", "Landing Platform",
        "Upper Step", "Goal Platform", "Overhang"
    };

    private static readonly string[] WallNames =
    {
        "Left Wall", "Finish Wall", "Course Ceiling"
    };

    [MenuItem("Tools/Paw Path/Apply KayKit Visual Polish")]
    private static void Apply()
    {
        if (!ValidateScene()) return;

        Camera camera = Camera.main != null ? Camera.main : UnityEngine.Object.FindFirstObjectByType<Camera>();
        if (camera == null)
        {
            EditorUtility.DisplayDialog("Paw Path", "No camera found in the Level scene.", "OK");
            return;
        }

        GameObject greenModel = FindKayKitModel("green");
        GameObject neutralModel = FindKayKitModel("neutral") ?? FindKayKitModel("blue");
        if (greenModel == null && neutralModel == null)
        {
            EditorUtility.DisplayDialog("Paw Path",
                "KayKit models were not found. Import the Platformer Pack into Assets/KayKit/Platformer, including fbx(unity).", "OK");
            return;
        }

        GameObject environmentRoot = GameObject.Find(EnvironmentRootName);
        if (environmentRoot == null)
        {
            environmentRoot = new GameObject(EnvironmentRootName);
            Undo.RegisterCreatedObjectUndo(environmentRoot, "Create environment root");
        }

        // A single manually dropped model, as in the user's screenshot, would
        // overlap the new automatic layout. Ask before hiding manually made work.
        List<GameObject> manualModels = new List<GameObject>();
        foreach (Transform child in environmentRoot.transform)
        {
            if (child.name == AutoRootName) continue;
            if (child.gameObject.activeSelf && child.GetComponentsInChildren<MeshRenderer>(true).Length > 0)
                manualModels.Add(child.gameObject);
        }

        if (manualModels.Count > 0)
        {
            bool hide = EditorUtility.DisplayDialog("Existing environment models",
                $"Found {manualModels.Count} existing visual object(s) under {EnvironmentRootName}. " +
                "Hide them (reversible with Undo) to prevent overlapping the generated layout?",
                "Hide and Apply", "Cancel");
            if (!hide) return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Apply Paw Path KayKit Visual Polish");

        foreach (GameObject model in manualModels)
        {
            Undo.RecordObject(model, "Hide old manual model");
            model.SetActive(false);
        }

        Transform old = environmentRoot.transform.Find(AutoRootName);
        if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

        GameObject generated = CreateObject(AutoRootName, environmentRoot.transform);
        Transform platformParent = CreateObject("KayKit Platforms (visual only)", generated.transform).transform;

        int convertedCount = 0;
        foreach (string name in PlatformNames)
            if (CoverColliderWithModel(name, greenModel ?? neutralModel, platformParent, false))
                convertedCount++;

        foreach (string name in WallNames)
            if (CoverColliderWithModel(name, neutralModel ?? greenModel, platformParent, true))
                convertedCount++;

        AddBackground(generated.transform, camera.transform);
        AddGoalAccents(generated.transform);
        ImproveLighting(camera);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Undo.CollapseUndoOperations(undoGroup);
        Selection.activeGameObject = generated;

        Debug.Log($"Paw Path: styled {convertedCount} collider shapes with KayKit models. " +
                  "Physics and camera controls were left untouched.");
        EditorUtility.DisplayDialog("Paw Path visual polish applied",
            $"Styled {convertedCount} level pieces with KayKit, added layered hills/clouds and tuned lighting.\n\n" +
            "Original BoxColliders remain active. Test in Game view and save the scene (Ctrl+S). " +
            "Undo (Ctrl+Z) or Tools > Paw Path > Remove Generated Visuals to revert visuals.", "OK");
    }

    [MenuItem("Tools/Paw Path/Remove Generated Visuals")]
    private static void Remove()
    {
        if (!ValidateScene()) return;

        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Remove Paw Path Visual Polish");

        GameObject environment = GameObject.Find(EnvironmentRootName);
        Transform generated = environment != null ? environment.transform.Find(AutoRootName) : null;
        if (generated != null) Undo.DestroyObjectImmediate(generated.gameObject);

        // Makes the original greybox visible again. Colliders are unchanged.
        foreach (string name in PlatformNames.Concat(WallNames))
        {
            GameObject original = GameObject.Find(name);
            if (original == null) continue;
            MeshRenderer renderer = original.GetComponent<MeshRenderer>();
            if (renderer == null) continue;
            Undo.RecordObject(renderer, "Show original greybox");
            renderer.enabled = true;
        }

        // Restore manual models hidden by Apply (if any). No objects deleted.
        if (environment != null)
        {
            foreach (Transform child in environment.transform)
            {
                if (child.name == AutoRootName) continue;
                if (child.name.StartsWith("platform_", StringComparison.OrdinalIgnoreCase) &&
                    !child.gameObject.activeSelf)
                {
                    Undo.RecordObject(child.gameObject, "Show manual platform");
                    child.gameObject.SetActive(true);
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Undo.CollapseUndoOperations(group);
        Debug.Log("Paw Path: removed generated visuals. Original collider meshes visible again.");
    }

    private static bool ValidateScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Paw Path", "Exit Play Mode before applying scene changes.", "OK");
            return false;
        }
        if (EditorSceneManager.GetActiveScene().name != "Level")
        {
            EditorUtility.DisplayDialog("Paw Path", "Open Assets/Scenes/Level.unity first.", "OK");
            return false;
        }
        return true;
    }

    private static GameObject FindKayKitModel(string color)
    {
        // Prefer the Unity-axis-ready FBX files in the screenshot's folder.
        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { "Assets/KayKit/Platformer" });
        var candidates = guids.Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => p.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase) &&
                        p.IndexOf("/fbx(unity)/" + color + "/", StringComparison.OrdinalIgnoreCase) >= 0 &&
                        System.IO.Path.GetFileName(p).StartsWith("platform_", StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.Contains("6x2x1") ? 0 : p.Contains("6x2") ? 1 : 2)
            .ThenBy(p => p, StringComparer.OrdinalIgnoreCase);

        foreach (string path in candidates)
        {
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset != null) return asset;
        }
        return null;
    }

    private static bool CoverColliderWithModel(string objectName, GameObject model,
        Transform parent, bool rotate90)
    {
        GameObject original = GameObject.Find(objectName);
        if (original == null || model == null) return false;
        BoxCollider collider = original.GetComponent<BoxCollider>();
        if (collider == null) return false;

        GameObject holder = CreateObject("Visual - " + objectName, parent);
        holder.transform.position = Vector3.zero;
        holder.transform.rotation = Quaternion.identity;
        holder.transform.localScale = Vector3.one;

        GameObject instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
        if (instance == null)
        {
            Undo.DestroyObjectImmediate(holder);
            return false;
        }
        Undo.RegisterCreatedObjectUndo(instance, "Place KayKit model");
        instance.transform.SetParent(holder.transform, false);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.Euler(0, 0, rotate90 ? 90f : 0f);
        instance.transform.localScale = Vector3.one;

        // Imported prefab geometry is decorative only: the existing BoxCollider
        // on the original object remains the *sole* gameplay collision surface.
        foreach (Collider importedCollider in instance.GetComponentsInChildren<Collider>(true))
        {
            Undo.RecordObject(importedCollider, "Disable decorative collider");
            importedCollider.enabled = false;
        }

        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            Undo.DestroyObjectImmediate(holder);
            return false;
        }

        Bounds current = CombinedBounds(renderers);
        Bounds target = collider.bounds;
        Vector3 size = current.size;
        holder.transform.localScale = new Vector3(
            target.size.x / Mathf.Max(0.001f, size.x),
            target.size.y / Mathf.Max(0.001f, size.y),
            target.size.z / Mathf.Max(0.001f, size.z));

        // Fit by rendered bounds, not by an assumed KayKit pivot/FBX orientation.
        current = CombinedBounds(renderers);
        holder.transform.position += target.center - current.center;

        MeshRenderer originalRenderer = original.GetComponent<MeshRenderer>();
        if (originalRenderer != null)
        {
            Undo.RecordObject(originalRenderer, "Hide original collision mesh");
            originalRenderer.enabled = false;
        }
        return true;
    }

    private static Bounds CombinedBounds(Renderer[] renderers)
    {
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    private static GameObject CreateObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create visual " + name);
        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        return go;
    }

    private static void AddBackground(Transform parent, Transform camera)
    {
        Material farMaterial = MaterialFor("FarHills", new Color(0.80f, 0.86f, 0.98f));
        Material middleMaterial = MaterialFor("MidHills", new Color(0.66f, 0.80f, 0.82f));
        Material nearMaterial = MaterialFor("NearHills", new Color(0.51f, 0.71f, 0.69f));
        Material cloudMaterial = MaterialFor("Clouds", new Color(0.98f, 0.98f, 1.00f));
        Material sunMaterial = MaterialFor("Sun", new Color(1.00f, 0.90f, 0.64f));

        // Camera is at Z=-10 looking toward the game at Z=0.
        // Decorations behind the course sit at positive Z and have no colliders.
        Transform far = MakeLayer("Far mountains", parent, camera, 0.86f, 0.72f);
        Transform mid = MakeLayer("Middle hills", parent, camera, 0.65f, 0.52f);
        Transform near = MakeLayer("Nearby hills", parent, camera, 0.45f, 0.35f);
        Transform sky = MakeLayer("Clouds and sun", parent, camera, 0.93f, 0.83f);

        for (int i = 0; i < 9; i++)
        {
            float x = -11f + i * 3.7f;
            MakeSphere("Far hill " + i, far,
                new Vector3(x, -2.25f + (i % 3) * 0.25f, 7f),
                new Vector3(5.5f, 4.1f, 0.3f), farMaterial);
        }
        for (int i = 0; i < 10; i++)
        {
            float x = -10f + i * 3.5f;
            MakeSphere("Middle hill " + i, mid,
                new Vector3(x, -3.0f + (i % 2) * 0.38f, 5.1f),
                new Vector3(4.9f, 3.9f, 0.35f), middleMaterial);
        }
        for (int i = 0; i < 11; i++)
        {
            float x = -10.5f + i * 3.15f;
            MakeSphere("Near hill " + i, near,
                new Vector3(x, -3.95f + (i % 3) * 0.24f, 3.3f),
                new Vector3(4.4f, 3.7f, 0.36f), nearMaterial);
        }
        MakeSphere("Warm sun", sky, new Vector3(-1.8f, 4.35f, 8f),
            new Vector3(1.1f, 1.1f, 0.15f), sunMaterial);

        for (int i = 0; i < 6; i++)
        {
            float x = -7.5f + i * 5.0f;
            float y = 3.3f + (i % 3) * 0.52f;
            MakeSphere("Cloud " + i + "a", sky, new Vector3(x, y, 7.5f),
                new Vector3(1.5f, 0.37f, 0.23f), cloudMaterial);
            MakeSphere("Cloud " + i + "b", sky, new Vector3(x + 0.40f, y + 0.14f, 7.5f),
                new Vector3(0.95f, 0.55f, 0.24f), cloudMaterial);
        }
    }

    private static Transform MakeLayer(string name, Transform parent,
        Transform camera, float xFollow, float yFollow)
    {
        GameObject go = CreateObject(name, parent);
        PawPathParallax parallax = Undo.AddComponent<PawPathParallax>(go);
        parallax.Configure(camera, xFollow, yFollow);
        return go.transform;
    }

    private static void MakeSphere(string name, Transform parent,
        Vector3 position, Vector3 size, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Undo.RegisterCreatedObjectUndo(go, "Add scenery " + name);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.transform.localScale = size;
        Collider collider = go.GetComponent<Collider>();
        if (collider != null) Undo.DestroyObjectImmediate(collider);
        Renderer renderer = go.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }

    private static void AddGoalAccents(Transform parent)
    {
        GameObject goal = GameObject.Find("Goal - Green Marker");
        if (goal == null) return;

        Material gold = MaterialFor("GoalSparkles", new Color(1.0f, 0.78f, 0.31f));
        Transform root = CreateObject("Goal sparkles (decorative)", parent).transform;
        Vector3 pos = goal.transform.position;
        for (int i = 0; i < 6; i++)
        {
            float t = i * Mathf.PI * 2f / 6f;
            MakeSphere("Sparkle " + i, root,
                new Vector3(pos.x + Mathf.Cos(t) * 0.65f,
                            pos.y + Mathf.Sin(t) * 0.83f, -0.75f),
                new Vector3(0.09f, 0.09f, 0.09f), gold);
        }
    }

    private static Material MaterialFor(string key, Color color)
    {
        EnsureGeneratedMaterialsFolder();
        string path = GeneratedMaterialsFolder + "/" + key + ".mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader);
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void EnsureGeneratedMaterialsFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/KayKit"))
            AssetDatabase.CreateFolder("Assets", "KayKit");
        if (!AssetDatabase.IsValidFolder("Assets/KayKit/Platformer"))
            AssetDatabase.CreateFolder("Assets/KayKit", "Platformer");
        if (!AssetDatabase.IsValidFolder(GeneratedMaterialsFolder))
            AssetDatabase.CreateFolder("Assets/KayKit/Platformer", "PawPathGenerated");
    }

    private static void ImproveLighting(Camera camera)
    {
        Undo.RecordObject(camera, "Change Paw Path sky color");
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.72f, 0.86f, 0.98f, 1f);
        // Keep the existing side-on orthographic camera orientation untouched.

        Light sun = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None)
            .FirstOrDefault(l => l.type == LightType.Directional);
        if (sun == null) return;

        Undo.RecordObject(sun, "Polish Paw Path key light");
        Undo.RecordObject(sun.transform, "Aim Paw Path key light");
        sun.transform.rotation = Quaternion.Euler(38f, -30f, 0f);
        sun.color = new Color(1f, 0.95f, 0.88f);
        sun.intensity = 1.15f;
        sun.shadows = LightShadows.Soft;
    }
}
