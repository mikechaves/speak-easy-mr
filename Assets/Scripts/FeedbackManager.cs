using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FeedbackManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text suggestionText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image statusIndicator;
    
    [Header("Audio Feedback")]
    [SerializeField] private AudioSource feedbackAudioSource;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip errorSound;
    [SerializeField] private AudioClip timeoutSound;
    
    [Header("Colors")]
    [SerializeField] private Color listeningColor = Color.green;
    [SerializeField] private Color idleColor = Color.grey;
    [SerializeField] private Color errorColor = Color.red;
    
    [Header("Animation")]
    [SerializeField] private float messageFadeTime = 3f;
    
    private Coroutine messageCoroutine;
    private CanvasGroup messageCanvasGroup;
    
    private void Awake()
    {
        // Make sure UI elements are properly set up
        if (statusText == null || statusIndicator == null)
        {
            Debug.LogError("Status UI elements not assigned in FeedbackManager");
        }
        
        // Set up message canvas group if needed
        if (messageText != null)
        {
            // Check if there's already a canvas group
            messageCanvasGroup = messageText.GetComponent<CanvasGroup>();
            if (messageCanvasGroup == null)
            {
                // Add a canvas group component if it doesn't exist
                messageCanvasGroup = messageText.gameObject.AddComponent<CanvasGroup>();
            }
            
            // Initially hide message
            if (messageText.gameObject.activeSelf)
            {
                messageText.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("Message text not assigned in FeedbackManager");
        }
        
        // Set up suggestion text
        if (suggestionText != null)
        {
            // Initially hide suggestion
            if (suggestionText.gameObject.activeSelf)
            {
                suggestionText.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("Suggestion text not assigned in FeedbackManager");
        }
    }
    
    public void UpdateStatusIndicator(bool isListening, string statusMessage)
    {
        if (statusText != null)
        {
            statusText.text = statusMessage;
        }
        
        if (statusIndicator != null)
        {
            statusIndicator.color = isListening ? listeningColor : idleColor;
        }
        
        Debug.Log($"Status updated: {statusMessage}, Listening: {isListening}");
    }
    
    public void PlaySuccessFeedback(string message)
    {
        if (feedbackAudioSource != null && successSound != null)
        {
            feedbackAudioSource.clip = successSound;
            feedbackAudioSource.Play();
        }
        
        ShowMessage(message);
        Debug.Log($"Success feedback: {message}");
    }
    
    public void PlayErrorFeedback(string message)
    {
        if (feedbackAudioSource != null && errorSound != null)
        {
            feedbackAudioSource.clip = errorSound;
            feedbackAudioSource.Play();
        }
        
        if (statusIndicator != null)
        {
            statusIndicator.color = errorColor;
        }
        
        ShowMessage(message);
        Debug.Log($"Error feedback: {message}");
        
        // Reset indicator color after delay
        StartCoroutine(ResetIndicatorColor());
    }
    
    public void PlayTimeoutFeedback(string message)
    {
        if (feedbackAudioSource != null && timeoutSound != null)
        {
            feedbackAudioSource.clip = timeoutSound;
            feedbackAudioSource.Play();
        }
        
        ShowMessage(message);
        Debug.Log($"Timeout feedback: {message}");
    }
    
    public void ShowSuggestion(string suggestion)
    {
        if (suggestionText == null)
        {
            Debug.LogError("Suggestion text not assigned in FeedbackManager");
            return;
        }
        
        suggestionText.text = suggestion;
        suggestionText.gameObject.SetActive(true);
        
        Debug.Log($"Showing suggestion: {suggestion}");
        
        // Hide suggestion after delay
        StartCoroutine(HideSuggestionAfterDelay());
    }
    
    public void ShowMessage(string message)
    {
        if (messageText == null)
        {
            Debug.LogError("Message text not assigned in FeedbackManager");
            return;
        }
        
        // Cancel existing fade if there is one
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }
        
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        
        // Set alpha through canvas group or text color
        if (messageCanvasGroup != null)
        {
            messageCanvasGroup.alpha = 1f;
        }
        else
        {
            // Fallback to color alpha if no canvas group
            Color color = messageText.color;
            color.a = 1f;
            messageText.color = color;
        }
        
        Debug.Log($"Showing message: {message}");
        
        // Start fading out the message
        messageCoroutine = StartCoroutine(FadeOutMessage());
    }
    
    private IEnumerator ResetIndicatorColor()
    {
        yield return new WaitForSeconds(1.5f);
        if (statusIndicator != null)
        {
            statusIndicator.color = idleColor;
        }
    }
    
    private IEnumerator HideSuggestionAfterDelay()
    {
        yield return new WaitForSeconds(5f);
        if (suggestionText != null)
        {
            suggestionText.gameObject.SetActive(false);
        }
    }
    
    private IEnumerator FadeOutMessage()
    {
        yield return new WaitForSeconds(2f);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < messageFadeTime)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / messageFadeTime);
            
            // Apply alpha through canvas group or text color
            if (messageCanvasGroup != null)
            {
                messageCanvasGroup.alpha = alpha;
            }
            else
            {
                // Fallback to color alpha if no canvas group
                Color color = messageText.color;
                color.a = alpha;
                messageText.color = color;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        messageText.gameObject.SetActive(false);
    }
}