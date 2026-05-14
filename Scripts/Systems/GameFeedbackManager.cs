using UnityEngine;

/// <summary>
/// Central Feedback Manager — orchestrates ALL gameplay feedback:
/// audio (Interface & Item Sounds Lite), VFX (Cartoon FX Remaster prefabs),
/// and floating text popups for every key player action.
///
/// Design:
/// - Singleton accessible via GameFeedbackManager.Instance
/// - VFX prefabs are instantiated at world positions and auto-destroy
/// - Audio is played as 2D one-shots for responsive UI-quality feel
/// - Floating text popups created via FloatingFeedbackText helper
///
/// Attach to: Player/PlayerCharacter (same object as RecyclingFeedback).
/// Wired by: Editor/GamePolishUtility.cs (Tools → Apply Gameplay Polish)
/// </summary>
public class GameFeedbackManager : MonoBehaviour
{
    // ─── Singleton ───
    public static GameFeedbackManager Instance { get; private set; }

    // ─── Audio Clips (from Interface & Item Sounds Lite - Version 2) ───
    [Header("Audio — Pickup")]
    [Tooltip("Short pop/click sound when picking up trash.")]
    public AudioClip pickupClip;

    [Header("Audio — Recycling")]
    [Tooltip("Positive confirmation sound for correct recycling.")]
    public AudioClip successClip;

    [Tooltip("Warning buzzer sound for wrong recycling.")]
    public AudioClip wrongClip;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float sfxVolume = 0.6f;

    // ─── VFX Prefabs (from Cartoon FX Remaster) ───
    [Header("VFX — Pickup")]
    [Tooltip("Small sparkle/poof effect when picking up an item.")]
    public GameObject pickupVFX;

    [Header("VFX — Recycling")]
    [Tooltip("Green sparkle burst for correct recycling.")]
    public GameObject successVFX;

    [Tooltip("Red warning burst for wrong recycling.")]
    public GameObject wrongVFX;

    // ─── VFX Settings ───
    [Header("VFX Settings")]
    [Tooltip("Vertical offset above spawn point for VFX.")]
    public float vfxYOffset = 1.0f;

    [Tooltip("Scale multiplier for pickup VFX (keep small).")]
    public float pickupVFXScale = 0.5f;

    [Tooltip("Scale multiplier for recycling VFX.")]
    public float recycleVFXScale = 0.7f;

    [Tooltip("Safety destroy time for VFX instances.")]
    public float vfxLifetime = 3f;

    // ─── Floating Text Settings ───
    [Header("Floating Text")]
    [Tooltip("How high the floating text travels upward.")]
    public float floatDistance = 1.5f;

    [Tooltip("Duration of the floating text display.")]
    public float floatDuration = 1.2f;

    // ─── Internal ───
    private AudioSource audioSource;

    // ─── Feedback Colors ───
    private static readonly Color SuccessTextColor = new Color(0.2f, 0.9f, 0.3f);
    private static readonly Color WrongTextColor = new Color(1f, 0.3f, 0.2f);
    private static readonly Color PickupTextColor = new Color(1f, 0.95f, 0.6f);

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("[GameFeedbackManager] Duplicate instance detected. Destroying this one.");
            Destroy(this);
            return;
        }

        // Ensure AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D for responsive UI feel
    }

    // ═══════════════════════════════════════════
    // PICKUP FEEDBACK
    // ═══════════════════════════════════════════

    /// <summary>
    /// Plays pickup feedback: sound + VFX + UI text.
    /// Call this when the player picks up a trash item.
    /// </summary>
    /// <param name="worldPos">Position of the picked-up item in world space.</param>
    public void PlayPickupFeedback(Vector3 worldPos)
    {
        // Audio
        PlaySound(pickupClip);

        // VFX
        SpawnVFX(pickupVFX, worldPos, pickupVFXScale);

        // UI Feedback (through UIManager)
        // Note: PlayerInventory already calls ShowPickupFeedback, so we skip duplicate UI here

        Debug.Log($"[GameFeedbackManager] ✓ Pickup feedback at {worldPos}");
    }

    // ═══════════════════════════════════════════
    // CORRECT RECYCLING FEEDBACK
    // ═══════════════════════════════════════════

    /// <summary>
    /// Plays success feedback: sound + green VFX + floating "+10 Correct!" text.
    /// </summary>
    /// <param name="worldPos">Position of the recycling bin.</param>
    /// <param name="points">Points awarded.</param>
    public void PlaySuccessFeedback(Vector3 worldPos, int points)
    {
        // Audio
        PlaySound(successClip);

        // VFX
        SpawnVFX(successVFX, worldPos, recycleVFXScale);

        // Floating World-Space Text
        string[] successMessages = { "Correct!", "Nice!", "Great!", "Perfect!" };
        string message = $"+{points} {successMessages[Random.Range(0, successMessages.Length)]}";
        FloatingFeedbackText.Spawn(worldPos + Vector3.up * 2f, message, SuccessTextColor, floatDuration, floatDistance);

        // Also update the HUD feedback
        if (UIManager.Instance != null)
            UIManager.Instance.ShowFeedback($"+{points} Correct Recycling!", SuccessTextColor);

        Debug.Log($"[GameFeedbackManager] ✓ Success feedback at {worldPos}, +{points}");
    }

    // ═══════════════════════════════════════════
    // WRONG RECYCLING FEEDBACK
    // ═══════════════════════════════════════════

    /// <summary>
    /// Plays wrong feedback: sound + red VFX + floating "Wrong Bin! -5" text.
    /// </summary>
    /// <param name="worldPos">Position of the recycling bin.</param>
    /// <param name="penalty">Points deducted (negative value).</param>
    public void PlayWrongFeedback(Vector3 worldPos, int penalty)
    {
        // Audio
        PlaySound(wrongClip);

        // VFX
        SpawnVFX(wrongVFX, worldPos, recycleVFXScale);

        // Floating World-Space Text
        string message = $"Wrong Bin! {penalty}";
        FloatingFeedbackText.Spawn(worldPos + Vector3.up * 2f, message, WrongTextColor, floatDuration, floatDistance);

        // Also update the HUD feedback
        if (UIManager.Instance != null)
            UIManager.Instance.ShowFeedback($"Wrong Bin! {penalty}", WrongTextColor);

        Debug.Log($"[GameFeedbackManager] ✗ Wrong feedback at {worldPos}, {penalty}");
    }

    // ═══════════════════════════════════════════
    // INTERNAL HELPERS
    // ═══════════════════════════════════════════

    /// <summary>
    /// Plays a one-shot sound effect (2D, non-overlapping with spatial audio).
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, sfxVolume);
        }
    }

    /// <summary>
    /// Instantiates a VFX prefab at the given world position with scale and auto-destroy.
    /// </summary>
    private void SpawnVFX(GameObject vfxPrefab, Vector3 worldPos, float scale)
    {
        if (vfxPrefab == null) return;

        Vector3 spawnPos = worldPos + Vector3.up * vfxYOffset;
        GameObject vfxInstance = Instantiate(vfxPrefab, spawnPos, Quaternion.identity);

        // Apply scale
        vfxInstance.transform.localScale = Vector3.one * scale;

        // Safety destroy (CFXR effects usually self-destroy via stopAction, but just in case)
        Destroy(vfxInstance, vfxLifetime);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
