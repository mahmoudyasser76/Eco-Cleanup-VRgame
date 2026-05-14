using UnityEngine;
using TMPro;

/// <summary>
/// Reads and displays the player's best score on the Main Menu.
/// Attach to the Best Score panel GameObject.
/// </summary>
public class HighScoreDisplay : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("TMP text showing the best score number.")]
    public TextMeshProUGUI bestScoreText;

    [Tooltip("TMP text showing the 'BEST SCORE' label (optional).")]
    public TextMeshProUGUI labelText;

    private void OnEnable()
    {
        RefreshDisplay();
    }

    /// <summary>
    /// Updates the displayed best score from PlayerPrefs.
    /// </summary>
    public void RefreshDisplay()
    {
        int bestScore = LeaderboardManager.GetBestScoreStatic();

        if (bestScoreText != null)
        {
            bestScoreText.text = bestScore.ToString();
        }

        if (labelText != null)
        {
            labelText.text = "BEST SCORE";
        }
    }
}
