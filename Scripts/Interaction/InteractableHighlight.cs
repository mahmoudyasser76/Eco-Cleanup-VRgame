using UnityEngine;

/// <summary>
/// Lightweight visual highlight component for interactable objects.
/// Applies a subtle emission pulse and scale bounce when focused.
/// Attach to any GameObject with a Renderer to enable visual feedback.
/// </summary>
public class InteractableHighlight : MonoBehaviour
{
    [Header("Highlight Settings")]
    [Tooltip("Color to apply as emission highlight when focused.")]
    public Color highlightColor = new Color(1f, 0.9f, 0.3f, 1f);

    [Tooltip("Intensity of the emission highlight.")]
    [Range(0f, 2f)]
    public float emissionIntensity = 0.5f;

    [Tooltip("Speed of the pulsing highlight animation.")]
    public float pulseSpeed = 3f;

    [Tooltip("Scale multiplier when focused (subtle bounce).")]
    [Range(1f, 1.2f)]
    public float focusScaleMultiplier = 1.05f;

    [Tooltip("Speed of the scale animation.")]
    public float scaleSpeed = 8f;

    private Renderer[] renderers;
    private MaterialPropertyBlock propBlock;
    private Vector3 originalScale;
    private bool isFocused;
    private float currentEmission;
    private float targetEmission;
    private float currentScaleFactor = 1f;

    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        propBlock = new MaterialPropertyBlock();
        originalScale = transform.localScale;
    }

    /// <summary>
    /// Enable the highlight effect with smooth transition.
    /// </summary>
    public void SetFocused(bool focused)
    {
        isFocused = focused;
        targetEmission = focused ? emissionIntensity : 0f;
    }

    private void Update()
    {
        if (!isFocused && currentEmission <= 0.001f && Mathf.Abs(currentScaleFactor - 1f) < 0.001f)
            return;

        // Smooth emission transition with pulse
        if (isFocused)
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.3f;
            currentEmission = Mathf.MoveTowards(currentEmission, targetEmission * pulse, Time.deltaTime * 3f);
        }
        else
        {
            currentEmission = Mathf.MoveTowards(currentEmission, 0f, Time.deltaTime * 4f);
        }

        // Apply emission via property block (no material instantiation)
        Color emission = highlightColor * currentEmission;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            renderers[i].GetPropertyBlock(propBlock);
            propBlock.SetColor(EmissionColorID, emission);
            renderers[i].SetPropertyBlock(propBlock);
        }

        // Smooth scale animation
        float targetScale = isFocused ? focusScaleMultiplier : 1f;
        currentScaleFactor = Mathf.Lerp(currentScaleFactor, targetScale, Time.deltaTime * scaleSpeed);
        transform.localScale = originalScale * currentScaleFactor;
    }

    private void OnDisable()
    {
        // Reset when disabled
        if (renderers != null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;
                renderers[i].GetPropertyBlock(propBlock);
                propBlock.SetColor(EmissionColorID, Color.black);
                renderers[i].SetPropertyBlock(propBlock);
            }
        }
        transform.localScale = originalScale;
        currentEmission = 0f;
        currentScaleFactor = 1f;
        isFocused = false;
    }
}
