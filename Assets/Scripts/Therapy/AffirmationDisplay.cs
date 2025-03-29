using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Controls the display of affirmations during the affirmation practice step,
/// handling transitions between affirmations and visual effects.
/// </summary>
public class AffirmationDisplay : MonoBehaviour, StepBehavior
{
    [Header("UI References")]
    [SerializeField] private TMP_Text affirmationText;
    [SerializeField] private Image affirmationBackground;
    [SerializeField] private Transform affirmationPanel;
    
    [Header("Affirmations")]
    [SerializeField] private string[] affirmations = new string[]
    {
        "I am calm",
        "I am strong",
        "I am capable",
        "I embrace peace",
        "I am worthy"
    };
    
    [Header("Animation Settings")]
    [SerializeField] private float displayDuration = 5.0f;
    [SerializeField] private float transitionDuration = 1.0f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool autoAdvance = true;
    
    [Header("Visual Effects")]
    [SerializeField] private Color[] affirmationColors;
    [SerializeField] private float pulseAmount = 0.05f;
    [SerializeField] private float pulseSpeed = 1.0f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip transitionSound;
    
    private Coroutine displayCoroutine;
    private Coroutine pulseCoroutine;
    private int currentAffirmationIndex = -1;
    private bool isActive = false;
    
    private void Awake()
    {
        // Hide initially
        //gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Implements StepBehavior interface to start the affirmation display
    /// </summary>
    public void ExecuteStep()
    {
        StartAffirmationDisplay();
    }
    
    /// <summary>
    /// Starts the affirmation display sequence
    /// </summary>
    public void StartAffirmationDisplay()
    {
        if (isActive)
            return;
            
        isActive = true;
        //gameObject.SetActive(true);
        
        // Reset to first affirmation
        currentAffirmationIndex = -1;
        
        // Start coroutines
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }
        
        if (autoAdvance)
        {
            displayCoroutine = StartCoroutine(AutoAdvanceRoutine());
        }
        else
        {
            DisplayNextAffirmation();
        }
        
        // Start pulsing effect
        StartPulseEffect();
    }
    
    // ADD this new method
    public void StopStep()
    {
        Debug.Log("<color=red>AffirmationDisplay: StopStep() called by SessionController.</color>");
        StopAffirmationDisplay();
    }

     // MODIFY StopAffirmationDisplay to ADD BACK SetActive(false)
    public void StopAffirmationDisplay()
    {
        if (!isActive)
             return;

        Debug.Log("<color=red>AffirmationDisplay: StopAffirmationDisplay() called.</color>");
        isActive = false;

        if (displayCoroutine != null) {
            StopCoroutine(displayCoroutine);
            displayCoroutine = null;
        }
        StopPulseEffect();

        // ADD THIS BACK - Hide the display
        Debug.Log("<color=red>AffirmationDisplay: Setting GameObject Active = false in Stop function.</color>");
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Displays the next affirmation in the sequence
    /// </summary>
    public void DisplayNextAffirmation()
    {
        if (!isActive || affirmations.Length == 0)
            return;
            
        // Advance to next affirmation
        currentAffirmationIndex = (currentAffirmationIndex + 1) % affirmations.Length;
        
        // Update text
        if (affirmationText != null)
        {
            StartCoroutine(TransitionTextRoutine(affirmations[currentAffirmationIndex]));
        }
        
        // Update background color if available
        if (affirmationBackground != null && affirmationColors.Length > 0)
        {
            Color targetColor = affirmationColors[currentAffirmationIndex % affirmationColors.Length];
            StartCoroutine(TransitionColorRoutine(targetColor));
        }
        
        // Play sound
        if (audioSource != null && transitionSound != null)
        {
            audioSource.PlayOneShot(transitionSound);
        }
    }
    
    /// <summary>
    /// Automatically advances through affirmations at fixed intervals
    /// </summary>
    private IEnumerator AutoAdvanceRoutine()
    {
        // Start with the first affirmation
        DisplayNextAffirmation();
        
        while (isActive)
        {
            // Wait for display duration
            yield return new WaitForSeconds(displayDuration);
            
            // Show next affirmation
            DisplayNextAffirmation();
        }
    }
    
    /// <summary>
    /// Transitions affirmation text with fade effect
    /// </summary>
    private IEnumerator TransitionTextRoutine(string newText)
    {
        // Fade out
        float elapsed = 0f;
        while (elapsed < transitionDuration / 2)
        {
            float t = elapsed / (transitionDuration / 2);
            float alpha = 1 - transitionCurve.Evaluate(t);
            
            if (affirmationText != null)
            {
                affirmationText.alpha = alpha;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Change text
        if (affirmationText != null)
        {
            affirmationText.text = newText;
        }
        
        // Fade in
        elapsed = 0f;
        while (elapsed < transitionDuration / 2)
        {
            float t = elapsed / (transitionDuration / 2);
            float alpha = transitionCurve.Evaluate(t);
            
            if (affirmationText != null)
            {
                affirmationText.alpha = alpha;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we end fully visible
        if (affirmationText != null)
        {
            affirmationText.alpha = 1f;
        }
    }
    
    /// <summary>
    /// Transitions background color with smooth interpolation
    /// </summary>
    private IEnumerator TransitionColorRoutine(Color targetColor)
    {
        if (affirmationBackground == null)
            yield break;
            
        Color startColor = affirmationBackground.color;
        
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            float t = elapsed / transitionDuration;
            float curvedT = transitionCurve.Evaluate(t);
            
            affirmationBackground.color = Color.Lerp(startColor, targetColor, curvedT);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we end at target color
        affirmationBackground.color = targetColor;
    }
    
    /// <summary>
    /// Starts a gentle pulsing effect on the affirmation panel
    /// </summary>
    private void StartPulseEffect()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }
        
        pulseCoroutine = StartCoroutine(PulseEffectRoutine());
    }
    
    /// <summary>
    /// Stops the pulsing effect
    /// </summary>
    private void StopPulseEffect()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
        
        // Reset scale
        if (affirmationPanel != null)
        {
            affirmationPanel.localScale = Vector3.one;
        }
    }
    
    /// <summary>
    /// Creates a subtle pulsing animation on the affirmation panel
    /// </summary>
    private IEnumerator PulseEffectRoutine()
    {
        float time = 0f;
        
        while (isActive)
        {
            time += Time.deltaTime;
            
            // Calculate scale with sine wave
            float pulse = 1f + (Mathf.Sin(time * pulseSpeed) * pulseAmount);
            
            // Apply to panel
            if (affirmationPanel != null)
            {
                affirmationPanel.localScale = new Vector3(pulse, pulse, pulse);
            }
            
            yield return null;
        }
    }
}