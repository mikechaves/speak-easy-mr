using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages the status indicator that shows the current voice recognition status.
/// Provides visual feedback about system state to the user.
/// </summary>
public class StatusIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Image statusBackground;
    [SerializeField] private Image statusIcon;
    
    [Header("Status Colors")]
    [SerializeField] private Color listeningColor = Color.green;
    [SerializeField] private Color idleColor = Color.grey;
    [SerializeField] private Color errorColor = Color.red;
    [SerializeField] private Color processingColor = Color.yellow;
    
    [Header("Animation")]
    [SerializeField] private bool pulseWhenListening = true;
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float pulseMinAlpha = 0.6f;
    [SerializeField] private float pulseMaxAlpha = 1.0f;
    
    private Coroutine pulseCoroutine;
    private RectTransform rectTransform;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    
    /// <summary>
    /// Updates the status indicator with the current recognition state
    /// </summary>
    /// <param name="status">Current recognition status</param>
    /// <param name="statusMessage">Message to display</param>
    public void UpdateStatus(VoiceRecognitionStatus status, string statusMessage)
    {
        // Update text
        if (statusText != null)
        {
            statusText.text = statusMessage;
        }
        
        // Update colors based on status
        Color targetColor = idleColor;
        
        switch (status)
        {
            case VoiceRecognitionStatus.Listening:
                targetColor = listeningColor;
                StartPulseEffect();
                break;
            case VoiceRecognitionStatus.Processing:
                targetColor = processingColor;
                StopPulseEffect();
                break;
            case VoiceRecognitionStatus.Error:
                targetColor = errorColor;
                StopPulseEffect();
                break;
            case VoiceRecognitionStatus.Idle:
            default:
                targetColor = idleColor;
                StopPulseEffect();
                break;
        }
        
        // Apply color to indicator
        if (statusIcon != null)
        {
            statusIcon.color = targetColor;
        }
        
        if (statusBackground != null)
        {
            statusBackground.color = new Color(targetColor.r * 0.3f, targetColor.g * 0.3f, targetColor.b * 0.3f, 0.8f);
        }
    }
    
    /// <summary>
    /// Positions the status indicator relative to the instruction panel
    /// </summary>
    /// <param name="referenceTransform">Transform to position relative to</param>
    /// <param name="offsetY">Vertical offset</param>
    public void PositionRelativeToPanel(Transform referenceTransform, float offsetY)
    {
        if (referenceTransform == null || rectTransform == null)
            return;
            
        // Position below the instruction panel
        Vector3 position = referenceTransform.position + Vector3.down * offsetY;
        transform.position = position;
        transform.rotation = referenceTransform.rotation;
    }
    
    private void StartPulseEffect()
    {
        if (!pulseWhenListening || statusIcon == null)
            return;
            
        // Stop any existing pulse effect
        StopPulseEffect();
        
        // Start new pulse effect
        pulseCoroutine = StartCoroutine(PulseRoutine());
    }
    
    private void StopPulseEffect()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
        
        // Reset alpha
        if (statusIcon != null)
        {
            Color iconColor = statusIcon.color;
            statusIcon.color = new Color(iconColor.r, iconColor.g, iconColor.b, 1.0f);
        }
    }
    
    private IEnumerator PulseRoutine()
    {
        float t = 0;
        
        while (true)
        {
            // Calculate alpha based on sine wave
            float alpha = Mathf.Lerp(pulseMinAlpha, pulseMaxAlpha, (Mathf.Sin(t * pulseSpeed) + 1) / 2);
            
            // Apply alpha to icon
            if (statusIcon != null)
            {
                Color iconColor = statusIcon.color;
                statusIcon.color = new Color(iconColor.r, iconColor.g, iconColor.b, alpha);
            }
            
            t += Time.deltaTime;
            yield return null;
        }
    }
}

/// <summary>
/// Enum representing the possible states of voice recognition
/// </summary>
public enum VoiceRecognitionStatus
{
    Idle,
    Listening,
    Processing,
    Error
}