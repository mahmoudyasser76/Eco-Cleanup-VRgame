using UnityEngine;

/// <summary>
/// Manages the player's single-item holding system.
/// Tracks the currently held TrashItem and exposes state for UI, recycling, and scoring systems.
/// 
/// Design:
/// - Only ONE item can be held at a time (single-item rule).
/// - Items are deactivated on pickup, not destroyed (supports reset/reuse).
/// - Provides events and accessors for future inventory UI, recycling bin validation, and score integration.
/// 
/// Attach to: Player/PlayerCharacter (same object as PlayerInteraction).
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    // ─── Singleton-style accessor (optional, for global access) ───
    public static PlayerInventory Instance { get; private set; }

    // ─── State ───
    [Header("Inventory State (Read-Only)")]
    [Tooltip("The trash item currently being held by the player.")]
    [SerializeField] private TrashItem currentHeldItem;

    /// <summary>
    /// The currently held TrashItem, or null if the player's hands are empty.
    /// </summary>
    public TrashItem CurrentHeldItem => currentHeldItem;

    /// <summary>
    /// Whether the player is currently holding an item.
    /// </summary>
    public bool IsHoldingItem => currentHeldItem != null;

    // ─── Clean API for External Systems ───

    /// <summary>
    /// Whether the player currently has an item. Alias for IsHoldingItem.
    /// Clean API for external systems (recycling bins, score, etc.).
    /// </summary>
    public bool HasItem() => currentHeldItem != null;

    /// <summary>
    /// Returns the currently held TrashItem, or null if empty.
    /// Clean API for external systems.
    /// </summary>
    public TrashItem GetHeldItem() => currentHeldItem;

    /// <summary>
    /// Clears the held item without reactivating it in the world.
    /// Used when an item is consumed (e.g., recycled correctly).
    /// Returns the cleared item, or null.
    /// </summary>
    public TrashItem ClearItem()
    {
        if (!IsHoldingItem)
            return null;

        TrashItem cleared = currentHeldItem;
        currentHeldItem = null;

        UpdateInventoryUI();
        OnItemReleased?.Invoke(cleared);

        Debug.Log($"[PlayerInventory] Cleared: {cleared.DisplayName}");
        return cleared;
    }

    // ─── Events (for future systems) ───

    /// <summary>
    /// Fired when the player picks up an item. Passes the picked-up TrashItem.
    /// </summary>
    public System.Action<TrashItem> OnItemPickedUp;

    /// <summary>
    /// Fired when the player drops or uses their held item. Passes the released TrashItem.
    /// </summary>
    public System.Action<TrashItem> OnItemReleased;

    // ─── Audio ───
    [Header("Audio Feedback")]
    [Tooltip("Optional pickup sound effect. Leave empty if no audio is available.")]
    public AudioClip pickupSound;

    [Tooltip("Volume for the pickup sound.")]
    [Range(0f, 1f)]
    public float pickupVolume = 0.7f;

    private AudioSource audioSource;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Debug.LogWarning("[PlayerInventory] Duplicate instance detected. Destroying this one.");
            Destroy(this);
            return;
        }

        // Ensure AudioSource for pickup SFX
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound for UI feedback
        }
    }

    // ─── Core API ───

    /// <summary>
    /// Attempts to pick up a trash item. Enforces single-item rule.
    /// Returns true if the item was successfully picked up.
    /// </summary>
    /// <param name="item">The TrashItem to pick up.</param>
    /// <returns>True if pickup succeeded, false if blocked.</returns>
    public bool TryPickup(TrashItem item)
    {
        if (item == null)
        {
            Debug.LogWarning("[PlayerInventory] TryPickup called with null item.");
            return false;
        }

        // ── Single-item rule: block if already holding ──
        if (IsHoldingItem)
        {
            ShowAlreadyHoldingFeedback();
            return false;
        }

        // ── Validate item state ──
        if (!item.IsInteractable || item.IsCollected)
        {
            Debug.LogWarning($"[PlayerInventory] Item '{item.DisplayName}' is not in a pickable state.");
            return false;
        }

        // ── Perform pickup ──
        currentHeldItem = item;

        // Mark item as collected (updates its internal state)
        item.IsCollected = true;
        item.IsInteractable = false;

        // Disable the item in the world (not destroyed — can be reset)
        DisableItemInWorld(item);

        // Play enhanced pickup feedback (VFX + sound via GameFeedbackManager)
        if (GameFeedbackManager.Instance != null)
            GameFeedbackManager.Instance.PlayPickupFeedback(item.transform.position);

        // Play pickup sound (fallback / legacy)
        PlayPickupSound();

        // Show pickup feedback on UI
        ShowPickupFeedback(item);

        // Update inventory display
        UpdateInventoryUI();

        // Fire event
        OnItemPickedUp?.Invoke(item);

        Debug.Log($"[PlayerInventory] Picked up: {item.DisplayName} ({item.Category})");
        return true;
    }

    /// <summary>
    /// Releases the currently held item (for recycling, dropping, etc.).
    /// Returns the released item, or null if nothing was held.
    /// </summary>
    /// <param name="reactivateInWorld">If true, re-enables the item in the scene.</param>
    /// <returns>The released TrashItem, or null.</returns>
    public TrashItem ReleaseItem(bool reactivateInWorld = false)
    {
        if (!IsHoldingItem)
            return null;

        TrashItem released = currentHeldItem;
        currentHeldItem = null;

        if (reactivateInWorld)
        {
            released.ResetItem();
        }

        // Update inventory display
        UpdateInventoryUI();

        // Fire event
        OnItemReleased?.Invoke(released);

        Debug.Log($"[PlayerInventory] Released: {released.DisplayName}");
        return released;
    }

    /// <summary>
    /// Consumes the held item (used when recycled correctly). 
    /// Returns point value. Does NOT reactivate the item in world.
    /// </summary>
    /// <returns>Point value of the consumed item, or 0 if nothing held.</returns>
    public int ConsumeItem()
    {
        if (!IsHoldingItem)
            return 0;

        int points = currentHeldItem.PointValue;
        TrashItem consumed = currentHeldItem;
        currentHeldItem = null;

        UpdateInventoryUI();
        OnItemReleased?.Invoke(consumed);

        Debug.Log($"[PlayerInventory] Consumed: {consumed.DisplayName} for {points} points.");
        return points;
    }

    // ─── Internal Helpers ───

    /// <summary>
    /// Disables the item's visual and physical presence in the world.
    /// Does NOT destroy the GameObject — keeps it for potential reset.
    /// </summary>
    private void DisableItemInWorld(TrashItem item)
    {
        if (item == null) return;

        // Disable the entire GameObject (cleanest approach — hides all renderers + colliders)
        item.gameObject.SetActive(false);
    }

    /// <summary>
    /// Plays the pickup sound effect if one is assigned.
    /// </summary>
    private void PlayPickupSound()
    {
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound, pickupVolume);
        }
    }

    /// <summary>
    /// Shows UI feedback for a successful pickup.
    /// </summary>
    private void ShowPickupFeedback(TrashItem item)
    {
        if (UIManager.Instance != null)
        {
            Color feedbackColor = item.GetCategoryColor();
            UIManager.Instance.ShowFeedback($"Picked up: {item.DisplayName}", feedbackColor);
        }
    }

    /// <summary>
    /// Shows feedback when trying to pick up while already holding an item.
    /// </summary>
    private void ShowAlreadyHoldingFeedback()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowFeedback("Already holding an item!", new Color(1f, 0.4f, 0.3f));
        }

        // Trigger blocked pulse on inventory UI if available
        var inventoryUI = FindFirstObjectByType<InventoryUIController>();
        if (inventoryUI != null)
            inventoryUI.PlayBlockedPulse();
    }

    /// <summary>
    /// Updates the HUD inventory display to reflect the current state.
    /// </summary>
    private void UpdateInventoryUI()
    {
        if (UIManager.Instance == null) return;

        if (IsHoldingItem)
        {
            UIManager.Instance.UpdateInventory(null, currentHeldItem.DisplayName);
        }
        else
        {
            UIManager.Instance.UpdateInventory(null, "No Item");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
