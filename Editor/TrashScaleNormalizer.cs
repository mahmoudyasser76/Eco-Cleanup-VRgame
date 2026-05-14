using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Editor tool that measures actual mesh bounds of every trash prefab,
/// normalizes their root transform scales to achieve consistent world-space
/// dimensions, and rebuilds colliders to match.
/// 
/// Run via menu: Tools > Normalize Trash Scales
/// </summary>
public class TrashScaleNormalizer : Editor
{
    // ─── Target world-space MAX dimension for each trash prefab ───
    // These represent the largest axis of the final rendered object.
    // Slightly exaggerated for gameplay readability (player is ~1.8m tall).
    static readonly Dictionary<string, float> targetMaxDimensions = new Dictionary<string, float>()
    {
        // ── Plastic Bottles ──
        { "TR_PlasticBottle_01", 0.35f },   // Water bottle
        { "TR_SodaBottle_01",    0.35f },   // Soda bottle (Unity primitive)

        // ── Glass Bottles ──
        { "TR_GlassBottle_01",   0.35f },   // Glass bottle
        { "TR_GlassBottle_02",   0.38f },   // Wine bottle (slightly taller)

        // ── Metal Cans ──
        { "TR_MetalCan_01",      0.18f },   // Soda can
        { "TR_MetalCan_02",      0.18f },
        { "TR_MetalCan_03",      0.18f },
        { "TR_MetalCan_04",      0.18f },
        { "TR_AluminumCan_01",   0.16f },   // Aluminum can
        { "TR_AluminumCan_02",   0.16f },

        // ── Food Cans ──
        { "TR_FoodCan_01",       0.14f },   // Food can (shorter)
        { "TR_FoodCan_02",       0.14f },
        { "TR_FoodCan_03",       0.14f },

        // ── Paper Items ──
        { "TR_NewspaperStack_01", 0.30f },  // Newspaper stack (wide)
        { "TR_PaperCup_01",      0.14f },   // Paper cup (Unity primitive)
        { "TR_PaperSheet_01",    0.35f },   // Paper sheet (wide, flat)
        { "TR_PizzaBox_01",      0.40f },   // Pizza box (wide, flat)
    };

    [MenuItem("Tools/Normalize Trash Scales")]
    static void NormalizeTrashScales()
    {
        string[] trashDirs = {
            "Assets/Trash/Glass/Prefabs",
            "Assets/Trash/Metal/Prefabs",
            "Assets/Trash/Paper/Prefabs",
            "Assets/Trash/Plastic/Prefabs"
        };

        int processed = 0;
        int errors = 0;

        foreach (string dir in trashDirs)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { dir });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (ProcessPrefab(path))
                    processed++;
                else
                    errors++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[TrashScaleNormalizer] Complete! Processed {processed} prefabs, {errors} errors.");
    }

    static bool ProcessPrefab(string prefabPath)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError($"[TrashScaleNormalizer] Failed to load: {prefabPath}");
            return false;
        }

        string prefabName = prefab.name;

        // Get target dimension
        float targetDim;
        if (!targetMaxDimensions.TryGetValue(prefabName, out targetDim))
        {
            Debug.LogWarning($"[TrashScaleNormalizer] No target defined for '{prefabName}', using 0.20m");
            targetDim = 0.20f;
        }

        // Step 1: Instantiate prefab at CURRENT scale to measure current bounds
        GameObject currentInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        currentInstance.transform.position = Vector3.zero;
        Bounds currentBounds = GetTotalRendererBounds(currentInstance);
        Vector3 currentScale = currentInstance.transform.localScale;

        if (currentBounds.size == Vector3.zero)
        {
            Debug.LogWarning($"[TrashScaleNormalizer] No renderers on '{prefabName}', skipping.");
            DestroyImmediate(currentInstance);
            return false;
        }

        // Step 2: Instantiate at scale (1,1,1) to measure raw model bounds
        GameObject rawInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        rawInstance.transform.position = new Vector3(100, 0, 0); // away from origin
        rawInstance.transform.localScale = Vector3.one;
        Bounds rawBounds = GetTotalRendererBounds(rawInstance);

        float rawMaxDim = Mathf.Max(rawBounds.size.x, rawBounds.size.y, rawBounds.size.z);
        float currentMaxDim = Mathf.Max(currentBounds.size.x, currentBounds.size.y, currentBounds.size.z);

        DestroyImmediate(currentInstance);
        DestroyImmediate(rawInstance);

        if (rawMaxDim < 0.0001f)
        {
            Debug.LogWarning($"[TrashScaleNormalizer] Raw bounds too small for '{prefabName}', skipping.");
            return false;
        }

        // Step 3: Calculate the correct uniform scale
        float newUniformScale = targetDim / rawMaxDim;

        Debug.Log($"[TrashScaleNormalizer] {prefabName}:\n" +
                  $"  Current scale: ({currentScale.x:F4}, {currentScale.y:F4}, {currentScale.z:F4})\n" +
                  $"  Current world size: ({currentBounds.size.x:F3}, {currentBounds.size.y:F3}, {currentBounds.size.z:F3})m, max={currentMaxDim:F3}m\n" +
                  $"  Raw model size at (1,1,1): ({rawBounds.size.x:F3}, {rawBounds.size.y:F3}, {rawBounds.size.z:F3})m, max={rawMaxDim:F3}m\n" +
                  $"  Target max dimension: {targetDim:F3}m\n" +
                  $"  NEW uniform scale: {newUniformScale:F6}");

        // Step 4: Apply the new scale to the prefab
        // Handle both regular prefabs and model prefab variants
        bool isPrefabVariant = PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.Variant ||
                               PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.Model;

        try
        {
            using (var editScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                GameObject root = editScope.prefabContentsRoot;

                // Set normalized scale
                root.transform.localScale = new Vector3(newUniformScale, newUniformScale, newUniformScale);

                // Keep a small Y offset so items sit on ground properly
                // Measure bounds at new scale to find bottom
                Bounds editBounds = GetTotalRendererBoundsLocal(root);
                if (editBounds.size != Vector3.zero)
                {
                    // Offset Y so that the bottom of the mesh sits at Y=0
                    float bottomY = editBounds.min.y;
                    if (bottomY < -0.001f)
                    {
                        Vector3 pos = root.transform.localPosition;
                        pos.y = -bottomY;
                        root.transform.localPosition = pos;
                    }
                    else
                    {
                        root.transform.localPosition = new Vector3(0, 0, 0);
                    }
                }

                // Step 5: Fix colliders
                RebuildColliders(root);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TrashScaleNormalizer] Error editing '{prefabName}': {e.Message}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the combined world-space bounds of all renderers on a scene instance.
    /// </summary>
    static Bounds GetTotalRendererBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds();

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }

    /// <summary>
    /// Gets combined bounds from mesh filters in the prefab editing context.
    /// Since we're in prefab edit mode, renderer.bounds won't work,
    /// so we calculate from MeshFilter + transforms manually.
    /// </summary>
    static Bounds GetTotalRendererBoundsLocal(GameObject root)
    {
        MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>();
        if (meshFilters.Length == 0) return new Bounds();

        bool first = true;
        Bounds totalBounds = new Bounds();

        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.sharedMesh == null) continue;

            Bounds meshBounds = mf.sharedMesh.bounds;
            // Transform the 8 corners of the mesh bounds to root space
            Transform t = mf.transform;
            Vector3[] corners = new Vector3[8];
            Vector3 min = meshBounds.min;
            Vector3 max = meshBounds.max;
            corners[0] = new Vector3(min.x, min.y, min.z);
            corners[1] = new Vector3(min.x, min.y, max.z);
            corners[2] = new Vector3(min.x, max.y, min.z);
            corners[3] = new Vector3(min.x, max.y, max.z);
            corners[4] = new Vector3(max.x, min.y, min.z);
            corners[5] = new Vector3(max.x, min.y, max.z);
            corners[6] = new Vector3(max.x, max.y, min.z);
            corners[7] = new Vector3(max.x, max.y, max.z);

            foreach (Vector3 c in corners)
            {
                Vector3 worldPoint = t.TransformPoint(c);
                if (first)
                {
                    totalBounds = new Bounds(worldPoint, Vector3.zero);
                    first = false;
                }
                else
                {
                    totalBounds.Encapsulate(worldPoint);
                }
            }
        }

        return totalBounds;
    }

    /// <summary>
    /// Removes all existing colliders and adds a properly-fitted collider
    /// based on the current mesh bounds.
    /// </summary>
    static void RebuildColliders(GameObject root)
    {
        // Remove all existing colliders
        Collider[] existingColliders = root.GetComponentsInChildren<Collider>();
        foreach (var col in existingColliders)
        {
            DestroyImmediate(col);
        }

        // Measure bounds from mesh filters
        Bounds bounds = GetTotalRendererBoundsLocal(root);
        if (bounds.size == Vector3.zero) return;

        // Convert bounds to root local space by inverting the root transform
        Vector3 localCenter = root.transform.InverseTransformPoint(bounds.center);
        Vector3 worldSize = bounds.size;
        Vector3 localSize = new Vector3(
            worldSize.x / Mathf.Abs(root.transform.lossyScale.x),
            worldSize.y / Mathf.Abs(root.transform.lossyScale.y),
            worldSize.z / Mathf.Abs(root.transform.lossyScale.z)
        );

        // Decide collider type based on aspect ratio
        float horizontalMax = Mathf.Max(localSize.x, localSize.z);
        float aspectRatio = horizontalMax > 0.001f ? localSize.y / horizontalMax : 1f;

        if (aspectRatio > 1.3f)
        {
            // Tall item → CapsuleCollider (bottles, cans)
            CapsuleCollider capsule = root.AddComponent<CapsuleCollider>();
            capsule.center = localCenter;
            capsule.height = localSize.y;
            capsule.radius = horizontalMax * 0.5f;
            capsule.direction = 1; // Y-axis
        }
        else
        {
            // Wide/flat item → BoxCollider (pizza box, paper, newspaper)
            BoxCollider box = root.AddComponent<BoxCollider>();
            box.center = localCenter;
            box.size = localSize;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // SCENE INSTANCE FIXER
    // ═══════════════════════════════════════════════════════════

    [MenuItem("Tools/Fix Scene Trash Instances")]
    static void FixSceneInstances()
    {
        TrashItem[] sceneTrash = Object.FindObjectsByType<TrashItem>(FindObjectsSortMode.None);
        if (sceneTrash.Length == 0)
        {
            Debug.LogWarning("[TrashScaleNormalizer] No TrashItem instances found in scene.");
            return;
        }

        int fixed_count = 0;
        int snapped = 0;

        foreach (TrashItem trash in sceneTrash)
        {
            GameObject go = trash.gameObject;

            // Revert scale property overrides so prefab scale takes effect
            if (PrefabUtility.IsPartOfPrefabInstance(go))
            {
                SerializedObject so = new SerializedObject(go.transform);

                // Revert localScale overrides
                PrefabUtility.RevertPropertyOverride(
                    so.FindProperty("m_LocalScale"),
                    InteractionMode.AutomatedAction
                );

                // Apply slight random variation (±8%) for visual variety
                float variation = Random.Range(0.92f, 1.08f);
                Vector3 prefabScale = go.transform.localScale;
                Undo.RecordObject(go.transform, "Fix Trash Instance Scale");
                go.transform.localScale = prefabScale * variation;
                EditorUtility.SetDirty(go);
                fixed_count++;
            }
            else
            {
                // Non-prefab instance: match by name
                string itemName = go.name.Replace("(Clone)", "").Trim();
                // Strip trailing numbers from distributed copies like "TR_PlasticBottle_01 (1)"
                int parenIdx = itemName.IndexOf('(');
                if (parenIdx > 0) itemName = itemName.Substring(0, parenIdx).Trim();

                if (targetMaxDimensions.ContainsKey(itemName))
                {
                    // We know the prefab was already fixed, measure its actual raw size
                    // and apply the same normalization
                    Debug.Log($"[TrashScaleNormalizer] Non-prefab instance '{go.name}' — keeping current scale.");
                }
                fixed_count++;
            }

            // Ground-snap: raycast down to find ground
            Vector3 pos = go.transform.position;
            if (Physics.Raycast(pos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 50f))
            {
                float groundY = hit.point.y + 0.02f; // slight offset above ground
                if (Mathf.Abs(pos.y - groundY) > 0.3f)
                {
                    Undo.RecordObject(go.transform, "Ground Snap Trash");
                    go.transform.position = new Vector3(pos.x, groundY, pos.z);
                    EditorUtility.SetDirty(go);
                    snapped++;
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log($"[TrashScaleNormalizer] Scene fix complete!\n" +
                  $"  Instances processed: {fixed_count}/{sceneTrash.Length}\n" +
                  $"  Ground-snapped: {snapped}");
    }

    // ═══════════════════════════════════════════════════════════
    // COMBINED: Fix everything in one click
    // ═══════════════════════════════════════════════════════════

    [MenuItem("Tools/Fix ALL Trash (Prefabs + Scene)")]
    static void FixAllTrashScales()
    {
        Debug.Log("═══════════════════════════════════════════════════");
        Debug.Log("  COMPLETE TRASH SCALE FIX — Starting...");
        Debug.Log("═══════════════════════════════════════════════════");

        // Step 1: Normalize all prefabs
        NormalizeTrashScales();

        // Step 2: Fix all scene instances
        FixSceneInstances();

        // Step 3: Save
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("═══════════════════════════════════════════════════");
        Debug.Log("  COMPLETE TRASH SCALE FIX — Done!");
        Debug.Log("═══════════════════════════════════════════════════");
    }
}
