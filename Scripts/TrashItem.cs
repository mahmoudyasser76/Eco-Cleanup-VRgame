using UnityEngine;

/// <summary>
/// Defines the recycling category for trash items.
/// Used by the interaction, inventory, and recycling validation systems.
/// </summary>
public enum TrashCategory
{
    Plastic,
    Paper,
    Glass,
    Metal
}

/// <summary>
/// Core component for all trash items in the recycling adventure game.
/// Attach this to every trash prefab to identify its type and manage gameplay state.
/// Implements IInteractable for seamless integration with the PlayerInteraction system.
/// </summary>
public class TrashItem : MonoBehaviour, IInteractable
{
    [Header("Trash Configuration")]
    [Tooltip("The recycling category this item belongs to.")]
    public TrashCategory Category;

    [Tooltip("Display name shown to the player during interaction.")]
    public string DisplayName = "Trash Item";

    [Tooltip("Point value awarded when this item is correctly recycled.")]
    public int PointValue = 10;

    [Header("Interaction State")]
    [Tooltip("Whether this item can currently be picked up.")]
    public bool IsInteractable = true;

    [Tooltip("Whether this item has already been collected.")]
    [HideInInspector]
    public bool IsCollected = false;

    // ─── IInteractable Implementation ───

    /// <summary>
    /// The prompt text displayed when the player is near this item.
    /// </summary>
    public string InteractionPrompt => "Press E to Pick Up";

    /// <summary>
    /// Whether this trash item can currently be interacted with.
    /// </summary>
    public bool CanInteract => IsInteractable && !IsCollected;

    /// <summary>
    /// Called when the player confirms interaction (presses E).
    /// Currently triggers collection. Override or extend for future systems.
    /// </summary>
public void OnInteract()
    {
        // Route through PlayerInventory for single-item rule enforcement.
        // If no inventory system is present, fall back to direct collection.
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.TryPickup(this);
        }
        else
        {
            // Fallback: direct collection (legacy behavior)
            Collect();
        }
    }

    /// <summary>
    /// Called each frame while this item is the player's interaction target.
    /// Visual feedback is handled by InteractableHighlight component.
    /// </summary>
    public void OnFocused()
    {
        // Visual highlight is managed by InteractableHighlight component.
        // Add additional per-frame focus effects here if needed.
    }

    /// <summary>
    /// Called when this item is no longer the player's interaction target.
    /// </summary>
    public void OnUnfocused()
    {
        // Cleanup is managed by InteractableHighlight component.
    }

    // ─── Original Methods (Preserved) ───

    /// <summary>
    /// Called when the player collects this trash item.
    /// Returns the point value and marks the item as collected.
    /// </summary>
    public int Collect()
    {
        if (!IsInteractable || IsCollected)
            return 0;

        IsCollected = true;
        IsInteractable = false;
        gameObject.SetActive(false);
        return PointValue;
    }

    /// <summary>
    /// Resets the item to its original state (for respawning).
    /// </summary>
    public void ResetItem()
    {
        IsCollected = false;
        IsInteractable = true;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Returns the correct recycling bin color for UI hints.
    /// </summary>
    public Color GetCategoryColor()
    {
        switch (Category)
        {
            case TrashCategory.Plastic: return new Color(1f, 0.84f, 0f);       // Yellow
            case TrashCategory.Paper:   return new Color(0.2f, 0.6f, 1f);      // Blue
            case TrashCategory.Glass:   return new Color(0.2f, 0.8f, 0.4f);    // Green
            case TrashCategory.Metal:   return new Color(0.75f, 0.75f, 0.75f); // Silver/Gray
            default:                    return Color.white;
        }
    }
}
