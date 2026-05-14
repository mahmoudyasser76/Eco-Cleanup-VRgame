/// <summary>
/// Interface for all interactable objects in the game.
/// Implement this on any MonoBehaviour that should respond to player interaction.
/// Supports trash items, recycling bins, and future interactable types.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Display name shown in the interaction prompt UI.
    /// </summary>
    string InteractionPrompt { get; }

    /// <summary>
    /// Whether this object can currently be interacted with.
    /// </summary>
    bool CanInteract { get; }

    /// <summary>
    /// Called when the player confirms an interaction (e.g., presses E).
    /// </summary>
    void OnInteract();

    /// <summary>
    /// Called each frame while this object is the current interaction target.
    /// Use for visual highlighting or other per-frame feedback.
    /// </summary>
    void OnFocused();

    /// <summary>
    /// Called when this object is no longer the interaction target.
    /// Use to remove visual highlighting or clean up feedback.
    /// </summary>
    void OnUnfocused();
}
