using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Handles Game Over state when the timer reaches 00:00.
/// Displays the UI, stops gameplay via Time.timeScale, unlocks cursor and provides restart functionality.
/// Saves score to leaderboard on game over.
/// </summary>
public class GameOverManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The main Game Over panel/overlay that gets activated.")]
    public GameObject gameOverPanel;
    
    [Tooltip("Text element displaying the final score.")]
    public TextMeshProUGUI finalScoreText;
    
    [Tooltip("The restart button to reload the scene.")]
    public Button restartButton;

    [Tooltip("Button to return to Main Menu.")]
    public Button mainMenuButton;

    [Header("Scene")]
    [Tooltip("Name of the Main Menu scene.")]
    public string mainMenuSceneName = "MainMenu";

    private bool isGameOverTriggered = false;

    private void Start()
    {
        // Ensure UI is hidden at start
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Hook up the buttons
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
    }

    private void Update()
    {
        // Wait until time is up
        if (!isGameOverTriggered && TimerManager.Instance != null && TimerManager.Instance.IsTimeUp)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        isGameOverTriggered = true;

        // 1. Freeze time to securely and globally stop standard updates, interactions, and physics
        Time.timeScale = 0f;

        // 2. Disable PlayerInput to fully disconnect movement, camera look, and background input
        PlayerInput playerInput = Object.FindFirstObjectByType<PlayerInput>();
        if (playerInput != null)
        {
            playerInput.enabled = false;
        }

        // 2.5 Force-disable StarterAssets cursor locking variables to prevent auto-relocating
        StarterAssets.StarterAssetsInputs starterInputs = Object.FindFirstObjectByType<StarterAssets.StarterAssetsInputs>();
        if (starterInputs != null)
        {
            starterInputs.cursorLocked = false;
            starterInputs.cursorInputForLook = false;
        }

        // 3. Unlock Cursor and make it visible so the player can effortlessly interact with the UI
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 4. Save score to leaderboard
        if (ScoreManager.Instance != null)
        {
            LeaderboardManager.SaveScore(ScoreManager.Instance.CurrentScore);
            Debug.Log($"[GameOverManager] Score {ScoreManager.Instance.CurrentScore} saved to leaderboard.");
        }

        // 5. Setup Final Score text exactly as requested (dark, centered, stacked)
        if (finalScoreText != null && ScoreManager.Instance != null)
        {
            finalScoreText.text = $"FINAL SCORE\n{ScoreManager.Instance.CurrentScore}";
        }

        // 6. Show UI 
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Debug.Log("[GameOverManager] Game Over Triggered. Gameplay frozen and Cursor unlocked for UI interaction.");
    }

    public void RestartGame()
    {
        // We MUST reset Time.timeScale before reloading to clear the frozen state
        Time.timeScale = 1f;

        // Soft reload the active scene resetting all stats perfectly
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
        Debug.Log("[GameOverManager] Restarting gameplay session.");
    }

    public void GoToMainMenu()
    {
        // Reset Time.timeScale before loading menu
        Time.timeScale = 1f;

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(mainMenuSceneName);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }

        Debug.Log("[GameOverManager] Returning to Main Menu.");
    }
}
