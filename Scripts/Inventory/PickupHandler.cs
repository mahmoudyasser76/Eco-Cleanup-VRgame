using UnityEngine;

/// <summary>
/// Handles player input for picking up interactable items.
/// Bridges the PlayerInteraction detection system with the PlayerInventory holding system.
/// 
/// Responsibilities:
/// - Listen for pickup input (E key)
/// - Validate the current interaction target
/// - Delegate pickup to PlayerInventory
/// - Clear the interaction target after successful pickup
/// 
/// Design notes:
/// - Input is guarded against spam (single press only)
/// - Null-safe: gracefully handles missing references
/// - Lightweight: no Update overhead when not pressing keys
/// 
/// Attach to: Player/PlayerCharacter (same object as PlayerInteraction).
/// Requires: PlayerInteraction, PlayerInventory on the same GameObject.
/// </summary>
[RequireComponent(typeof(PlayerInteraction))]
[RequireComponent(typeof(PlayerInventory))]
public class PickupHandler : MonoBehaviour
{
    // ─── Cached References ───
    private PlayerInteraction playerInteraction;
    private PlayerInventory playerInventory;

    // ─── Input Settings ───
    [Header("Input Configuration")]
    [Tooltip("The key used to pick up items.")]
    public KeyCode pickupKey = KeyCode.E;

    [Tooltip("Minimum time between pickup attempts (prevents spam).")]
    [Range(0.1f, 1f)]
    public float pickupCooldown = 0.25f;

    private float lastPickupTime;

    private void Awake()
    {
        // Cache component references
        playerInteraction = GetComponent<PlayerInteraction>();
        playerInventory = GetComponent<PlayerInventory>();

        if (playerInteraction == null)
            Debug.LogError("[PickupHandler] PlayerInteraction component not found! Pickup will not work.");

        if (playerInventory == null)
            Debug.LogError("[PickupHandler] PlayerInventory component not found! Pickup will not work.");
    }

    private void Update()
    {
        // ── Listen for pickup input (KeyDown = single press only) ──
        if (Input.GetKeyDown(pickupKey))
        {
            TryPerformPickup();
        }
    }

    /// <summary>
    /// Attempts to perform a pickup action.
    /// Validates all conditions before delegating to PlayerInventory.
    /// </summary>
    private void TryPerformPickup()
    {
        // ── Cooldown check (anti-spam) ──
        if (Time.time - lastPickupTime < pickupCooldown)
            return;

        lastPickupTime = Time.time;

        // ── Validate references ──
        if (playerInteraction == null || playerInventory == null)
            return;

        // ── Check if we have a valid interaction target ──
        if (!playerInteraction.HasTarget)
            return;

        // ── Get the current target's IInteractable ──
        IInteractable target = playerInteraction.CurrentTarget;
        if (target == null || !target.CanInteract)
            return;

        // ── Check if the target is a TrashItem ──
        TrashItem trashItem = null;

        // Try to get TrashItem from the target object
        if (playerInteraction.CurrentTargetObject != null)
        {
            trashItem = playerInteraction.CurrentTargetObject.GetComponent<TrashItem>();
        }

        // Fallback: cast IInteractable to MonoBehaviour and try GetComponent
        if (trashItem == null && target is MonoBehaviour targetMono)
        {
            trashItem = targetMono.GetComponent<TrashItem>();
        }

        if (trashItem == null)
        {
            // Target is an interactable but NOT a TrashItem — delegate to its own OnInteract
            target.OnInteract();
            return;
        }

        // ── Attempt pickup via inventory system ──
        bool success = playerInventory.TryPickup(trashItem);

        if (success)
        {
            // Clear the interaction target since the item is now deactivated
            playerInteraction.ClearTarget();
        }
        // If failed (already holding), PlayerInventory already showed feedback
    }
}
