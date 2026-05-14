using UnityEngine;
using TMPro;

/// <summary>
/// Lightweight world-space floating text popup for score and feedback messages.
/// Creates a temporary Canvas with TextMeshPro, animates it upward with
/// pop-in and fade-out, then self-destroys.
///
/// Usage:
///   FloatingFeedbackText.Spawn(position, "+10 Correct!", Color.green);
///
/// No prefab required — fully runtime-generated.
/// </summary>
public class FloatingFeedbackText : MonoBehaviour
{
    // ─── Configuration ───
    private string displayText;
    private Color textColor;
    private float duration;
    private float floatDistance;
    private float timer;

    // ─── References ───
    private TextMeshPro textMesh;
    private Vector3 startPos;

    // ─── Animation State ───
    private float popPhase = 0.15f; // Time for initial pop scale
    private Vector3 baseScale;

    /// <summary>
    /// Spawns a floating feedback text at the given world position.
    /// </summary>
    /// <param name="worldPos">World position to spawn the text.</param>
    /// <param name="text">Text to display (e.g., "+10 Correct!").</param>
    /// <param name="color">Text color.</param>
    /// <param name="duration">Total display duration in seconds.</param>
    /// <param name="floatDist">How far upward the text floats.</param>
    public static FloatingFeedbackText Spawn(Vector3 worldPos, string text, Color color,
                                              float duration = 1.2f, float floatDist = 1.5f)
    {
        // Create the floating text GameObject
        GameObject obj = new GameObject("FloatingFeedbackText");
        obj.transform.position = worldPos;

        FloatingFeedbackText fft = obj.AddComponent<FloatingFeedbackText>();
        fft.displayText = text;
        fft.textColor = color;
        fft.duration = duration;
        fft.floatDistance = floatDist;

        return fft;
    }

    private void Start()
    {
        // Add TextMeshPro component for crisp 3D world text
        textMesh = gameObject.AddComponent<TextMeshPro>();
        textMesh.text = displayText;
        textMesh.color = textColor;
        textMesh.fontSize = 6f;
        textMesh.fontStyle = FontStyles.Bold;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.enableWordWrapping = false;

        // Make the text render in world space and always face camera
        textMesh.sortingOrder = 100;

        // Configure RectTransform for world-space sizing
        RectTransform rt = textMesh.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = new Vector2(4f, 1.5f);
        }

        startPos = transform.position;

        // Initial scale (start small for pop effect)
        transform.localScale = Vector3.zero;
        baseScale = Vector3.one * 0.5f;

        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= duration)
        {
            Destroy(gameObject);
            return;
        }

        float normalizedTime = timer / duration;

        // ── Billboard: always face the camera ──
        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }

        // ── Position: float upward ──
        float floatProgress = EaseOutQuad(normalizedTime);
        transform.position = startPos + Vector3.up * (floatDistance * floatProgress);

        // ── Scale: pop in then hold ──
        if (timer < popPhase)
        {
            // Pop in: 0 → overshoot → settle
            float popT = timer / popPhase;
            float popEase = 1f + 0.3f * Mathf.Sin(popT * Mathf.PI); // Overshoot
            transform.localScale = baseScale * popEase * popT;
        }
        else
        {
            transform.localScale = baseScale;
        }

        // ── Alpha: hold then fade out in last 30% ──
        if (textMesh != null)
        {
            float alpha = 1f;
            if (normalizedTime > 0.7f)
            {
                alpha = 1f - ((normalizedTime - 0.7f) / 0.3f);
            }
            Color c = textMesh.color;
            c.a = Mathf.Clamp01(alpha);
            textMesh.color = c;
        }
    }

    /// <summary>
    /// Ease-out quadratic for smooth deceleration.
    /// </summary>
    private static float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }
}
