using UnityEngine;

/// <summary>
/// Centralized score management singleton.
/// Tracks the player's recycling score and updates the HUD automatically.
/// 
/// Scoring rules:
///   +10 for correct recycling
///   -5  for wrong recycling
///   Minimum score is 0 (never goes negative)
/// 
/// Attach to: A persistent GameObject in the scene (e.g., "GameManagers").
/// </summary>
public class ScoreManager : MonoBehaviour
{
    // ─── Singleton ───
    public static ScoreManager Instance { get; private set; }

    // ─── State ───
    [Header("Score State (Read-Only)")]
    [SerializeField] private int currentScore = 0;

    /// <summary>
    /// The player's current score.
    /// </summary>
    public int CurrentScore => currentScore;

    // ─── Score Constants ───
    public const int CORRECT_RECYCLE_POINTS = 10;
    public const int WRONG_RECYCLE_PENALTY = -5;

    // ─── Events ───
    /// <summary>
    /// Fired when the score changes. Passes (newScore, delta).
    /// </summary>
    public System.Action<int, int> OnScoreChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("[ScoreManager] Duplicate instance detected. Destroying this one.");
            Destroy(gameObject);
            return;
        }

        // Initialize UI
        UpdateScoreUI();
    }

    /// <summary>
    /// Adds (or subtracts) points from the current score.
    /// Score is clamped to a minimum of 0.
    /// </summary>
    /// <param name="amount">Points to add. Use negative values for penalties.</param>
    public void AddScore(int amount)
    {
        int previousScore = currentScore;
        currentScore = Mathf.Max(0, currentScore + amount);

        int delta = currentScore - previousScore;

        UpdateScoreUI();
        OnScoreChanged?.Invoke(currentScore, delta);

        Debug.Log($"[ScoreManager] Score: {previousScore} → {currentScore} (delta: {(delta >= 0 ? "+" : "")}{delta})");
    }

    /// <summary>
    /// Resets the score to zero.
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreUI();
        OnScoreChanged?.Invoke(0, 0);
        Debug.Log("[ScoreManager] Score reset to 0.");
    }

    /// <summary>
    /// Pushes the current score value to the UI.
    /// </summary>
    private void UpdateScoreUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(currentScore);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
