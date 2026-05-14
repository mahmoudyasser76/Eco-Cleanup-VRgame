using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Dedicated UI controller for the inventory display panel.
/// Subscribes to PlayerInventory events and provides smooth, polished visual updates.
/// 
/// Features:
/// - Animated transitions on pickup/release (scale pop + fade)
/// - Category-colored accent strip showing item type
/// - Dynamic icon swapping per trash category
/// - Smooth "No Item" ↔ held-item state transitions
/// - Pulse animation when pickup is blocked (already holding)
/// 
/// Attach to: GameCanvas/HUD/InventoryUI
/// Requires: PlayerInventory to be present in scene.
/// </summary>
public class InventoryUIController : MonoBehaviour
{
    // ─── References ───
    [Header("UI Elements")]
    [Tooltip("The Image component showing the item/category icon.")]
    public Image itemIcon;

    [Tooltip("The Text component showing the item name.")]
    public Text itemNameText;

    [Tooltip("Optional accent image that changes color based on trash category.")]
    public Image categoryAccent;

    [Header("Category Icons (Optional)")]
    [Tooltip("Icon displayed when holding a Plastic item.")]
    public Sprite plasticIcon;

    [Tooltip("Icon displayed when holding a Paper item.")]
    public Sprite paperIcon;

    [Tooltip("Icon displayed when holding a Glass item.")]
    public Sprite glassIcon;

    [Tooltip("Icon displayed when holding a Metal item.")]
    public Sprite metalIcon;

    [Tooltip("Default icon displayed when not holding any item.")]
    public Sprite emptyIcon;

    [Header("Animation Settings")]
    [Tooltip("Duration of the pickup pop animation.")]
    [Range(0.1f, 0.5f)]
    public float popDuration = 0.25f;

    [Tooltip("Scale multiplier at peak of pop animation.")]
    [Range(1.05f, 1.3f)]
    public float popScale = 1.15f;

    [Tooltip("Duration of the content fade transition.")]
    [Range(0.05f, 0.3f)]
    public float fadeDuration = 0.15f;

    // ─── Cached State ───
    private PlayerInventory inventory;
    private RectTransform panelRect;
    private CanvasGroup canvasGroup;
    private Coroutine activeAnimation;
    private Vector3 originalScale;

    // ─── Colors ───
    private static readonly Color EmptyTextColor = new Color(0.7f, 0.75f, 0.8f, 0.6f);
    private static readonly Color HoldingTextColor = Color.white;

    private void Awake()
    {
        panelRect = GetComponent<RectTransform>();
        if (panelRect != null)
            originalScale = panelRect.localScale;

        // Add CanvasGroup if not present (for fade animations)
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        // Find PlayerInventory
        inventory = PlayerInventory.Instance;
        if (inventory == null)
            inventory = FindFirstObjectByType<PlayerInventory>();

        if (inventory == null)
        {
            Debug.LogWarning("[InventoryUIController] PlayerInventory not found. UI will not update.");
            return;
        }

        // Subscribe to events
        inventory.OnItemPickedUp += OnItemPickedUp;
        inventory.OnItemReleased += OnItemReleased;

        // Initialize to current state
        if (inventory.IsHoldingItem)
            SetHoldingState(inventory.CurrentHeldItem, false);
        else
            SetEmptyState(false);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (inventory != null)
        {
            inventory.OnItemPickedUp -= OnItemPickedUp;
            inventory.OnItemReleased -= OnItemReleased;
        }
    }

    // ─── Event Handlers ───

    private void OnItemPickedUp(TrashItem item)
    {
        if (item == null) return;
        SetHoldingState(item, true);
    }

    private void OnItemReleased(TrashItem item)
    {
        SetEmptyState(true);
    }

    // ─── State Management ───

    /// <summary>
    /// Updates the UI to reflect holding an item, with optional animation.
    /// </summary>
    private void SetHoldingState(TrashItem item, bool animate)
    {
        // Update text
        if (itemNameText != null)
        {
            itemNameText.text = item.DisplayName;
            itemNameText.color = HoldingTextColor;
        }

        // Update icon based on category
        if (itemIcon != null)
        {
            Sprite categorySprite = GetCategorySprite(item.Category);
            if (categorySprite != null)
            {
                itemIcon.sprite = categorySprite;
                itemIcon.enabled = true;
                itemIcon.color = Color.white;
            }
            else
            {
                // No category sprite — use default or hide
                if (emptyIcon != null)
                {
                    itemIcon.sprite = emptyIcon;
                    itemIcon.enabled = true;
                }
                // Tint icon to category color
                itemIcon.color = item.GetCategoryColor();
            }
        }

        // Update category accent strip
        if (categoryAccent != null)
        {
            categoryAccent.color = item.GetCategoryColor();
            categoryAccent.enabled = true;
        }

        // Also update UIManager for cross-system consistency
        if (UIManager.Instance != null)
        {
            Sprite sprite = GetCategorySprite(item.Category);
            UIManager.Instance.UpdateInventory(sprite, item.DisplayName);
        }

        // Animate
        if (animate)
            PlayPickupAnimation();
    }

    /// <summary>
    /// Updates the UI to reflect empty hands, with optional animation.
    /// </summary>
    private void SetEmptyState(bool animate)
    {
        // Update text
        if (itemNameText != null)
        {
            itemNameText.text = "No Item";
            itemNameText.color = EmptyTextColor;
        }

        // Update icon
        if (itemIcon != null)
        {
            if (emptyIcon != null)
            {
                itemIcon.sprite = emptyIcon;
                itemIcon.enabled = true;
                itemIcon.color = new Color(1f, 1f, 1f, 0.4f);
            }
            else
            {
                itemIcon.enabled = false;
            }
        }

        // Hide category accent
        if (categoryAccent != null)
        {
            categoryAccent.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        }

        // Also update UIManager
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateInventory(null, "No Item");
        }

        // Animate
        if (animate)
            PlayReleaseAnimation();
    }

    // ─── Animation ───

    /// <summary>
    /// Plays a satisfying pop + settle animation on pickup.
    /// </summary>
    private void PlayPickupAnimation()
    {
        if (activeAnimation != null)
            StopCoroutine(activeAnimation);
        activeAnimation = StartCoroutine(PickupAnimationRoutine());
    }

    /// <summary>
    /// Plays a subtle shrink animation on release.
    /// </summary>
    private void PlayReleaseAnimation()
    {
        if (activeAnimation != null)
            StopCoroutine(activeAnimation);
        activeAnimation = StartCoroutine(ReleaseAnimationRoutine());
    }

    /// <summary>
    /// Plays a warning pulse when pickup is blocked. 
    /// Call externally from PlayerInventory or PickupHandler.
    /// </summary>
    public void PlayBlockedPulse()
    {
        if (activeAnimation != null)
            StopCoroutine(activeAnimation);
        activeAnimation = StartCoroutine(BlockedPulseRoutine());
    }

    private IEnumerator PickupAnimationRoutine()
    {
        if (panelRect == null) yield break;

        // Phase 1: Quick scale up
        float halfDuration = popDuration * 0.4f;
        float time = 0f;
        while (time < halfDuration)
        {
            time += Time.deltaTime;
            float t = time / halfDuration;
            float easeOut = 1f - (1f - t) * (1f - t); // Ease out quad
            panelRect.localScale = Vector3.Lerp(originalScale, originalScale * popScale, easeOut);
            yield return null;
        }

        // Phase 2: Settle back with overshoot
        time = 0f;
        float settleDuration = popDuration * 0.6f;
        Vector3 peakScale = originalScale * popScale;
        while (time < settleDuration)
        {
            time += Time.deltaTime;
            float t = time / settleDuration;
            // Elastic ease out for satisfying bounce
            float elastic = 1f - Mathf.Pow(2f, -10f * t) * Mathf.Cos(t * Mathf.PI * 2f);
            panelRect.localScale = Vector3.Lerp(peakScale, originalScale, elastic);
            yield return null;
        }

        panelRect.localScale = originalScale;
        activeAnimation = null;
    }

    private IEnumerator ReleaseAnimationRoutine()
    {
        if (panelRect == null) yield break;

        // Quick shrink and return
        float time = 0f;
        float shrinkScale = 0.92f;
        float duration = popDuration * 0.5f;

        // Shrink
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            panelRect.localScale = Vector3.Lerp(originalScale, originalScale * shrinkScale, t);
            yield return null;
        }

        // Return
        time = 0f;
        Vector3 shrunk = originalScale * shrinkScale;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            float easeOut = 1f - (1f - t) * (1f - t);
            panelRect.localScale = Vector3.Lerp(shrunk, originalScale, easeOut);
            yield return null;
        }

        panelRect.localScale = originalScale;
        activeAnimation = null;
    }

    private IEnumerator BlockedPulseRoutine()
    {
        if (panelRect == null) yield break;

        // Quick red-tinted shake
        Color originalTextColor = itemNameText != null ? itemNameText.color : Color.white;
        Color warningColor = new Color(1f, 0.3f, 0.25f);

        for (int i = 0; i < 2; i++)
        {
            // Flash red
            if (itemNameText != null)
                itemNameText.color = warningColor;

            panelRect.localScale = originalScale * 1.05f;
            yield return new WaitForSeconds(0.06f);

            if (itemNameText != null)
                itemNameText.color = originalTextColor;

            panelRect.localScale = originalScale * 0.97f;
            yield return new WaitForSeconds(0.06f);
        }

        panelRect.localScale = originalScale;
        if (itemNameText != null)
            itemNameText.color = originalTextColor;

        activeAnimation = null;
    }

    // ─── Helpers ───

    /// <summary>
    /// Returns the appropriate sprite for a trash category, or null if not set.
    /// </summary>
    private Sprite GetCategorySprite(TrashCategory category)
    {
        switch (category)
        {
            case TrashCategory.Plastic: return plasticIcon;
            case TrashCategory.Paper:   return paperIcon;
            case TrashCategory.Glass:   return glassIcon;
            case TrashCategory.Metal:   return metalIcon;
            default:                    return null;
        }
    }

    /// <summary>
    /// Force-refresh the UI to match current inventory state.
    /// Safe to call at any time.
    /// </summary>
    public void RefreshDisplay()
    {
        if (inventory == null) return;

        if (inventory.IsHoldingItem)
            SetHoldingState(inventory.CurrentHeldItem, false);
        else
            SetEmptyState(false);
    }
}
