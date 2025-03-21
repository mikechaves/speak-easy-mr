using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Controls a visual breathing guide that expands and contracts with
/// configurable timing to guide the user through breathing exercises.
/// </summary>
public class BreathingVisualizer : MonoBehaviour, StepBehavior
{
    [Header("References")]
    [SerializeField] private RectTransform breathCircle;
    [SerializeField] private Image breathCircleImage;
    [SerializeField] private AudioSource breathAudioSource;
    [SerializeField] private AudioClip inhalingSound;
    [SerializeField] private AudioClip exhalingSound;
    
    [Header("Breathing Pattern")]
    [SerializeField] private float inhaleDuration = 4f;
    [SerializeField] private float holdDuration = 2f;
    [SerializeField] private float exhaleDuration = 6f;
    [SerializeField] private int totalBreathCycles = 3;
    
    [Header("Visual Settings")]
    [SerializeField] private float minScale = 0.4f;
    [SerializeField] private float maxScale = 1.0f;
    [SerializeField] private Color inhaleColor = new Color(0.2f, 0.6f, 1.0f, 0.8f);
    [SerializeField] private Color holdColor = new Color(0.2f, 0.8f, 0.4f, 0.8f);
    [SerializeField] private Color exhaleColor = new Color(0.4f, 0.4f, 0.9f, 0.8f);
    [SerializeField] private AnimationCurve breathCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem breathParticles;
    
    private Coroutine breathingCoroutine;
    private bool isActive = false;
    private int currentCycle = 0;
    
    /// <summary>
    /// Implements the StepBehavior interface to start the breathing visualization
    /// </summary>
    public void ExecuteStep()
    {
        StartBreathingVisualization();
    }
    
    /// <summary>
    /// Starts the breathing visualization sequence
    /// </summary>
    public void StartBreathingVisualization()
    {
        if (isActive)
            return;
            
        isActive = true;
        currentCycle = 0;
        
        // Make sure the visualizer is visible
        gameObject.SetActive(true);
        
        // Stop any existing coroutine
        if (breathingCoroutine != null)
        {
            StopCoroutine(breathingCoroutine);
        }
        
        // Start the breathing animation
        breathingCoroutine = StartCoroutine(BreathingCycleRoutine());
    }
    
    /// <summary>
    /// Stops the breathing visualization
    /// </summary>
    public void StopBreathingVisualization()
    {
        if (!isActive)
            return;
            
        isActive = false;
        
        // Stop the coroutine
        if (breathingCoroutine != null)
        {
            StopCoroutine(breathingCoroutine);
            breathingCoroutine = null;
        }
        
        // Stop any audio
        if (breathAudioSource != null && breathAudioSource.isPlaying)
        {
            breathAudioSource.Stop();
        }
        
        // Stop particles
        if (breathParticles != null)
        {
            breathParticles.Stop();
        }
    }
    
    /// <summary>
    /// Main coroutine that controls the breathing cycle animation
    /// </summary>
    private IEnumerator BreathingCycleRoutine()
    {
        while (isActive && (totalBreathCycles <= 0 || currentCycle < totalBreathCycles))
        {
            // Start a new breath cycle
            currentCycle++;
            
            // Inhale phase
            yield return StartCoroutine(InhaleRoutine());
            
            // Hold phase
            yield return StartCoroutine(HoldRoutine());
            
            // Exhale phase
            yield return StartCoroutine(ExhaleRoutine());
            
            // Small pause between cycles
            yield return new WaitForSeconds(0.5f);
        }
        
        // When cycles are complete, reset and hide
        isActive = false;
        
        // Optionally hide the visualizer when complete
        // gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Handles the inhale phase of the breath cycle
    /// </summary>
    private IEnumerator InhaleRoutine()
    {
        // Play inhale sound
        if (breathAudioSource != null && inhalingSound != null)
        {
            breathAudioSource.clip = inhalingSound;
            breathAudioSource.Play();
        }
        
        // Start particles
        if (breathParticles != null)
        {
            var main = breathParticles.main;
            main.startColor = inhaleColor;
            breathParticles.Play();
        }
        
        // Set color
        if (breathCircleImage != null)
        {
            breathCircleImage.color = inhaleColor;
        }
        
        // Animate from min to max scale
        float elapsed = 0f;
        while (elapsed < inhaleDuration)
        {
            float t = elapsed / inhaleDuration;
            float curvedT = breathCurve.Evaluate(t);
            float scale = Mathf.Lerp(minScale, maxScale, curvedT);
            
            if (breathCircle != null)
            {
                breathCircle.localScale = new Vector3(scale, scale, scale);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we end at exactly the max scale
        if (breathCircle != null)
        {
            breathCircle.localScale = new Vector3(maxScale, maxScale, maxScale);
        }
    }
    
    /// <summary>
    /// Handles the hold phase of the breath cycle
    /// </summary>
    private IEnumerator HoldRoutine()
    {
        // Stop any audio
        if (breathAudioSource != null && breathAudioSource.isPlaying)
        {
            breathAudioSource.Stop();
        }
        
        // Change particles
        if (breathParticles != null)
        {
            var main = breathParticles.main;
            main.startColor = holdColor;
        }
        
        // Set color
        if (breathCircleImage != null)
        {
            breathCircleImage.color = holdColor;
        }
        
        // Hold at max scale
        if (breathCircle != null)
        {
            breathCircle.localScale = new Vector3(maxScale, maxScale, maxScale);
        }
        
        yield return new WaitForSeconds(holdDuration);
    }
    
    /// <summary>
    /// Handles the exhale phase of the breath cycle
    /// </summary>
    private IEnumerator ExhaleRoutine()
    {
        // Play exhale sound
        if (breathAudioSource != null && exhalingSound != null)
        {
            breathAudioSource.clip = exhalingSound;
            breathAudioSource.Play();
        }
        
        // Change particles
        if (breathParticles != null)
        {
            var main = breathParticles.main;
            main.startColor = exhaleColor;
        }
        
        // Set color
        if (breathCircleImage != null)
        {
            breathCircleImage.color = exhaleColor;
        }
        
        // Animate from max to min scale
        float elapsed = 0f;
        while (elapsed < exhaleDuration)
        {
            float t = elapsed / exhaleDuration;
            float curvedT = breathCurve.Evaluate(t);
            float scale = Mathf.Lerp(maxScale, minScale, curvedT);
            
            if (breathCircle != null)
            {
                breathCircle.localScale = new Vector3(scale, scale, scale);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we end at exactly the min scale
        if (breathCircle != null)
        {
            breathCircle.localScale = new Vector3(minScale, minScale, minScale);
        }
    }
}