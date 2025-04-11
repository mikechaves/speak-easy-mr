using UnityEngine;
using System.Collections;

/// <summary>
/// Animates the AI Assistant Orb, providing idle pulsing and visual feedback when TTS is active.
/// Assumes the orb's material has an Emission property that can be controlled.
/// </summary>
public class OrbAnimator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Renderer of the orb model. Used to access the material.")]
    [SerializeField] private Renderer orbRenderer;

    [Header("Idle Pulse Animation")]
    [Tooltip("Base color for emission when idle.")]
    [SerializeField] private Color idleEmissionColor = Color.white; // Adjust based on your 'warm white' light
    [Tooltip("Minimum emission intensity during idle pulse.")]
    [SerializeField] private float idleMinIntensity = 0.5f;
    [Tooltip("Maximum emission intensity during idle pulse.")]
    [SerializeField] private float idleMaxIntensity = 1.0f;
    [Tooltip("Speed of the idle pulse animation.")]
    [SerializeField] private float idlePulseSpeed = 1.0f;

    [Header("Speaking Animation")]
    [Tooltip("Emission color when TTS is speaking.")]
    [SerializeField] private Color speakingEmissionColor = new Color(1f, 1f, 0.8f); // Slightly warmer/brighter white
    [Tooltip("Emission intensity when TTS is speaking (can also pulse).")]
    [SerializeField] private float speakingIntensity = 1.5f;
     [Tooltip("Speed of the pulse animation while speaking.")]
    [SerializeField] private float speakingPulseSpeed = 1.8f;
    [Tooltip("Transition time when starting/stopping speech.")]
    [SerializeField] private float transitionDuration = 0.3f;

    // Material property block for efficient material changes
    private MaterialPropertyBlock propBlock;
    private Material orbMaterialInstance; // Instance of the material to modify
    private Coroutine currentAnimationCoroutine;
    private bool isSpeaking = false;

    // Shader property ID for emission color (common names, check your shader if needed)
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    // You might need a different ID for intensity if your shader separates them, e.g., "_EmissionIntensity"
    // For simplicity, we'll multiply the color by intensity here.

    void Awake()
    {
        propBlock = new MaterialPropertyBlock();

        if (orbRenderer == null)
        {
            orbRenderer = GetComponent<Renderer>();
            if (orbRenderer == null) {
                 Debug.LogError("OrbAnimator: Orb Renderer not found or assigned!", this);
                 enabled = false; // Disable script if no renderer
                 return;
            }
        }

        // Create an instance of the material to avoid modifying the asset file
        orbMaterialInstance = orbRenderer.material;
        // Ensure the material has emission enabled
        orbMaterialInstance.EnableKeyword("_EMISSION");
         Debug.Log("OrbAnimator: Material instance created and emission enabled.", this);
    }

    void Start()
    {
        // Start the idle animation by default
        StartIdleAnimation();
    }

     void OnEnable()
    {
        // Optional: Re-start idle animation if the object was disabled and re-enabled
        if (!isSpeaking) {
            StartIdleAnimation();
        }
    }

    void OnDisable()
    {
        // Stop any running animations when disabled
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }
    }

     // --- Public Methods to be called by TTSManager Events ---

    /// <summary>
    /// Call this when TTS playback starts.
    /// </summary>
    public void HandleSpeakStart()
    {
        if (isSpeaking) return; // Already in speaking state
        isSpeaking = true;
        Debug.Log("OrbAnimator: Handling Speak Start", this);

        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }
        currentAnimationCoroutine = StartCoroutine(AnimateOrb(true)); // Start speaking animation
    }

    /// <summary>
    /// Call this when TTS playback ends (completes, errors, or is aborted).
    /// </summary>
    public void HandleSpeakEnd()
    {
        if (!isSpeaking) return; // Already in idle state
        isSpeaking = false;
        Debug.Log("OrbAnimator: Handling Speak End", this);

        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }
        currentAnimationCoroutine = StartCoroutine(AnimateOrb(false)); // Transition back to idle animation
    }

    // --- Internal Animation Logic ---

    private void StartIdleAnimation() {
         if (currentAnimationCoroutine != null) {
             StopCoroutine(currentAnimationCoroutine);
         }
         // Check if enabled because Start() might call this before Awake finishes if setup is weird
         if (this.enabled && gameObject.activeInHierarchy) {
            currentAnimationCoroutine = StartCoroutine(AnimateOrb(false)); // Start idle animation
         }
    }


    /// <summary>
    /// Coroutine to handle the orb's pulsing animation (both idle and speaking).
    /// </summary>
    /// <param name="speaking">True if animating for speaking state, false for idle.</param>
    private IEnumerator AnimateOrb(bool speaking)
    {
        float targetMinIntensity = speaking ? speakingIntensity * 0.8f : idleMinIntensity; // Adjust speaking min intensity
        float targetMaxIntensity = speaking ? speakingIntensity : idleMaxIntensity;
        Color targetBaseColor = speaking ? speakingEmissionColor : idleEmissionColor;
        float targetPulseSpeed = speaking ? speakingPulseSpeed : idlePulseSpeed;

        // Get current values for smooth transition
        Color currentEmissionColor = Color.black; // Default start
         if (orbRenderer != null) {
             orbRenderer.GetPropertyBlock(propBlock);
             // Check if the property exists before getting it
             if (propBlock.HasColor(EmissionColorID)) {
                 // Approximating current intensity and color is tricky if they are combined.
                 // We'll transition the color directly and intensity via magnitude.
                 currentEmissionColor = propBlock.GetColor(EmissionColorID);
             } else if (orbMaterialInstance.HasProperty(EmissionColorID)){
                 currentEmissionColor = orbMaterialInstance.GetColor(EmissionColorID);
             }
         }
         // Estimate current intensity based on color magnitude - imperfect but works for transition
         float currentIntensityMagnitude = currentEmissionColor.maxColorComponent > 0 ? currentEmissionColor.grayscale / targetBaseColor.grayscale : 0; // Estimate based on brightness
         currentIntensityMagnitude = Mathf.Clamp(currentIntensityMagnitude, 0f, 5f); // Clamp unreasonable values

        float transitionElapsed = 0f;

        // --- Transition Phase ---
        while (transitionElapsed < transitionDuration)
        {
            transitionElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(transitionElapsed / transitionDuration);

            // Pulsate during transition
            float pulseFactor = (Mathf.Sin(Time.time * targetPulseSpeed) + 1f) / 2f; // 0 to 1 range
            float currentPulseIntensity = Mathf.Lerp(idleMinIntensity, targetMaxIntensity, pulseFactor); // Pulse towards target max

             // Lerp color and intensity separately during transition
             Color transitionColor = Color.Lerp(currentEmissionColor, targetBaseColor, t);
             float transitionIntensity = Mathf.Lerp(currentIntensityMagnitude, currentPulseIntensity, t);

            SetOrbEmission(transitionColor * transitionIntensity); // Apply combined color * intensity

            yield return null;
        }

         // --- Continuous Animation Phase ---
        while (isSpeaking == speaking) // Continue until state changes
        {
            // Calculate pulse based on current state (idle or speaking)
            float pulseFactor = (Mathf.Sin(Time.time * targetPulseSpeed) + 1f) / 2f; // 0 to 1 range
            float intensity = Mathf.Lerp(targetMinIntensity, targetMaxIntensity, pulseFactor);

            SetOrbEmission(targetBaseColor * intensity); // Apply combined color * intensity

            yield return null;
        }

        // If the loop exited because the state changed, the corresponding Handle method will start the new animation coroutine.
        currentAnimationCoroutine = null; // Mark as finished
    }


    /// <summary>
    /// Sets the emission color/intensity on the orb's material.
    /// </summary>
    private void SetOrbEmission(Color emissionColor)
    {
        if (orbRenderer == null || propBlock == null) return;

        // Use PropertyBlock for efficiency
        orbRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor(EmissionColorID, emissionColor * GetIntensityFactor()); // Multiply by intensity factor
        orbRenderer.SetPropertyBlock(propBlock);

         // Also set on the material instance if needed, though PropertyBlock should override
         // orbMaterialInstance.SetColor(EmissionColorID, emissionColor);
    }

    // Helper function to get intensity factor (adjust as needed based on HDR pipeline)
    private float GetIntensityFactor() {
        // For URP/HDRP, intensity might be handled differently.
        // This basic approach assumes intensity is baked into the color brightness.
        // You might return a fixed value like 1.0f if your shader handles intensity separately.
        return 1.0f; // Adjust if your shader uses a separate intensity property
    }
}
