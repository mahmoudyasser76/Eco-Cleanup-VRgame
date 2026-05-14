using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Centralized Timer System to handle countdown, UI updates, warning states, and session end.
/// Stops recycling actions correctly at 00:00 without modifying interaction core components.
/// 
/// Attach to: A persistent GameObject in the scene (e.g., "ScoreManager" or "GameManager").
/// </summary>
public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    [Header("Timer Settings")]
    [Tooltip("Starting time in seconds (e.g., 150 = 2m 30s)")]
    public float startingTime = 150f;
    
    [Tooltip("Time in seconds when the warning state starts (e.g., 30s)")]
    public float warningTimeThreshold = 30f;

    [Header("State (Read-Only)")]
    [SerializeField] private float timeRemaining;
    [SerializeField] private bool isTimerRunning = false;
    [SerializeField] private bool isInWarningState = false;

    [Header("Audio Feedback")]
    [Tooltip("Sound effect to play natively strictly when the timer hits zero")]
    public AudioClip timeUpSound;
    private AudioSource audioSource;

    public float TimeRemaining => timeRemaining;
    public bool IsTimeUp => timeRemaining <= 0;

    // References
    private Text timerTextCache;
    private Color originalTimerColor = Color.white;
    private Vector3 originalTimerScale = Vector3.one;

    private float tickTimer = 0f;

    private void Awake()
    {
        // Resolve Audio dependencies securely
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("[TimerManager] Duplicate instance detected. Destroying this one.");
            Destroy(gameObject);
            return;
        }

        ResetTimer();
    }

    private void Start()
    {
        // Cache original UI state if available
        if (UIManager.Instance != null && UIManager.Instance.timerText != null)
        {
            timerTextCache = UIManager.Instance.timerText;
            originalTimerColor = timerTextCache.color;
            originalTimerScale = timerTextCache.transform.localScale;
        }

        StartTimer();
    }

    private void Update()
    {
        if (!isTimerRunning) return;

        timeRemaining -= Time.deltaTime;

        // UI Updates via UIManager
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateTimer(timeRemaining);
        }

        CheckWarningState();

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            HandleTimeUp();
        }
    }

    public void StartTimer()
    {
        if (timeRemaining > 0)
        {
            isTimerRunning = true;
            Debug.Log("[TimerManager] Timer started.");
        }
    }

    public void StopTimer()
    {
        isTimerRunning = false;
        Debug.Log("[TimerManager] Timer stopped.");
    }

    public void ResetTimer()
    {
        timeRemaining = startingTime;
        isTimerRunning = false;
        isInWarningState = false;

        // Reset UI if it was changed
        if (timerTextCache != null)
        {
            timerTextCache.color = originalTimerColor;
            timerTextCache.transform.localScale = originalTimerScale;
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateTimer(timeRemaining);
        }
    }

    private void CheckWarningState()
    {
        if (timeRemaining <= warningTimeThreshold && timeRemaining > 0)
        {
            if (!isInWarningState)
            {
                isInWarningState = true;
                Debug.Log("[TimerManager] Warning state activated (<= 30s)!");
            }

            // Warning visual effects
            if (timerTextCache != null)
            {
                // Turn text red
                timerTextCache.color = Color.red;

                // Simple pulse animation (between 1.0 and 1.25 scale)
                float pulse = 1f + 0.15f * Mathf.Abs(Mathf.Sin(Time.time * 6f));
                timerTextCache.transform.localScale = originalTimerScale * pulse;
            }

            // Subtle tick interval tracker (can be used to hook up audio later)
            tickTimer += Time.deltaTime;
            if (tickTimer >= 1f)
            {
                tickTimer -= 1f;
                // Optional: Play tick sound here if AudioSource exists
            }
        }
    }

    private void HandleTimeUp()
    {
        StopTimer();

        // 1. Reset UI from warning state pulse
        if (timerTextCache != null)
        {
            timerTextCache.transform.localScale = originalTimerScale;
            timerTextCache.color = Color.red; // Keep it red to signify time is up
        }

        // 2. Disable further recycling actions without modifying interaction scripts
        PlayerInteraction playerInteraction = FindFirstObjectByType<PlayerInteraction>();
        if (playerInteraction != null)
        {
            playerInteraction.enabled = false;
            // Force clear target to remove prompt if they were looking at a bin
            playerInteraction.ClearTarget(); 
            Debug.Log("[TimerManager] Disabled PlayerInteraction: Gameplay session ended cleanly.");
        }
        else
        {
            Debug.LogWarning("[TimerManager] Could not find PlayerInteraction to disable.");
        }

        // 3. Play Feedback Audio ONCE cleanly
        if (timeUpSound != null && audioSource != null)
        {
            // Setup strict bypass logic ignoring physics mixes
            audioSource.PlayOneShot(timeUpSound, 1.0f);
        }

        // 4. Show Feedback
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowFeedback("TIME UP!", Color.red);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
