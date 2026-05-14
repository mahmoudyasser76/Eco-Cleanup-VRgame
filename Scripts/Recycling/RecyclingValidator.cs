using UnityEngine;

/// <summary>
/// Core recycling validation logic.
/// Compares the held item's TrashCategory against the RecyclingBin's binType.
/// Awards or penalizes score, triggers feedback, and clears the inventory.
/// 
/// Integration flow:
///   RecyclingBin.OnInteract() → RecyclingValidator.TryRecycle(bin)
///     → PlayerInventory (check + clear item)
///     → ScoreManager (add/subtract points)
///     → RecyclingFeedback (audio + VFX + UI)
/// 
/// Attach to: Player/PlayerCharacter (same object as PlayerInventory).
/// </summary>
public class RecyclingValidator : MonoBehaviour
{
    // ─── Singleton ───
    public static RecyclingValidator Instance { get; private set; }

    // ─── References ───
    [Header("References")]
    [Tooltip("Feedback handler for sounds and effects. Auto-found if not assigned.")]
    public RecyclingFeedback feedback;

    // ─── Stats (Read-Only) ───
    [Header("Session Stats")]
    [SerializeField] private int correctRecycles = 0;
    [SerializeField] private int wrongRecycles = 0;

    /// <summary>Number of correct recycling actions this session.</summary>
    public int CorrectRecycles => correctRecycles;

    /// <summary>Number of wrong recycling actions this session.</summary>
    public int WrongRecycles => wrongRecycles;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("[RecyclingValidator] Duplicate instance detected. Destroying this one.");
            Destroy(this);
            return;
        }

        // Auto-find feedback if not assigned
        if (feedback == null)
            feedback = GetComponent<RecyclingFeedback>();
    }

    /// <summary>
    /// Main entry point — called by RecyclingBin.OnInteract().
    /// Validates the held item against the bin type and processes the result.
    /// </summary>
    /// <param name="bin">The recycling bin the player is interacting with.</param>
    public void TryRecycle(RecyclingBin bin)
    {
        // ── Safety checks ──
        if (bin == null)
        {
            Debug.LogWarning("[RecyclingValidator] TryRecycle called with null bin.");
            return;
        }

        // ── Check if player is holding an item ──
        if (PlayerInventory.Instance == null || !PlayerInventory.Instance.HasItem())
        {
            // No item held — show subtle feedback and return
            if (feedback != null)
                feedback.PlayNoItem();

            Debug.Log("[RecyclingValidator] No item held. Cannot recycle.");
            return;
        }

        // ── Get the held item ──
        TrashItem heldItem = PlayerInventory.Instance.GetHeldItem();
        if (heldItem == null)
        {
            // Defensive: shouldn't happen if HasItem() returned true
            if (feedback != null)
                feedback.PlayNoItem();
            return;
        }

        // ── Determine bin world position for particle effects ──
        Vector3 effectPosition = bin.dropPoint != null
            ? bin.dropPoint.position
            : bin.transform.position;

        // ── VALIDATION: Compare item category vs bin type ──
        bool isCorrect = (heldItem.Category == bin.binType);

        if (isCorrect)
        {
            ProcessCorrectRecycle(heldItem, bin, effectPosition);
        }
        else
        {
            ProcessWrongRecycle(heldItem, bin, effectPosition);
        }

        // ── ALWAYS clear the held item (both correct and wrong) ──
        PlayerInventory.Instance.ClearItem();

        Debug.Log($"[RecyclingValidator] Recycle complete. Stats: {correctRecycles} correct, {wrongRecycles} wrong.");
    }

    /// <summary>
    /// Handles a correct recycling action.
    /// </summary>
    private void ProcessCorrectRecycle(TrashItem item, RecyclingBin bin, Vector3 effectPos)
    {
        correctRecycles++;

        // Award points
        int points = ScoreManager.CORRECT_RECYCLE_POINTS;
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(points);

        // Play success feedback
        if (feedback != null)
            feedback.PlaySuccess(effectPos, points);

        Debug.Log($"[RecyclingValidator] ✓ CORRECT: {item.DisplayName} ({item.Category}) → {bin.binType} bin. +{points} points.");
    }

    /// <summary>
    /// Handles a wrong recycling action.
    /// </summary>
    private void ProcessWrongRecycle(TrashItem item, RecyclingBin bin, Vector3 effectPos)
    {
        wrongRecycles++;

        // Apply penalty
        int penalty = ScoreManager.WRONG_RECYCLE_PENALTY;
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(penalty);

        // Play wrong feedback
        if (feedback != null)
            feedback.PlayWrong(effectPos, penalty);

        Debug.Log($"[RecyclingValidator] ✗ WRONG: {item.DisplayName} ({item.Category}) → {bin.binType} bin. {penalty} points.");
    }

    /// <summary>
    /// Resets session stats.
    /// </summary>
    public void ResetStats()
    {
        correctRecycles = 0;
        wrongRecycles = 0;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
