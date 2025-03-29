using UnityEngine;
using System.Collections;

/// <summary>
/// Manages an environment for guided visualization therapy,
/// controlling scene transitions, ambient sounds, and visual elements.
/// </summary>
public class VisualizationEnvironment : MonoBehaviour, StepBehavior
{
    [Header("Environment References")]
    [SerializeField] private GameObject environmentObject;
    [SerializeField] private Light environmentLight;
    [SerializeField] private ParticleSystem environmentParticles;
    
    [Header("Audio")]
    [SerializeField] private AudioSource ambientAudioSource;
    [SerializeField] private float audioFadeDuration = 2.0f;
    
    [Header("Visual Transition")]
    [SerializeField] private float visualFadeDuration = 1.5f;
    [SerializeField] private Color environmentLightColor = new Color(0.9f, 0.8f, 0.6f, 1.0f);
    [SerializeField] private float environmentLightIntensity = 0.7f;
    
    [Header("Animation")]
    [SerializeField] private bool animateElements = true;
    [SerializeField] private float animationSpeed = 0.2f;
    [SerializeField] private float waveHeight = 0.1f;
    
    private Coroutine transitionCoroutine;
    private Coroutine animationCoroutine;
    private bool isActive = false;
    private Transform[] animatedObjects;
    private Vector3[] originalPositions;
    
    private void Awake()
    {
        // Initialize and hide the environment
        if (environmentObject != null)
        {
            environmentObject.SetActive(false);
        }
        
        // Collect animated objects if needed
        if (animateElements && environmentObject != null)
        {
            // Find elements to animate (e.g., waves, leaves, clouds)
            animatedObjects = environmentObject.GetComponentsInChildren<Transform>();
            originalPositions = new Vector3[animatedObjects.Length];
            
            // Store original positions
            for (int i = 0; i < animatedObjects.Length; i++)
            {
                originalPositions[i] = animatedObjects[i].localPosition;
            }
        }
    }

    // ADD this new method
    public void StopStep()
    {
        Debug.Log("<color=red>VisualizationEnvironment: StopStep() called by SessionController.</color>");
        // HideEnvironment handles disabling the child environment object,
        // We probably don't need to disable this root GameObject itself.
        HideEnvironment();
    }
    
    /// <summary>
    /// Implements StepBehavior interface to activate the visualization environment
    /// </summary>
    public void ExecuteStep()
    {
        ShowEnvironment();
    }
    
    /// <summary>
    /// Shows the visualization environment with a smooth transition
    /// </summary>
    public void ShowEnvironment()
    {
        if (isActive)
            return;
            
        isActive = true;
        
        // Make sure the object is active
        gameObject.SetActive(true);
        
        if (environmentObject != null)
        {
            environmentObject.SetActive(true);
        }
        
        // Stop any existing transition
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        
        // Start the transition
        transitionCoroutine = StartCoroutine(TransitionInRoutine());
        
        // Start animations if enabled
        if (animateElements)
        {
            StartAnimations();
        }
    }
    
    /// <summary>
    /// Hides the visualization environment with a smooth transition
    /// </summary>
    public void HideEnvironment()
    {
        if (!isActive)
            return;
            
        isActive = false;
        
        // Stop any existing transition
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        
        // Start the out transition
        transitionCoroutine = StartCoroutine(TransitionOutRoutine());
        
        // Stop animations
        StopAnimations();
    }
    
    /// <summary>
    /// Transitions the environment in with audio and visual fades
    /// </summary>
    private IEnumerator TransitionInRoutine()
    {
        // Start with low light
        if (environmentLight != null)
        {
            environmentLight.intensity = 0f;
            environmentLight.color = environmentLightColor;
        }
        
        // Start audio at zero volume
        if (ambientAudioSource != null)
        {
            ambientAudioSource.volume = 0f;
            ambientAudioSource.Play();
        }
        
        // Start particles
        if (environmentParticles != null)
        {
            environmentParticles.Play();
        }
        
        // Fade in visuals and audio
        float elapsed = 0f;
        while (elapsed < visualFadeDuration)
        {
            float t = elapsed / visualFadeDuration;
            
            // Fade in light
            if (environmentLight != null)
            {
                environmentLight.intensity = Mathf.Lerp(0f, environmentLightIntensity, t);
            }
            
            // Fade in audio
            if (ambientAudioSource != null)
            {
                ambientAudioSource.volume = Mathf.Lerp(0f, 1f, t);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we end at full values
        if (environmentLight != null)
        {
            environmentLight.intensity = environmentLightIntensity;
        }
        
        if (ambientAudioSource != null)
        {
            ambientAudioSource.volume = 1f;
        }
    }
    
    /// <summary>
    /// Transitions the environment out with audio and visual fades
    /// </summary>
    private IEnumerator TransitionOutRoutine()
    {
        // Fade out visuals and audio
        float elapsed = 0f;
        while (elapsed < visualFadeDuration)
        {
            float t = elapsed / visualFadeDuration;
            
            // Fade out light
            if (environmentLight != null)
            {
                environmentLight.intensity = Mathf.Lerp(environmentLightIntensity, 0f, t);
            }
            
            // Fade out audio
            if (ambientAudioSource != null)
            {
                ambientAudioSource.volume = Mathf.Lerp(1f, 0f, t);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Stop audio
        if (ambientAudioSource != null)
        {
            ambientAudioSource.Stop();
        }
        
        // Stop particles
        if (environmentParticles != null)
        {
            environmentParticles.Stop();
        }
        
        // Hide the environment
        if (environmentObject != null)
        {
            environmentObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Starts subtle animations for environmental elements
    /// </summary>
    private void StartAnimations()
    {
        if (!animateElements || animatedObjects == null)
            return;
            
        // Stop any existing animation
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        
        // Start gentle animation coroutine
        animationCoroutine = StartCoroutine(AnimateEnvironmentRoutine());
    }
    
    /// <summary>
    /// Stops all environment animations
    /// </summary>
    private void StopAnimations()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        
        // Reset positions
        if (animatedObjects != null && originalPositions != null)
        {
            for (int i = 0; i < animatedObjects.Length; i++)
            {
                if (i < originalPositions.Length && animatedObjects[i] != null)
                {
                    animatedObjects[i].localPosition = originalPositions[i];
                }
            }
        }
    }
    
    /// <summary>
    /// Animates environment elements with gentle movements
    /// </summary>
    private IEnumerator AnimateEnvironmentRoutine()
    {
        float time = 0f;
        
        while (isActive)
        {
            time += Time.deltaTime;
            
            // Animate each relevant object
            for (int i = 0; i < animatedObjects.Length; i++)
            {
                if (i < originalPositions.Length && animatedObjects[i] != null)
                {
                    // Skip the parent object and any cameras or lights
                    if (animatedObjects[i] == transform || 
                        animatedObjects[i].GetComponent<Camera>() != null ||
                        animatedObjects[i].GetComponent<Light>() != null)
                    {
                        continue;
                    }
                    
                    // Different phase for each object
                    float phase = i * 0.42f;
                    
                    // Simple wave motion
                    Vector3 newPos = originalPositions[i];
                    newPos.y += Mathf.Sin(time * animationSpeed + phase) * waveHeight;
                    
                    animatedObjects[i].localPosition = newPos;
                }
            }
            
            yield return null;
        }
    }
}