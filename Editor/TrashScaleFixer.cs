using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Trash Scale Fixer — sets ALL trash to balanced, gameplay-visible proportions.
///
/// Reference measurements:
/// - Player: ~1.8m tall (scale 2.2)
/// - Recycling bins: ~1m tall (top at Y ≈ 1.08)
/// - Target: trash items 0.3-0.6m (clearly visible, hand-holdable by player)
///
/// The FBX models are natively ~10-20 Unity units tall (modeled in cm).
/// Scale factor 0.04-0.06 converts to ~0.4-0.6m gameplay height.
///
/// Run via: Tools → Fix Trash Scales
/// </summary>
public static class TrashScaleFixer
{
    // ═══════════════════════════════════════════
    // PROFESSIONALLY CALIBRATED SCALES
    // ═══════════════════════════════════════════
    // Calibrated against: recycling bins (~1m), player (~1.8m)
    // Style: cartoon-slightly-exaggerated for gameplay readability
    //
    // Bottles: ~0.4-0.5m (forearm-length, clearly visible)
    // Cans:    ~0.2-0.25m (handheld, easy to spot)
    // Boxes:   ~0.5-0.6m (recognizable pizza box size)
    // Paper:   ~0.3-0.4m (readable from medium distance)

    private static readonly Dictionary<string, Vector3> trashScales = new Dictionary<string, Vector3>()
    {
        // ── Plastic (bottles) ──
        // Soda/water bottles: ~40-50cm — clearly visible beside bins
        { "Assets/Trash/Plastic/Prefabs/TR_SodaBottle_01.prefab",    Vec(0.045f) },
        { "Assets/Trash/Plastic/Prefabs/TR_PlasticBottle_01.prefab", Vec(0.042f) },

        // ── Paper ──
        // Pizza box: ~50cm — recognizable flat box
        { "Assets/Trash/Paper/Prefabs/TR_PizzaBox_01.prefab",        Vec(0.048f) },
        // Paper cup: ~20-25cm — small but readable
        { "Assets/Trash/Paper/Prefabs/TR_PaperCup_01.prefab",        Vec(0.055f) },
        // Paper sheet: ~35cm — visible on ground
        { "Assets/Trash/Paper/Prefabs/TR_PaperSheet_01.prefab",      Vec(0.050f) },
        // Newspaper stack: ~35cm — compact readable stack
        { "Assets/Trash/Paper/Prefabs/TR_NewspaperStack_01.prefab",  Vec(0.040f) },

        // ── Glass (bottles) ──
        // Glass bottles: ~40cm — wine/glass bottle size
        { "Assets/Trash/Glass/Prefabs/TR_GlassBottle_01.prefab",     Vec(0.042f) },
        { "Assets/Trash/Glass/Prefabs/TR_GlassBottle_02.prefab",     Vec(0.044f) },

        // ── Metal (cans) ──
        // Regular cans: ~20-25cm — clearly handheld
        { "Assets/Trash/Metal/Prefabs/TR_MetalCan_01.prefab",        Vec(0.055f) },
        { "Assets/Trash/Metal/Prefabs/TR_MetalCan_02.prefab",        Vec(0.055f) },
        { "Assets/Trash/Metal/Prefabs/TR_MetalCan_03.prefab",        Vec(0.055f) },
        { "Assets/Trash/Metal/Prefabs/TR_MetalCan_04.prefab",        Vec(0.055f) },
        // Food cans: slightly larger than soda cans
        { "Assets/Trash/Metal/Prefabs/TR_FoodCan_01.prefab",         Vec(0.050f) },
        { "Assets/Trash/Metal/Prefabs/TR_FoodCan_02.prefab",         Vec(0.050f) },
        { "Assets/Trash/Metal/Prefabs/TR_FoodCan_03.prefab",         Vec(0.050f) },
        // Aluminum cans
        { "Assets/Trash/Metal/Prefabs/TR_AluminumCan_01.prefab",     Vec(0.052f) },
        { "Assets/Trash/Metal/Prefabs/TR_AluminumCan_02.prefab",     Vec(0.052f) },
    };

    /// <summary>Helper to create uniform Vector3 scale.</summary>
    private static Vector3 Vec(float s) => new Vector3(s, s, s);

    [MenuItem("Tools/Fix Trash Scales")]
    public static void FixTrashScales()
    {
        Debug.Log("═══════════════════════════════════════════════════════");
        Debug.Log("  TRASH SCALE FIX — Balanced Gameplay Visibility");
        Debug.Log("═══════════════════════════════════════════════════════");

        // ══════════════════════════════════════
        // STEP 1: Fix all prefab scales
        // ══════════════════════════════════════
        int prefabsFixed = 0;
        foreach (var entry in trashScales)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(entry.Key);
            if (prefab != null)
            {
                prefab.transform.localScale = entry.Value;
                EditorUtility.SetDirty(prefab);
                prefabsFixed++;
                Debug.Log($"  Prefab: {prefab.name} → scale {entry.Value.x:F3}");
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[Step 1] ✓ Fixed {prefabsFixed} prefab scales.");

        // ══════════════════════════════════════
        // STEP 2: Fix ALL scene trash instances
        // ══════════════════════════════════════
        TrashItem[] sceneTrash = Object.FindObjectsByType<TrashItem>(FindObjectsSortMode.None);
        int instancesFixed = 0;

        foreach (TrashItem trash in sceneTrash)
        {
            GameObject prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(trash.gameObject);
            if (prefabSource != null)
            {
                string prefabPath = AssetDatabase.GetAssetPath(prefabSource);
                if (trashScales.ContainsKey(prefabPath))
                {
                    Vector3 targetScale = trashScales[prefabPath];
                    float variation = Random.Range(0.92f, 1.08f);

                    Undo.RecordObject(trash.transform, "Fix Trash Instance Scale");
                    trash.transform.localScale = targetScale * variation;
                    EditorUtility.SetDirty(trash.gameObject);
                    instancesFixed++;
                }
            }
            else
            {
                // Match by name
                string itemName = trash.gameObject.name.Replace("(Clone)", "").Trim();
                foreach (var entry in trashScales)
                {
                    if (entry.Key.Contains(itemName))
                    {
                        float variation = Random.Range(0.92f, 1.08f);
                        Undo.RecordObject(trash.transform, "Fix Trash Instance Scale");
                        trash.transform.localScale = entry.Value * variation;
                        EditorUtility.SetDirty(trash.gameObject);
                        instancesFixed++;
                        break;
                    }
                }
            }
        }
        Debug.Log($"[Step 2] ✓ Fixed {instancesFixed}/{sceneTrash.Length} scene instances.");

        // ══════════════════════════════════════
        // STEP 3: Ground-snap floating items
        // ══════════════════════════════════════
        int groundSnapped = 0;
        foreach (TrashItem trash in sceneTrash)
        {
            Vector3 pos = trash.transform.position;
            if (Physics.Raycast(pos + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 50f))
            {
                if (Mathf.Abs(pos.y - hit.point.y) > 0.3f)
                {
                    Undo.RecordObject(trash.transform, "Ground Snap Trash");
                    trash.transform.position = new Vector3(pos.x, hit.point.y + 0.02f, pos.z);
                    EditorUtility.SetDirty(trash.gameObject);
                    groundSnapped++;
                }
            }
        }
        Debug.Log($"[Step 3] ✓ Ground-snapped {groundSnapped} floating items.");

        // ══════════════════════════════════════
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("═══════════════════════════════════════════════════════");
        Debug.Log("  TRASH SCALE FIX — Complete!");
        Debug.Log($"  Prefabs: {prefabsFixed} | Instances: {instancesFixed} | Ground-snapped: {groundSnapped}");
        Debug.Log("  Reference: bins ~1m, player ~1.8m, trash ~0.3-0.6m");
        Debug.Log("═══════════════════════════════════════════════════════");
    }
}
