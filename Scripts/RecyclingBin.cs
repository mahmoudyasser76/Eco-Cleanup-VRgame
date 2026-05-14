using UnityEngine;

/// <summary>
/// Interactable recycling bin endpoint for future recycling validation and scoring.
/// Stores the accepted trash category and exposes the standard interaction prompt.
/// </summary>
public class RecyclingBin : MonoBehaviour, IInteractable
{
    [Header("Bin Configuration")]
    [Tooltip("The trash category accepted by this recycling bin.")]
    public TrashCategory binType;

    [Tooltip("Optional point marking the visual drop area inside the open bin.")]
    public Transform dropPoint;

    [Header("Interaction State")]
    [Tooltip("Whether this recycling bin can currently be interacted with.")]
    public bool isInteractable = true;

    public string InteractionPrompt => "Press E to Recycle";

    public bool CanInteract => isInteractable;

    public void OnInteract()
    {
        if (RecyclingValidator.Instance != null)
        {
            RecyclingValidator.Instance.TryRecycle(this);
        }
        else
        {
            Debug.LogWarning("[RecyclingBin] RecyclingValidator not found in scene. Cannot process recycling.");
        }
    }

    public void OnFocused()
    {
        // Focus visuals are handled by InteractableHighlight.
    }

    public void OnUnfocused()
    {
        // Focus cleanup is handled by InteractableHighlight.
    }
}
