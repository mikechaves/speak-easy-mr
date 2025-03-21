using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages temporary feedback messages that appear in response to user actions.
/// Handles animated fade-in/out and positioning.
/// </summary>
public class FeedbackMessage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image messageBackground;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float displayDuration = 2.0f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Appearance")]
    [SerializeField] private Color successColor = new Color(0.2f, 0.7f, 0.2f, 0.8f);
    [SerializeField] private Color errorColor = new Color(0.7f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color neutralColor = new Color(0.2f, 0.2f, 0.7f, 0.8f);
    
    private Coroutine activeAnimation;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Initial state - invisible
        canvasGroup.alpha = 0;
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Shows a success message with animation
    /// </summary>
    /// <param name="message">Message text to display</param>
    public void ShowSuccessMessage(string message)
    {
        ShowMessage(message, MessageType.Success);
    }
    
    /// <summary>
    /// Shows an error message with animation
    /// </summary>
    /// <param name="message">Message text to display</param>
    public void ShowErrorMessage(string message)
    {
        ShowMessage(message, MessageType.Error);
    }
    
    /// <summary>
    /// Shows a neutral information message with animation
    /// </summary>
    /// <param name="message">Message text to display</param>
    public void ShowInfoMessage(string message)
    {
        ShowMessage(message, MessageType.Neutral);
    }
    
    /// <summary>
    /// Shows a message with the specified type and animates it
    /// </summary>
    /// <param name="message">Message text</param>
    /// <param name="type">Type of message (determines color)</param>
    public void ShowMessage(string message, MessageType type)
    {
        // Set the message text
        if (messageText != null)
        {
            messageText.text = message;
        }
        
        // Set the appropriate color based on message type
        Color backgroundColor = neutralColor;
        switch (type)
        {
            case MessageType.Success:
                backgroundColor = successColor;
                break;
            case MessageType.Error:
                backgroundColor = errorColor;
                break;
            case MessageType.Neutral:
            default:
                backgroundColor = neutralColor;
                break;
        }
        
        if (messageBackground != null)
        {
            messageBackground.color = backgroundColor;
        }
        
        // Stop any active animation
        if (activeAnimation != null)
        {
            StopCoroutine(activeAnimation);
        }
        
        // Start the show/hide animation
        activeAnimation = StartCoroutine(AnimateMessage());
    }
    
    /// <summary>
    /// Positions the message relative to another UI element
    /// </summary>
    /// <param name="referenceTransform">Transform to position relative to</param>
    /// <param name="offsetY">Vertical offset</param>
    public void PositionRelativeToPanel(Transform referenceTransform, float offsetY)
    {
        if (referenceTransform == null || rectTransform == null)
            return;
            
        // Position above the reference panel
        Vector3 position = referenceTransform.position + Vector3.up * offsetY;
        transform.position = position;
        transform.rotation = referenceTransform.rotation;
    }
    
    /// <summary>
    /// Animates the message appearance and disappearance
    /// </summary>
    private IEnumerator AnimateMessage()
    {
        gameObject.SetActive(true);
        
        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            canvasGroup.alpha = fadeCurve.Evaluate(elapsed / fadeInDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;
        
        // Display duration
        yield return new WaitForSeconds(displayDuration);
        
        // Fade out
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            canvasGroup.alpha = 1 - fadeCurve.Evaluate(elapsed / fadeOutDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0f;
        
        gameObject.SetActive(false);
        activeAnimation = null;
    }
}

/// <summary>
/// Enum representing the type of feedback message
/// </summary>
public enum MessageType
{
    Success,
    Error,
    Neutral
}