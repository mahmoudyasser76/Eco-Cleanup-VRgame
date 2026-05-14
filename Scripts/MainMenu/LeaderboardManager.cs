using UnityEngine;

/// <summary>
/// Manages persistent local leaderboard using PlayerPrefs.
/// Stores and retrieves top 5 high scores sorted highest-to-lowest.
/// </summary>
public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    private const int MAX_SCORES = 5;
    private const string SCORE_KEY_PREFIX = "LeaderboardScore_";

    private int[] scores;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        LoadScores();
    }

    /// <summary>
    /// Static helper to save a score from anywhere (e.g., GameOverManager).
    /// Works even without an active LeaderboardManager instance.
    /// </summary>
    public static void SaveScore(int score)
    {
        if (Instance != null)
        {
            Instance.AddScore(score);
        }
        else
        {
            // Fallback: save directly to PlayerPrefs
            int[] currentScores = LoadScoresStatic();
            InsertAndSort(ref currentScores, score);
            SaveScoresStatic(currentScores);
        }
    }

    /// <summary>
    /// Adds a score to the leaderboard, sorts, and keeps only top entries.
    /// </summary>
    public void AddScore(int score)
    {
        InsertAndSort(ref scores, score);
        SaveAllScores();
        Debug.Log($"[LeaderboardManager] Score {score} added. Top score: {scores[0]}");
    }

    /// <summary>
    /// Returns the top scores array (highest first).
    /// </summary>
    public int[] GetScores()
    {
        if (scores == null) LoadScores();
        return scores;
    }

    /// <summary>
    /// Returns the highest score ever recorded.
    /// </summary>
    public int GetBestScore()
    {
        if (scores == null) LoadScores();
        return scores[0];
    }

    /// <summary>
    /// Returns the best score from PlayerPrefs (static, no instance needed).
    /// </summary>
    public static int GetBestScoreStatic()
    {
        return PlayerPrefs.GetInt(SCORE_KEY_PREFIX + "0", 0);
    }

    private void LoadScores()
    {
        scores = LoadScoresStatic();
    }

    private static int[] LoadScoresStatic()
    {
        int[] result = new int[MAX_SCORES];
        for (int i = 0; i < MAX_SCORES; i++)
        {
            result[i] = PlayerPrefs.GetInt(SCORE_KEY_PREFIX + i, 0);
        }
        return result;
    }

    private void SaveAllScores()
    {
        SaveScoresStatic(scores);
    }

    private static void SaveScoresStatic(int[] scoreArray)
    {
        for (int i = 0; i < MAX_SCORES; i++)
        {
            PlayerPrefs.SetInt(SCORE_KEY_PREFIX + i, scoreArray[i]);
        }
        PlayerPrefs.Save();
    }

    private static void InsertAndSort(ref int[] scoreArray, int newScore)
    {
        // Check if new score qualifies
        if (newScore <= scoreArray[MAX_SCORES - 1] && scoreArray[MAX_SCORES - 1] != 0)
            return;

        // Find insertion point
        for (int i = 0; i < MAX_SCORES; i++)
        {
            if (newScore > scoreArray[i])
            {
                // Shift scores down
                for (int j = MAX_SCORES - 1; j > i; j--)
                {
                    scoreArray[j] = scoreArray[j - 1];
                }
                scoreArray[i] = newScore;
                return;
            }
        }

        // If we get here and there's an empty slot, fill it
        for (int i = 0; i < MAX_SCORES; i++)
        {
            if (scoreArray[i] == 0)
            {
                scoreArray[i] = newScore;
                return;
            }
        }
    }

    /// <summary>
    /// Clears all leaderboard data (for debugging).
    /// </summary>
    public void ClearLeaderboard()
    {
        for (int i = 0; i < MAX_SCORES; i++)
        {
            PlayerPrefs.DeleteKey(SCORE_KEY_PREFIX + i);
        }
        PlayerPrefs.Save();
        LoadScores();
        Debug.Log("[LeaderboardManager] Leaderboard cleared.");
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
