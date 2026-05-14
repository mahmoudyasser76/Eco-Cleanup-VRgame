using UnityEngine;

/// <summary>
/// Handles all audio and visual feedback for recycling results.
/// Creates lightweight particle effects at runtime (no prefabs needed).
/// Plays success/wrong sounds and shows UI feedback messages.
/// 
/// Attach to: Player/PlayerCharacter (same object as RecyclingValidator).
/// </summary>
public class RecyclingFeedback : MonoBehaviour
{
    // ─── Audio ───
    [Header("Audio")]
    [Tooltip("Sound played on correct recycling.")]
    public AudioClip successClip;

    [Tooltip("Sound played on wrong recycling.")]
    public AudioClip wrongClip;

    [Tooltip("Volume for recycling feedback sounds.")]
    [Range(0f, 1f)]
    public float sfxVolume = 0.6f;

    private AudioSource audioSource;

    // ─── Particle Colors ───
    private static readonly Color SuccessColorA = new Color(0.2f, 0.9f, 0.4f, 1f);   // Green
    private static readonly Color SuccessColorB = new Color(0.6f, 1f, 0.7f, 1f);     // Light green
    private static readonly Color WrongColorA = new Color(1f, 0.25f, 0.2f, 1f);      // Red
    private static readonly Color WrongColorB = new Color(1f, 0.5f, 0.3f, 1f);       // Orange-red

    // ─── UI Feedback Colors ───
    private static readonly Color SuccessFeedbackColor = new Color(0.2f, 0.85f, 0.4f);
    private static readonly Color WrongFeedbackColor = new Color(1f, 0.35f, 0.25f);
    private static readonly Color NoItemFeedbackColor = new Color(0.7f, 0.7f, 0.75f);

    private void Awake()
    {
        // Ensure AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D for UI-like feedback
        }
    }

    /// <summary>
    /// Plays success feedback: sound, particles, and UI message.
    /// Routes through GameFeedbackManager for Cartoon FX Remaster VFX if available.
    /// </summary>
    /// <param name="worldPosition">Where to spawn the particle effect (bin position).</param>
    /// <param name="points">Points awarded (for UI display).</param>
    public void PlaySuccess(Vector3 worldPosition, int points)
    {
        // Route through centralized GameFeedbackManager (Cartoon FX Remaster VFX + enhanced audio)
        if (GameFeedbackManager.Instance != null)
        {
            GameFeedbackManager.Instance.PlaySuccessFeedback(worldPosition, points);
        }
        else
        {
            // Fallback: original behavior
            if (successClip != null && audioSource != null)
                audioSource.PlayOneShot(successClip, sfxVolume);

            SpawnParticleEffect(worldPosition, SuccessColorA, SuccessColorB, 20, 1.5f);

            if (UIManager.Instance != null)
                UIManager.Instance.ShowFeedback($"+{points} Correct Recycling!", SuccessFeedbackColor);
        }

        Debug.Log($"[RecyclingFeedback] ✓ Success feedback at {worldPosition}");
    }

    /// <summary>
    /// Plays wrong recycling feedback: sound, particles, and UI message.
    /// Routes through GameFeedbackManager for Cartoon FX Remaster VFX if available.
    /// </summary>
    /// <param name="worldPosition">Where to spawn the particle effect (bin position).</param>
    /// <param name="penalty">Points deducted (for UI display).</param>
    public void PlayWrong(Vector3 worldPosition, int penalty)
    {
        // Route through centralized GameFeedbackManager (Cartoon FX Remaster VFX + enhanced audio)
        if (GameFeedbackManager.Instance != null)
        {
            GameFeedbackManager.Instance.PlayWrongFeedback(worldPosition, penalty);
        }
        else
        {
            // Fallback: original behavior
            if (wrongClip != null && audioSource != null)
                audioSource.PlayOneShot(wrongClip, sfxVolume);

            SpawnParticleEffect(worldPosition, WrongColorA, WrongColorB, 12, 1f);

            if (UIManager.Instance != null)
                UIManager.Instance.ShowFeedback($"Wrong Bin! {penalty}", WrongFeedbackColor);
        }

        Debug.Log($"[RecyclingFeedback] ✗ Wrong feedback at {worldPosition}");
    }

    /// <summary>
    /// Shows subtle feedback when trying to recycle without holding an item.
    /// </summary>
    public void PlayNoItem()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ShowFeedback("No Item", NoItemFeedbackColor);

        Debug.Log("[RecyclingFeedback] No item to recycle.");
    }

    // ─── Particle System Factory ───

    /// <summary>
    /// Creates a lightweight, self-destroying particle burst at the given position.
    /// No prefabs required — fully runtime-generated.
    /// </summary>
    private void SpawnParticleEffect(Vector3 position, Color colorA, Color colorB, int burstCount, float lifetime)
    {
        // Create a temporary GameObject for the particle system
        GameObject particleObj = new GameObject("RecycleFX");
        particleObj.transform.position = position + Vector3.up * 1.2f; // Offset above bin

        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();

        // Stop the auto-playing default system so we can configure it
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // ── Main Module ──
        var main = ps.main;
        main.duration = 0.5f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
        main.startColor = new ParticleSystem.MinMaxGradient(colorA, colorB);
        main.maxParticles = burstCount;
        main.loop = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0.5f;
        main.stopAction = ParticleSystemStopAction.Destroy; // Auto-cleanup

        // ── Emission (single burst) ──
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, (short)burstCount)
        });

        // ── Shape (hemisphere for upward spread) ──
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.3f;

        // ── Color over Lifetime (fade out) ──
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient fadeGradient = new Gradient();
        fadeGradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(fadeGradient);

        // ── Size over Lifetime (shrink) ──
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));

        // ── Renderer (use default particle material) ──
        var renderer = particleObj.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.SetColor("_Color", colorA);

        // Play the effect
        ps.Play();

        // Safety destroy after lifetime (in case stopAction doesn't fire)
        Destroy(particleObj, lifetime + 0.5f);
    }
}
