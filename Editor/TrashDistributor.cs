using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Professional Trash Distribution System — Step 10
/// Distributes trash naturally throughout the city with density-based zones,
/// cluster spawning, ground snapping, and natural rotation variance.
///
/// Design:
/// - 3 concentric density zones: Plaza Core → Nearby Streets → City Edges
/// - Cluster spawning: 30% chance of 2-4 item clusters for realism
/// - Ground-only placement via downward raycasting (rejects rooftops/walls)
/// - Natural rotation with tilt variance
/// - Uses correct prefab scales from TrashScaleFixer (0.012-0.018)
/// - ±10% scale variation for visual diversity
///
/// Run via: Tools → Distribute Trash
/// </summary>
public static class TrashDistributor
{
    // ═══════════════════════════════════════════
    // TRASH PREFAB PATHS
    // ═══════════════════════════════════════════
    private static readonly string[] trashPrefabPaths = {
        // Plastic
        "Assets/Trash/Plastic/Prefabs/TR_SodaBottle_01.prefab",
        "Assets/Trash/Plastic/Prefabs/TR_PlasticBottle_01.prefab",
        // Paper
        "Assets/Trash/Paper/Prefabs/TR_PizzaBox_01.prefab",
        "Assets/Trash/Paper/Prefabs/TR_PaperCup_01.prefab",
        "Assets/Trash/Paper/Prefabs/TR_PaperSheet_01.prefab",
        "Assets/Trash/Paper/Prefabs/TR_NewspaperStack_01.prefab",
        // Glass
        "Assets/Trash/Glass/Prefabs/TR_GlassBottle_01.prefab",
        "Assets/Trash/Glass/Prefabs/TR_GlassBottle_02.prefab",
        // Metal
        "Assets/Trash/Metal/Prefabs/TR_MetalCan_01.prefab",
        "Assets/Trash/Metal/Prefabs/TR_MetalCan_02.prefab",
        "Assets/Trash/Metal/Prefabs/TR_MetalCan_03.prefab",
        "Assets/Trash/Metal/Prefabs/TR_FoodCan_01.prefab",
        "Assets/Trash/Metal/Prefabs/TR_AluminumCan_01.prefab",
    };

    // ═══════════════════════════════════════════
    // DISTRIBUTION SETTINGS
    // ═══════════════════════════════════════════

    // Zone 1: Plaza Core (immediate gameplay area)
    private const float ZONE1_MIN = 2f;      // Don't spawn directly on player
    private const float ZONE1_MAX = 25f;     // Plaza radius
    private const int   ZONE1_ITEMS = 60;    // HIGH density — instant gameplay

    // Zone 2: Nearby Streets (medium exploration)
    private const float ZONE2_MIN = 25f;
    private const float ZONE2_MAX = 65f;
    private const int   ZONE2_ITEMS = 50;    // MEDIUM density

    // Zone 3: City Edges (reward exploration)
    private const float ZONE3_MIN = 65f;
    private const float ZONE3_MAX = 140f;
    private const int   ZONE3_ITEMS = 40;    // LOWER density

    // Placement quality
    private const float GROUND_MAX_Y = 0.8f;   // Reject surfaces above this Y (rooftops)
    private const float GROUND_MIN_Y = -2f;     // Reject underground
    private const float GROUND_OFFSET = 0.02f;  // Slight elevation above ground to prevent z-fighting
    private const float MIN_ITEM_SPACING = 0.3f; // Minimum meters between items (prevents stacking)

    // Cluster settings
    private const float CLUSTER_CHANCE = 0.30f;  // 30% chance of spawning a cluster
    private const int   CLUSTER_MIN = 2;
    private const int   CLUSTER_MAX = 4;
    private const float CLUSTER_RADIUS = 1.5f;   // How spread out clusters are

    // Container name
    private const string CONTAINER_NAME = "City_Trash_Distributions";

    [MenuItem("Tools/Distribute Trash")]
    public static void DistributeTrash()
    {
        Debug.Log("═══════════════════════════════════════════════════════");
        Debug.Log("  TRASH DISTRIBUTION SYSTEM — Starting Professional Placement");
        Debug.Log("═══════════════════════════════════════════════════════");

        // ── Find epicenter (player spawn) ──
        Vector3 epicenter = Vector3.zero;
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            epicenter = player.transform.position;
            epicenter.y = 0; // Use ground level
            Debug.Log($"[Setup] Epicenter at Player position: {epicenter}");
        }
        else
        {
            Debug.Log("[Setup] Player not found, using world origin as epicenter.");
        }

        // ── Load all prefabs ──
        List<GameObject> loadedPrefabs = new List<GameObject>();
        foreach (string path in trashPrefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
                loadedPrefabs.Add(prefab);
            else
                Debug.LogWarning($"[Setup] Prefab not found: {path}");
        }

        if (loadedPrefabs.Count == 0)
        {
            Debug.LogError("[Setup] FAILED: No valid trash prefabs found!");
            return;
        }
        Debug.Log($"[Setup] Loaded {loadedPrefabs.Count} trash prefabs.");

        // ── Cleanup existing distribution ──
        GameObject existingContainer = GameObject.Find(CONTAINER_NAME);
        if (existingContainer != null)
        {
            Undo.DestroyObjectImmediate(existingContainer);
            Debug.Log("[Setup] Cleared previous trash distribution.");
        }

        GameObject container = new GameObject(CONTAINER_NAME);
        Undo.RegisterCreatedObjectUndo(container, "Distribute Trash");

        // ── Track spawned positions to enforce minimum spacing ──
        List<Vector3> spawnedPositions = new List<Vector3>();

        // ── Zone 1: Plaza Core — HIGH density ──
        EditorUtility.DisplayProgressBar("Distributing Trash", "Zone 1: Plaza Core...", 0.1f);
        int zone1Count = SpawnInZone(epicenter, ZONE1_MIN, ZONE1_MAX, ZONE1_ITEMS,
                                      loadedPrefabs, container.transform, spawnedPositions);
        Debug.Log($"[Zone 1] ✓ Plaza Core: {zone1Count} items (radius {ZONE1_MIN}-{ZONE1_MAX}m)");

        // ── Zone 2: Nearby Streets — MEDIUM density ──
        EditorUtility.DisplayProgressBar("Distributing Trash", "Zone 2: Nearby Streets...", 0.4f);
        int zone2Count = SpawnInZone(epicenter, ZONE2_MIN, ZONE2_MAX, ZONE2_ITEMS,
                                      loadedPrefabs, container.transform, spawnedPositions);
        Debug.Log($"[Zone 2] ✓ Nearby Streets: {zone2Count} items (radius {ZONE2_MIN}-{ZONE2_MAX}m)");

        // ── Zone 3: City Edges — LOWER density ──
        EditorUtility.DisplayProgressBar("Distributing Trash", "Zone 3: City Edges...", 0.7f);
        int zone3Count = SpawnInZone(epicenter, ZONE3_MIN, ZONE3_MAX, ZONE3_ITEMS,
                                      loadedPrefabs, container.transform, spawnedPositions);
        Debug.Log($"[Zone 3] ✓ City Edges: {zone3Count} items (radius {ZONE3_MIN}-{ZONE3_MAX}m)");

        // ── Finalize ──
        EditorUtility.ClearProgressBar();

        int totalItems = zone1Count + zone2Count + zone3Count;

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("═══════════════════════════════════════════════════════");
        Debug.Log("  TRASH DISTRIBUTION — Complete!");
        Debug.Log($"  Total items placed: {totalItems}");
        Debug.Log($"  Zone 1 (Plaza):    {zone1Count}");
        Debug.Log($"  Zone 2 (Streets):  {zone2Count}");
        Debug.Log($"  Zone 3 (Edges):    {zone3Count}");
        Debug.Log($"  Prefabs used:      {loadedPrefabs.Count}");
        Debug.Log("═══════════════════════════════════════════════════════");
    }

    /// <summary>
    /// Spawns trash items in a ring zone around the center with professional placement.
    /// </summary>
    private static int SpawnInZone(Vector3 center, float minRadius, float maxRadius,
                                    int targetCount, List<GameObject> prefabs,
                                    Transform parent, List<Vector3> spawnedPositions)
    {
        int spawned = 0;
        int maxAttempts = targetCount * 8; // Allow generous retries
        int attempts = 0;

        while (spawned < targetCount && attempts < maxAttempts)
        {
            attempts++;

            // ── Random position in donut ring ──
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float radius = Random.Range(minRadius, maxRadius);
            Vector3 candidateXZ = center + new Vector3(randomDir.x * radius, 0, randomDir.y * radius);

            // ── Raycast downward to find ground ──
            Vector3 rayStart = candidateXZ + Vector3.up * 50f;
            if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 100f))
                continue;

            // ── Reject non-ground surfaces ──
            if (hit.point.y > GROUND_MAX_Y || hit.point.y < GROUND_MIN_Y)
                continue;

            // ── Reject surfaces that aren't roughly flat (walls, steep slopes) ──
            if (Vector3.Dot(hit.normal, Vector3.up) < 0.7f)
                continue;

            Vector3 spawnPos = hit.point + Vector3.up * GROUND_OFFSET;

            // ── Check minimum spacing ──
            if (!IsPositionValid(spawnPos, spawnedPositions))
                continue;

            // ── Decide: single item or cluster ──
            if (Random.value < CLUSTER_CHANCE && spawned + CLUSTER_MIN <= targetCount)
            {
                // Spawn a cluster of 2-4 items
                int clusterSize = Random.Range(CLUSTER_MIN, CLUSTER_MAX + 1);
                int clusterSpawned = 0;

                for (int c = 0; c < clusterSize && spawned < targetCount; c++)
                {
                    Vector3 clusterOffset = Vector3.zero;
                    if (c > 0) // First item at exact position, others offset
                    {
                        Vector2 offset2D = Random.insideUnitCircle * CLUSTER_RADIUS;
                        clusterOffset = new Vector3(offset2D.x, 0, offset2D.y);

                        // Re-raycast for cluster offset position
                        Vector3 clusterRayStart = spawnPos + clusterOffset + Vector3.up * 10f;
                        if (Physics.Raycast(clusterRayStart, Vector3.down, out RaycastHit clusterHit, 20f))
                        {
                            if (clusterHit.point.y > GROUND_MAX_Y || clusterHit.point.y < GROUND_MIN_Y)
                                continue;
                            clusterOffset.y = clusterHit.point.y - spawnPos.y + GROUND_OFFSET;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    Vector3 itemPos = spawnPos + clusterOffset;
                    if (c > 0 && !IsPositionValid(itemPos, spawnedPositions))
                        continue;

                    GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];
                    if (SpawnSingleItem(prefab, itemPos, parent))
                    {
                        spawnedPositions.Add(itemPos);
                        spawned++;
                        clusterSpawned++;
                    }
                }
            }
            else
            {
                // Single item
                GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];
                if (SpawnSingleItem(prefab, spawnPos, parent))
                {
                    spawnedPositions.Add(spawnPos);
                    spawned++;
                }
            }
        }

        return spawned;
    }

    /// <summary>
    /// Spawns a single trash item with natural rotation and scale variation.
    /// </summary>
    private static bool SpawnSingleItem(GameObject prefab, Vector3 position, Transform parent)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        if (instance == null)
            instance = Object.Instantiate(prefab);

        if (instance == null)
            return false;

        // ── Position ──
        instance.transform.position = position;

        // ── Rotation: random Y + natural tilt ──
        float yRot = Random.Range(0f, 360f);
        float xTilt = 0f;
        float zTilt = 0f;

        // Some items look good tilted (bottles, cans), others not (pizza boxes)
        bool isPizzaBox = prefab.name.Contains("PizzaBox");
        bool isPaperSheet = prefab.name.Contains("PaperSheet");

        if (!isPizzaBox && !isPaperSheet)
        {
            // 50% chance of tilt for non-flat items
            if (Random.value > 0.5f)
            {
                xTilt = Random.Range(-20f, 20f);
                zTilt = Random.Range(-20f, 20f);
            }
        }
        else if (isPaperSheet)
        {
            // Paper sheets lie flat with slight random rotation
            xTilt = Random.Range(-5f, 5f);
            zTilt = Random.Range(-5f, 5f);
        }

        instance.transform.rotation = Quaternion.Euler(xTilt, yRot, zTilt);

        // ── Scale: prefab scale with ±10% natural variation ──
        // Prefab scales are already correctly set by TrashScaleFixer
        float scaleVariation = Random.Range(0.9f, 1.1f);
        instance.transform.localScale = prefab.transform.localScale * scaleVariation;

        // ── Parent under container ──
        instance.transform.SetParent(parent);

        return true;
    }

    /// <summary>
    /// Checks if a position is far enough from all previously spawned items.
    /// </summary>
    private static bool IsPositionValid(Vector3 pos, List<Vector3> existing)
    {
        float sqrMinSpacing = MIN_ITEM_SPACING * MIN_ITEM_SPACING;
        for (int i = 0; i < existing.Count; i++)
        {
            if ((pos - existing[i]).sqrMagnitude < sqrMinSpacing)
                return false;
        }
        return true;
    }
}
