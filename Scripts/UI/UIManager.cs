using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("HUD Elements")]
    public Text scoreText;
    public Text timerText;
    
    [Header("Inventory Elements")]
    public Image inventoryIcon;
    public Text inventoryText;
    
    [Header("Interaction & Feedback")]
    public CanvasGroup interactionGroup;
    public Text interactionText;
    
    public CanvasGroup feedbackGroup;
    public Text feedbackText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if (interactionGroup) interactionGroup.alpha = 0;
        if (feedbackGroup) feedbackGroup.alpha = 0;
        
        // Initial setup
        UpdateScore(0);
        UpdateTimer(150f); // 02:30 default
        UpdateInventory(null, "No Item");
    }

    public void UpdateScore(int score)
    {
        if (scoreText) scoreText.text = score.ToString();
    }

    public void UpdateTimer(float timeRemaining)
    {
        if (timerText)
        {
            int minutes = Mathf.FloorToInt(Mathf.Max(0, timeRemaining) / 60);
            int seconds = Mathf.FloorToInt(Mathf.Max(0, timeRemaining) % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void UpdateInventory(Sprite icon, string itemName)
    {
        if (inventoryText) inventoryText.text = itemName;
        if (inventoryIcon) 
        {
            inventoryIcon.sprite = icon;
            inventoryIcon.enabled = icon != null;
        }
    }

    public void ShowInteractionPrompt(string promptText)
    {
        if (interactionText) interactionText.text = promptText;
        StopCoroutine("FadeInteraction");
        StartCoroutine(FadeInteraction(1f, 0.2f));
    }

    public void HideInteractionPrompt()
    {
        StopCoroutine("FadeInteraction");
        StartCoroutine(FadeInteraction(0f, 0.2f));
    }

    private IEnumerator FadeInteraction(float targetAlpha, float duration)
    {
        if (!interactionGroup) yield break;
        float startAlpha = interactionGroup.alpha;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            interactionGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }
        interactionGroup.alpha = targetAlpha;
    }

    public void ShowFeedback(string message, Color color)
    {
        if (feedbackText) 
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }
        StopCoroutine("FeedbackRoutine");
        StartCoroutine(FeedbackRoutine());
    }

    private IEnumerator FeedbackRoutine()
    {
        if (!feedbackGroup) yield break;
        
        feedbackGroup.transform.localScale = Vector3.one * 0.8f;
        feedbackGroup.alpha = 0;
        
        float time = 0;
        while(time < 0.2f)
        {
            time += Time.deltaTime;
            float t = time / 0.2f;
            feedbackGroup.alpha = t;
            feedbackGroup.transform.localScale = Vector3.Lerp(Vector3.one * 0.8f, Vector3.one * 1.1f, t);
            yield return null;
        }
        
        // Bounce back to 1.0
        time = 0;
        while(time < 0.1f)
        {
            time += Time.deltaTime;
            feedbackGroup.transform.localScale = Vector3.Lerp(Vector3.one * 1.1f, Vector3.one, time / 0.1f);
            yield return null;
        }
        feedbackGroup.transform.localScale = Vector3.one;
        
        yield return new WaitForSeconds(2f);
        
        time = 0;
        while(time < 0.3f)
        {
            time += Time.deltaTime;
            feedbackGroup.alpha = 1f - (time / 0.3f);
            yield return null;
        }
        feedbackGroup.alpha = 0f;
    }
}
