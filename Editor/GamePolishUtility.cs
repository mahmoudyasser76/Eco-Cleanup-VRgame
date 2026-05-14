using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Complete Feedback & Polish Editor Utility — Step 11
/// Wires all audio clips, VFX prefabs, fixes trash sizes, and replaces timer sound.
/// Run via: Tools → Apply Gameplay Polish
/// </summary>
public static class GamePolishUtility
{
    // ═══════════════════════════════════════════
    // AUDIO PATHS (Interface & Item Sounds Lite - Version 2)
    // ═══════════════════════════════════════════
    private const string PICKUP_CLIP_PATH = "Assets/Interface & Item Sounds Lite - Version 2/Items & Collectibles/Coin Item 9-1.wav";
    private const string SUCCESS_CLIP_PATH = "Assets/Interface & Item Sounds Lite - Version 2/Items & Collectibles/Special Collectible 20-1.wav";
    private const string WRONG_CLIP_PATH = "Assets/Interface & Item Sounds Lite - Version 2/Interface/Negative Action 15-1.wav";
    private const string TIMER_END_CLIP_PATH = "Assets/Interface & Item Sounds Lite - Version 2/Miscellaneous/Countdown 1-3.wav";

    // Background Music
    private const string GAMEPLAY_MUSIC_PATH = "Assets/Audio/gameplay.mp3";

    // ═══════════════════════════════════════════
    // VFX PATHS (Cartoon FX Remaster)
    // ═══════════════════════════════════════════
    private const string PICKUP_VFX_PATH = "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Misc/CFXR Magic Poof.prefab";
    private const string SUCCESS_VFX_PATH = "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Impacts/Variants/CFXR Hit B (Green).prefab";
    private const string WRONG_VFX_PATH = "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Impacts/CFXR Hit A (Red).prefab";

    // ═══════════════════════════════════════════
    // TRASH PREFAB SIZE CONFIGURATION
    // ═══════════════════════════════════════════
    // Using ABSOLUTE scales to avoid compounding from previous 1.5x blanket scale.
    // Each value is tuned for gameplay camera visibility.
    private static readonly Dictionary<string, Vector3> trashScaleOverrides = new Dictionary<string, Vector3>()
    {
        // Plastic — water/soda bottles (revert oversizing)
        { "Assets/Trash/Plastic/Prefabs/TR_SodaBottle_01.prefab",       new Vector3(1.2f, 1.2f, 1.2f) },
        { "Assets/Trash/Plastic/Prefabs/TR_PlasticBottle_01.prefab",    new Vector3(1.2f, 1.2f, 1.2f) },

        // Paper — increase for readability
        { "Assets/Trash/Paper/Prefabs/TR_PizzaBox_01.prefab",           new Vector3(1.6f, 1.6f, 1.6f) },
        { "Assets/Trash/Paper/Prefabs/TR_PaperCup_01.prefab",          new Vector3(1.8f, 1.8f, 1.8f) },
        { "Assets/Trash/Paper/Prefabs/TR_PaperSheet_01.prefab",        new Vector3(1.8f, 1.8f, 1.8f) },
        { "Assets/Trash/Paper/Prefabs/TR_NewspaperStack_01.prefab",    new Vector3(1.6f, 1.6f, 1.6f) },

        // Glass — slightly increased
        { "Assets/Trash/Glass/Prefabs/TR_GlassBottle_01.prefab",       new Vector3(1.5f, 1.5f, 1.5f) },
        { "Assets/Trash/Glass/Prefabs/TR_GlassBottle_02.prefab",       new Vector3(1.5f, 1.5f, 1.5f) },

        // Metal — cans need to be bigger for visibility
        { "Assets/Trash/Metal/Prefabs/TR_MetalCan_01.prefab",          new Vector3(1.8f, 1.8f, 1.8f) },
        { "Assets/Trash/Metal/Prefabs/TR_MetalCan_02.prefab",          new Vector3(1.8f, 1.8f, 1.8f) },
        { "Assets/Trash/Metal/Prefabs/TR_MetalCan_03.prefab",          new Vector3(1.8f, 1.8f, 1.8f) },
        { "Assets/Trash/Metal/Prefabs/TR_FoodCan_01.prefab",           new Vector3(1.8f, 1.8f, 1.8f) },
        { "Assets/Trash/Metal/Prefabs/TR_FoodCan_03.prefab",           new Vector3(1.8f, 1.8f, 1.8f) },
        { "Assets/Trash/Metal/Prefabs/TR_AluminumCan_01.prefab",       new Vector3(1.8f, 1.8f, 1.8f) },
    };

    [MenuItem("Tools/Apply Gameplay Polish")]
    public static void ApplyPolish()
    {
        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log("  FEEDBACK & POLISH SYSTEM — Step 11 Pipeline Starting");
        Debug.Log("═══════════════════════════════════════════════════════════");

        int totalSteps = 6;
        int currentStep = 0;

        // ══════════════════════════════════════
        // STEP 1: Ensure GameFeedbackManager exists in scene
        // ══════════════════════════════════════
        currentStep++;
        EditorUtility.DisplayProgressBar("Gameplay Polish", $"Step {currentStep}/{totalSteps}: Setting up GameFeedbackManager...", (float)currentStep / totalSteps);

        GameFeedbackManager feedbackManager = Object.FindFirstObjectByType<GameFeedbackManager>();
        if (feedbackManager == null)
        {
            // Find the PlayerCharacter to attach it (same object as RecyclingFeedback)
            RecyclingFeedback recyclingFeedback = Object.FindFirstObjectByType<RecyclingFeedback>();
            if (recyclingFeedback != null)
            {
                feedbackManager = recyclingFeedback.gameObject.AddComponent<GameFeedbackManager>();
                Debug.Log("[Step 1] Created GameFeedbackManager on PlayerCharacter (alongside RecyclingFeedback).");
            }
            else
            {
                // Fallback: find player by name
                GameObject player = GameObject.Find("PlayerCharacter");
                if (player == null) player = GameObject.Find("Player");
                if (player != null)
                {
                    feedbackManager = player.AddComponent<GameFeedbackManager>();
                    Debug.Log("[Step 1] Created GameFeedbackManager on Player object.");
                }
                else
                {
                    Debug.LogError("[Step 1] FAILED: Cannot find PlayerCharacter or RecyclingFeedback in scene!");
                    EditorUtility.ClearProgressBar();
                    return;
                }
            }
        }
        else
        {
            Debug.Log("[Step 1] GameFeedbackManager already exists in scene.");
        }

        // ══════════════════════════════════════
        // STEP 2: Assign Audio Clips
        // ══════════════════════════════════════
        currentStep++;
        EditorUtility.DisplayProgressBar("Gameplay Polish", $"Step {currentStep}/{totalSteps}: Assigning audio clips...", (float)currentStep / totalSteps);

        int audioAssigned = 0;

        // Load all audio clips
        AudioClip pickupClip = AssetDatabase.LoadAssetAtPath<AudioClip>(PICKUP_CLIP_PATH);
        AudioClip successClip = AssetDatabase.LoadAssetAtPath<AudioClip>(SUCCESS_CLIP_PATH);
        AudioClip wrongClip = AssetDatabase.LoadAssetAtPath<AudioClip>(WRONG_CLIP_PATH);
        AudioClip timerEndClip = AssetDatabase.LoadAssetAtPath<AudioClip>(TIMER_END_CLIP_PATH);

        // Assign to GameFeedbackManager
        if (feedbackManager != null)
        {
            Undo.RecordObject(feedbackManager, "Assign Feedback Audio");

            if (pickupClip != null) { feedbackManager.pickupClip = pickupClip; audioAssigned++; }
            else Debug.LogWarning($"[Step 2] Pickup clip not found at: {PICKUP_CLIP_PATH}");

            if (successClip != null) { feedbackManager.successClip = successClip; audioAssigned++; }
            else Debug.LogWarning($"[Step 2] Success clip not found at: {SUCCESS_CLIP_PATH}");

            if (wrongClip != null) { feedbackManager.wrongClip = wrongClip; audioAssigned++; }
            else Debug.LogWarning($"[Step 2] Wrong clip not found at: {WRONG_CLIP_PATH}");

            feedbackManager.sfxVolume = 0.6f;
            EditorUtility.SetDirty(feedbackManager);
        }

        // Also assign pickup sound to PlayerInventory (for legacy fallback)
        PlayerInventory playerInventory = Object.FindFirstObjectByType<PlayerInventory>();
        if (playerInventory != null && pickupClip != null)
        {
            Undo.RecordObject(playerInventory, "Assign Pickup Audio");
            playerInventory.pickupSound = pickupClip;
            playerInventory.pickupVolume = 0.5f; // Slightly lower since GameFeedbackManager also plays
            EditorUtility.SetDirty(playerInventory);
            audioAssigned++;
        }

        // Assign to RecyclingFeedback (for fallback)
        RecyclingFeedback rfb = Object.FindFirstObjectByType<RecyclingFeedback>();
        if (rfb != null)
        {
            Undo.RecordObject(rfb, "Assign Recycling Audio");
            if (successClip != null) rfb.successClip = successClip;
            if (wrongClip != null) rfb.wrongClip = wrongClip;
            rfb.sfxVolume = 0.5f;
            EditorUtility.SetDirty(rfb);
        }

        // Replace timer end sound (REMOVE thunder, use arcade countdown)
        TimerManager timerManager = Object.FindFirstObjectByType<TimerManager>();
        if (timerManager != null)
        {
            Undo.RecordObject(timerManager, "Replace Timer End Sound");
            if (timerEndClip != null)
            {
                timerManager.timeUpSound = timerEndClip;
                EditorUtility.SetDirty(timerManager);
                audioAssigned++;
                Debug.Log($"[Step 2] ✓ Timer end sound replaced with '{timerEndClip.name}' (removed thunder/lightning).");
            }
            else
            {
                Debug.LogWarning($"[Step 2] Timer end clip not found at: {TIMER_END_CLIP_PATH}");
            }
        }

        Debug.Log($"[Step 2] ✓ Assigned {audioAssigned} audio clips across all systems.");

        // ══════════════════════════════════════
        // STEP 3: Assign VFX Prefabs
        // ══════════════════════════════════════
        currentStep++;
        EditorUtility.DisplayProgressBar("Gameplay Polish", $"Step {currentStep}/{totalSteps}: Assigning VFX prefabs...", (float)currentStep / totalSteps);

        int vfxAssigned = 0;

        GameObject pickupVFX = AssetDatabase.LoadAssetAtPath<GameObject>(PICKUP_VFX_PATH);
        GameObject successVFX = AssetDatabase.LoadAssetAtPath<GameObject>(SUCCESS_VFX_PATH);
        GameObject wrongVFX = AssetDatabase.LoadAssetAtPath<GameObject>(WRONG_VFX_PATH);

        if (feedbackManager != null)
        {
            Undo.RecordObject(feedbackManager, "Assign Feedback VFX");

            if (pickupVFX != null) { feedbackManager.pickupVFX = pickupVFX; vfxAssigned++; }
            else Debug.LogWarning($"[Step 3] Pickup VFX not found at: {PICKUP_VFX_PATH}");

            if (successVFX != null) { feedbackManager.successVFX = successVFX; vfxAssigned++; }
            else Debug.LogWarning($"[Step 3] Success VFX not found at: {SUCCESS_VFX_PATH}");

            if (wrongVFX != null) { feedbackManager.wrongVFX = wrongVFX; vfxAssigned++; }
            else Debug.LogWarning($"[Step 3] Wrong VFX not found at: {WRONG_VFX_PATH}");

            EditorUtility.SetDirty(feedbackManager);
        }

        Debug.Log($"[Step 3] ✓ Assigned {vfxAssigned}/3 Cartoon FX Remaster VFX prefabs.");

        // ══════════════════════════════════════
        // STEP 4: Fix Trash Prefab Sizes
        // ══════════════════════════════════════
        currentStep++;
        EditorUtility.DisplayProgressBar("Gameplay Polish", $"Step {currentStep}/{totalSteps}: Fixing trash prefab sizes...", (float)currentStep / totalSteps);

        int sizeFixed = 0;
        foreach (var entry in trashScaleOverrides)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(entry.Key);
            if (prefab != null)
            {
                // Set ABSOLUTE scale (not multiplied — avoids compounding from previous runs)
                prefab.transform.localScale = entry.Value;
                EditorUtility.SetDirty(prefab);
                sizeFixed++;
            }
            else
            {
                Debug.LogWarning($"[Step 4] Trash prefab not found: {entry.Key}");
            }
        }
        AssetDatabase.SaveAssets();

        Debug.Log($"[Step 4] ✓ Set absolute scales on {sizeFixed}/{trashScaleOverrides.Count} trash prefabs.");

        // ══════════════════════════════════════
        // STEP 5: Update Scene Trash Instances
        // ══════════════════════════════════════
        currentStep++;
        EditorUtility.DisplayProgressBar("Gameplay Polish", $"Step {currentStep}/{totalSteps}: Updating scene trash instances...", (float)currentStep / totalSteps);

        // Find all TrashItem instances in the scene and update their scales to match prefabs
        TrashItem[] sceneTrash = Object.FindObjectsByType<TrashItem>(FindObjectsSortMode.None);
        int instancesUpdated = 0;

        foreach (TrashItem trash in sceneTrash)
        {
            // Get the prefab source path for this instance
            GameObject prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(trash.gameObject);
            if (prefabSource != null)
            {
                string prefabPath = AssetDatabase.GetAssetPath(prefabSource);
                if (trashScaleOverrides.ContainsKey(prefabPath))
                {
                    Undo.RecordObject(trash.transform, "Update Trash Instance Scale");
                    trash.transform.localScale = trashScaleOverrides[prefabPath];
                    EditorUtility.SetDirty(trash.gameObject);
                    instancesUpdated++;
                }
            }
        }

        Debug.Log($"[Step 5] ✓ Updated {instancesUpdated} trash instances in scene to match new prefab scales.");

        // ══════════════════════════════════════
        // STEP 6: Setup Gameplay Background Music
        // ══════════════════════════════════════
        currentStep++;
        EditorUtility.DisplayProgressBar("Gameplay Polish", $"Step {currentStep}/{totalSteps}: Setting up gameplay background music...", (float)currentStep / totalSteps);

        GameplayMusicManager musicManager = Object.FindFirstObjectByType<GameplayMusicManager>();
        if (musicManager == null)
        {
            // Create a new GameObject for the music manager
            GameObject musicObj = new GameObject("GameplayMusicManager");
            Undo.RegisterCreatedObjectUndo(musicObj, "Create GameplayMusicManager");
            musicManager = musicObj.AddComponent<GameplayMusicManager>();
            Debug.Log("[Step 6] Created GameplayMusicManager in scene.");
        }
        else
        {
            Debug.Log("[Step 6] GameplayMusicManager already exists in scene.");
        }

        AudioClip gameplayMusicClip = AssetDatabase.LoadAssetAtPath<AudioClip>(GAMEPLAY_MUSIC_PATH);
        if (gameplayMusicClip != null)
        {
            Undo.RecordObject(musicManager, "Assign Gameplay Music");
            musicManager.gameplayMusic = gameplayMusicClip;
            musicManager.musicVolume = 0.25f;
            musicManager.fadeInDuration = 2f;
            EditorUtility.SetDirty(musicManager);
            Debug.Log("[Step 6] ✓ Gameplay music clip assigned (gameplay.mp3).");
        }
        else
        {
            Debug.LogWarning($"[Step 6] Gameplay music not found at: {GAMEPLAY_MUSIC_PATH}");
        }

        // ══════════════════════════════════════
        // COMPLETE
        // ══════════════════════════════════════
        EditorUtility.ClearProgressBar();

        Debug.Log("═══════════════════════════════════════════════════════════");
        Debug.Log("  FEEDBACK & POLISH SYSTEM — Pipeline Complete!");
        Debug.Log($"  Audio clips: {audioAssigned} assigned");
        Debug.Log($"  VFX prefabs: {vfxAssigned} assigned");
        Debug.Log($"  Trash prefabs: {sizeFixed} rescaled");
        Debug.Log($"  Scene instances: {instancesUpdated} updated");
        Debug.Log($"  Gameplay music: {(gameplayMusicClip != null ? "✓" : "✗")}");
        Debug.Log("═══════════════════════════════════════════════════════════");

        // Mark scene dirty so user is prompted to save
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }
}
