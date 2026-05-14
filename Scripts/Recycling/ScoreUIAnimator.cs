using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Lightweight score UI animator that subscribes to ScoreManager events.
/// Provides satisfying pop + color flash when the score changes.
/// 
/// Features:
///   - Scale pop on score change (bouncy overshoot)
///   - Green flash for positive changes, red flash for negative
///   - Smooth settle animation
///   - No per-frame overhead (event-driven only)
/// 
/// Attach to: The same GameObject as the score Text element (e.g., HUD/ScoreText).
/// Auto-finds: The Text component on this GameObject.
/// </summary>
public class ScoreUIAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Scale multiplier at peak of pop animation.")]
    [Range(1.05f, 1.5f)]
    public float popScale = 1.25f;

    [Tooltip("Duration of the pop animation in seconds.")]
    [Range(0.1f, 0.6f)]
    public float popDuration = 0.3f;

    // ─── Colors ───
    private static readonly Color PositiveColor = new Color(0.2f, 0.9f, 0.3f);  // Green
    private static readonly Color NegativeColor = new Color(1f, 0.3f, 0.25f);    // Red
    private static readonly Color NormalColor = Color.white;

    // ─── Cached ───
    private RectTransform rectTransform;
    private Text scoreText;
    private Vector3 originalScale;
    private Coroutine activeAnimation;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        scoreText = GetComponent<Text>();

        if (rectTransform != null)
            originalScale = rectTransform.localScale;
    }

    private void Start()
    {
        // Subscribe to ScoreManager events
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += OnScoreChanged;
        }
        else
        {
            // Retry after a frame (ScoreManager may not be initialized yet)
            StartCoroutine(LateSubscribe());
        }
    }

    private IEnumerator LateSubscribe()
    {
        yield return null; // Wait one frame
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += OnScoreChanged;
        }
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= OnScoreChanged;
        }
    }

    /// <summary>
    /// Event handler — triggered whenever the score changes.
    /// </summary>
    private void OnScoreChanged(int newScore, int delta)
    {
        if (delta == 0) return;

        // Stop any existing animation
        if (activeAnimation != null)
            StopCoroutine(activeAnimation);

        activeAnimation = StartCoroutine(ScorePopRoutine(delta > 0));
    }

    /// <summary>
    /// Bouncy pop animation with color flash.
    /// </summary>
    private IEnumerator ScorePopRoutine(bool isPositive)
    {
        Color flashColor = isPositive ? PositiveColor : NegativeColor;

        // ── Phase 1: Scale up + color flash ──
        float halfDuration = popDuration * 0.35f;
        float time = 0f;

        while (time < halfDuration)
        {
            time += Time.deltaTime;
            float t = time / halfDuration;
            float easeOut = 1f - (1f - t) * (1f - t);

            if (rectTransform != null)
                rectTransform.localScale = Vector3.Lerp(originalScale, originalScale * popScale, easeOut);

            if (scoreText != null)
                scoreText.color = Color.Lerp(NormalColor, flashColor, easeOut);

            yield return null;
        }

        // ── Phase 2: Settle back with elastic bounce ──
        time = 0f;
        float settleDuration = popDuration * 0.65f;
        Vector3 peakScale = originalScale * popScale;

        while (time < settleDuration)
        {
            time += Time.deltaTime;
            float t = time / settleDuration;
            float elastic = 1f - Mathf.Pow(2f, -8f * t) * Mathf.Cos(t * Mathf.PI * 1.5f);

            if (rectTransform != null)
                rectTransform.localScale = Vector3.Lerp(peakScale, originalScale, elastic);

            if (scoreText != null)
                scoreText.color = Color.Lerp(flashColor, NormalColor, t);

            yield return null;
        }

        // ── Final reset ──
        if (rectTransform != null)
            rectTransform.localScale = originalScale;

        if (scoreText != null)
            scoreText.color = NormalColor;

        activeAnimation = null;
    }
}
