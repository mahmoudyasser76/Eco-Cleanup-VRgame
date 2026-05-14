using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Provides polished cartoon-style hover/click animations for menu buttons.
/// Attach to any Button GameObject for automatic scale animations and audio feedback.
/// </summary>
public class MenuButtonAnimator : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Settings")]
    [Tooltip("Scale multiplier on hover.")]
    public float hoverScale = 1.08f;

    [Tooltip("Scale multiplier on press.")]
    public float pressScale = 0.95f;

    [Tooltip("Speed of scale animation.")]
    public float animationSpeed = 12f;

    [Header("Audio")]
    [Tooltip("Play hover sound via MenuAudioManager.")]
    public bool playHoverSound = true;

    [Tooltip("Play click sound via MenuAudioManager.")]
    public bool playClickSound = true;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isHovered = false;
    private bool isPressed = false;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform != null ? rectTransform.localScale : transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        Vector3 currentScale = rectTransform != null ? rectTransform.localScale : transform.localScale;
        Vector3 newScale = Vector3.Lerp(currentScale, targetScale, Time.unscaledDeltaTime * animationSpeed);

        if (rectTransform != null)
            rectTransform.localScale = newScale;
        else
            transform.localScale = newScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        if (!isPressed)
        {
            targetScale = originalScale * hoverScale;
        }

        if (playHoverSound)
        {
            MenuAudioManager.TriggerHover();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        if (!isPressed)
        {
            targetScale = originalScale;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        targetScale = originalScale * pressScale;

        if (playClickSound)
        {
            MenuAudioManager.TriggerClick();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        targetScale = isHovered ? originalScale * hoverScale : originalScale;
    }

    private void OnDisable()
    {
        // Reset scale when disabled
        if (rectTransform != null)
            rectTransform.localScale = originalScale;
        else if (transform != null)
            transform.localScale = originalScale;

        isHovered = false;
        isPressed = false;
        targetScale = originalScale;
    }
}
