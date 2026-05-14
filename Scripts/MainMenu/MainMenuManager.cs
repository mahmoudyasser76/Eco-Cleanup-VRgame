using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Central Main Menu controller.
/// Manages cursor, button actions, leaderboard panel, and UI fade-in.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button playButton;
    public Button leaderboardButton;
    public Button quitButton;
    public Button closeLeaderboardButton;

    [Header("Panels")]
    [Tooltip("The leaderboard popup panel (hidden by default).")]
    public GameObject leaderboardPanel;

    [Header("Leaderboard Display")]
    [Tooltip("TMP texts for the 5 score entries in the leaderboard.")]
    public TextMeshProUGUI[] leaderboardScoreTexts;

    [Header("Scene")]
    [Tooltip("Name of the gameplay scene to load.")]
    public string gameplaySceneName = "UrbanRecyclingCity";

    [Header("Fade In")]
    [Tooltip("CanvasGroup on the main UI for fade-in effect.")]
    public CanvasGroup mainCanvasGroup;
    public float fadeInDuration = 0.8f;

    private void Start()
    {
        // Cursor setup for Main Menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Ensure time is running
        Time.timeScale = 1f;

        // Wire up buttons
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);

        if (leaderboardButton != null)
            leaderboardButton.onClick.AddListener(OnLeaderboardClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        if (closeLeaderboardButton != null)
            closeLeaderboardButton.onClick.AddListener(OnCloseLeaderboardClicked);

        // Hide leaderboard panel
        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false);

        // Start fade in
        if (mainCanvasGroup != null)
        {
            StartCoroutine(FadeInUI());
        }

        // Destroy any leftover SceneTransitionManager duplicates
        // (it persists via DontDestroyOnLoad)
    }

    private void OnPlayClicked()
    {
        Debug.Log("[MainMenuManager] Play clicked. Loading gameplay scene...");

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(gameplaySceneName);
        }
        else
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
    }

    private void OnLeaderboardClicked()
    {
        Debug.Log("[MainMenuManager] Leaderboard clicked.");

        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);
            RefreshLeaderboardDisplay();
            StartCoroutine(AnimatePanel(leaderboardPanel, true));
        }
    }

    private void OnCloseLeaderboardClicked()
    {
        Debug.Log("[MainMenuManager] Close leaderboard clicked.");

        if (leaderboardPanel != null)
        {
            StartCoroutine(AnimatePanel(leaderboardPanel, false));
        }
    }

    private void OnQuitClicked()
    {
        Debug.Log("[MainMenuManager] Quit clicked.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Refreshes the leaderboard panel with current top scores.
    /// </summary>
    private void RefreshLeaderboardDisplay()
    {
        int[] scores;

        if (LeaderboardManager.Instance != null)
        {
            scores = LeaderboardManager.Instance.GetScores();
        }
        else
        {
            // Fallback: read directly from PlayerPrefs
            scores = new int[5];
            for (int i = 0; i < 5; i++)
            {
                scores[i] = PlayerPrefs.GetInt("LeaderboardScore_" + i, 0);
            }
        }

        if (leaderboardScoreTexts != null)
        {
            for (int i = 0; i < leaderboardScoreTexts.Length && i < scores.Length; i++)
            {
                if (leaderboardScoreTexts[i] != null)
                {
                    if (scores[i] > 0)
                        leaderboardScoreTexts[i].text = $"{i + 1}. {scores[i]}";
                    else
                        leaderboardScoreTexts[i].text = $"{i + 1}. ---";
                }
            }
        }
    }

    /// <summary>
    /// Smooth fade-in for the main UI canvas.
    /// </summary>
    private IEnumerator FadeInUI()
    {
        mainCanvasGroup.alpha = 0f;
        float timer = 0f;

        while (timer < fadeInDuration)
        {
            timer += Time.unscaledDeltaTime;
            mainCanvasGroup.alpha = timer / fadeInDuration;
            yield return null;
        }

        mainCanvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Animates a panel open (scale up) or closed (scale down + deactivate).
    /// </summary>
    private IEnumerator AnimatePanel(GameObject panel, bool open)
    {
        RectTransform rt = panel.GetComponent<RectTransform>();
        if (rt == null) yield break;

        float duration = 0.3f;
        float timer = 0f;

        if (open)
        {
            panel.SetActive(true);

            CanvasGroup cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();

            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                float t = timer / duration;
                float ease = 1f - Mathf.Pow(1f - t, 3f); // ease-out cubic

                rt.localScale = Vector3.one * ease;
                cg.alpha = ease;
                yield return null;
            }

            rt.localScale = Vector3.one;
            cg.alpha = 1f;
        }
        else
        {
            CanvasGroup cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();

            while (timer < duration)
            {
                timer += Time.unscaledDeltaTime;
                float t = timer / duration;
                float ease = 1f - t;

                rt.localScale = Vector3.one * ease;
                cg.alpha = ease;
                yield return null;
            }

            panel.SetActive(false);
            rt.localScale = Vector3.one;
            cg.alpha = 1f;
        }
    }
}
