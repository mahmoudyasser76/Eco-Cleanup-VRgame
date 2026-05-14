using UnityEngine;

/// <summary>
/// Core interaction detection system for the player.
/// Detects nearby interactable objects using Physics.OverlapSphere with layer filtering.
/// Manages targeting priority, UI prompt visibility, and visual highlights.
/// 
/// Optimized for performance:
/// - Uses layer mask to filter only "Interactable" layer objects
/// - Pre-allocated Collider buffer to avoid GC allocations
/// - Configurable scan interval to reduce physics calls
/// - Hysteresis distance to prevent target flickering
/// 
/// Attach to the Player root or PlayerCharacter object.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Maximum distance to detect interactable objects.")]
    [Range(1f, 10f)]
    public float interactionRange = 3.5f;

    [Tooltip("Extra distance before losing a locked-on target (prevents flickering).")]
    [Range(0f, 2f)]
    public float hysteresisBuffer = 0.8f;

    [Tooltip("Time interval between detection scans (seconds). Lower = more responsive but more CPU.")]
    [Range(0.02f, 0.2f)]
    public float scanInterval = 0.1f;

    [Tooltip("Maximum number of colliders to detect per scan.")]
    public int maxDetectedObjects = 20;

    [Header("Layer Settings")]
    [Tooltip("Layer mask for interactable objects. Set to 'Interactable' layer.")]
    public LayerMask interactableLayer;

    [Header("References")]
    [Tooltip("Transform used as the detection origin. Defaults to this transform if not set.")]
    public Transform detectionOrigin;

    // Current interaction target
    private IInteractable currentTarget;
    private GameObject currentTargetObject;
    private InteractableHighlight currentHighlight;
    
    // Pre-allocated buffer for overlap sphere results
    private Collider[] overlapBuffer;
    
    // Scan timer
    private float scanTimer;
    
    // Cached layer mask value
    private int layerMaskValue;
    
    // Public accessor for other systems
    /// <summary>
    /// The currently targeted interactable object, or null if none in range.
    /// </summary>
    public IInteractable CurrentTarget => currentTarget;
    
    /// <summary>
    /// The GameObject of the current target, or null if none.
    /// </summary>
    public GameObject CurrentTargetObject => currentTargetObject;
    
    /// <summary>
    /// Whether the player currently has a valid interaction target.
    /// </summary>
    public bool HasTarget => currentTarget != null;

    private void Awake()
    {
        // Pre-allocate overlap buffer
        overlapBuffer = new Collider[maxDetectedObjects];
        
        // Cache layer mask
        if (interactableLayer.value == 0)
        {
            // Fallback: set to "Interactable" layer if not configured
            int layer = LayerMask.NameToLayer("Interactable");
            if (layer >= 0)
                layerMaskValue = 1 << layer;
            else
                Debug.LogWarning("[PlayerInteraction] 'Interactable' layer not found! Configure the layer mask in the inspector.");
        }
        else
        {
            layerMaskValue = interactableLayer.value;
        }
        
        // Default detection origin
        if (detectionOrigin == null)
            detectionOrigin = transform;
    }

    private void Update()
    {
        // Throttled scanning
        scanTimer += Time.deltaTime;
        if (scanTimer >= scanInterval)
        {
            scanTimer = 0f;
            ScanForInteractables();
        }
    }

    /// <summary>
    /// Performs a physics overlap sphere to find the closest valid interactable.
    /// Uses hysteresis to prevent rapid target switching at range boundaries.
    /// </summary>
    private void ScanForInteractables()
    {
        Vector3 origin = detectionOrigin.position;
        
        // Use extended range if we already have a target (hysteresis)
        float scanRange = (currentTarget != null) 
            ? interactionRange + hysteresisBuffer 
            : interactionRange;
        
        // Perform overlap sphere with layer filter
        int hitCount = Physics.OverlapSphereNonAlloc(origin, scanRange, overlapBuffer, layerMaskValue);
        
        // Find the closest valid interactable
        IInteractable bestTarget = null;
        GameObject bestObject = null;
        float bestDistance = float.MaxValue;
        
        for (int i = 0; i < hitCount; i++)
        {
            Collider col = overlapBuffer[i];
            if (col == null) continue;
            
            GameObject obj = col.gameObject;
            
            // Check for IInteractable interface
            IInteractable interactable = obj.GetComponent<IInteractable>();
            if (interactable == null)
            {
                // Also check parent (for compound colliders)
                interactable = obj.GetComponentInParent<IInteractable>();
                if (interactable != null)
                    obj = ((MonoBehaviour)interactable).gameObject;
            }
            
            if (interactable == null || !interactable.CanInteract)
                continue;
            
            // Calculate distance
            float dist = Vector3.Distance(origin, obj.transform.position);
            
            // Prefer the current target slightly to prevent flickering
            if (interactable == currentTarget)
                dist -= 0.3f; // Bias toward current target
            
            if (dist < bestDistance)
            {
                bestDistance = dist;
                bestTarget = interactable;
                bestObject = obj;
            }
            
            // Clear buffer slot to avoid lingering references
            overlapBuffer[i] = null;
        }
        
        // Update target
        if (bestTarget != currentTarget)
        {
            SetTarget(bestTarget, bestObject);
        }
        else if (currentTarget != null)
        {
            // Keep calling OnFocused for current target
            currentTarget.OnFocused();
        }
    }

    /// <summary>
    /// Sets a new interaction target, handling UI and highlight transitions.
    /// </summary>
    private void SetTarget(IInteractable newTarget, GameObject newObject)
    {
        // Unfocus previous target
        if (currentTarget != null)
        {
            currentTarget.OnUnfocused();
            
            if (currentHighlight != null)
            {
                currentHighlight.SetFocused(false);
                currentHighlight = null;
            }
        }
        
        // Set new target
        currentTarget = newTarget;
        currentTargetObject = newObject;
        
        if (currentTarget != null)
        {
            // Focus new target
            currentTarget.OnFocused();
            
            // Enable highlight
            currentHighlight = newObject.GetComponent<InteractableHighlight>();
            if (currentHighlight != null)
                currentHighlight.SetFocused(true);
            
            // Show UI prompt
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowInteractionPrompt(currentTarget.InteractionPrompt);
            }
        }
        else
        {
            // Hide UI prompt
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideInteractionPrompt();
            }
        }
    }

    /// <summary>
    /// Force clears the current target. Call when an item is collected or destroyed.
    /// </summary>
    public void ClearTarget()
    {
        SetTarget(null, null);
    }

    private void OnDisable()
    {
        ClearTarget();
    }

#if UNITY_EDITOR
    /// <summary>
    /// Draws the detection range in the Scene view for debugging.
    /// Green = normal range, Yellow = hysteresis range.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = detectionOrigin != null ? detectionOrigin.position : transform.position;
        
        // Normal interaction range
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawWireSphere(origin, interactionRange);
        
        // Hysteresis range
        Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
        Gizmos.DrawWireSphere(origin, interactionRange + hysteresisBuffer);
    }
#endif
}
