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
    // ADD Awake and Start for logging if they don't exist
    private void Awake()
    {
        //Debug.Log("<color=lightblue>BreathingVisualizer: Awake() called.</color>");
        // Hide initially - This happens BEFORE Start can run
        //Debug.Log("<color=lightblue>BreathingVisualizer: Setting GameObject Active = false in Awake.</color>");
        //gameObject.SetActive(false);
    }

    private void Start() {
         Debug.Log("<color=lightblue>BreathingVisualizer: Start() called. (Should only happen if object becomes active)</color>");
    }

    // Modify ExecuteStep
    public void ExecuteStep()
    {
        Debug.Log("<color=green>BreathingVisualizer: ExecuteStep() called by SessionController.</color>"); // ADDED
        StartBreathingVisualization();
    }

    // Modify StartBreathingVisualization
    public void StartBreathingVisualization()
    {
        Debug.Log("<color=green>BreathingVisualizer: StartBreathingVisualization() called.</color>"); // ADDED
        if (isActive)
        {
            Debug.LogWarning("<color=orange>BreathingVisualizer: Start called but already active. Returning.</color>");
            return;
        }

        isActive = true;
        currentCycle = 0;

        // Log BEFORE SetActive
        Debug.Log("<color=lime>BreathingVisualizer: Attempting to set GameObject Active = true...</color>");
        //gameObject.SetActive(true);
        // Log AFTER SetActive, checking the actual state
        Debug.Log($"<color=lime>BreathingVisualizer: After SetActive(true), gameObject.activeSelf = {gameObject.activeSelf}, gameObject.activeInHierarchy = {gameObject.activeInHierarchy}</color>");

        // Stop any existing coroutine
        if (breathingCoroutine != null)
        {
            Debug.Log("<color=orange>BreathingVisualizer: Stopping existing breathing coroutine.</color>");
            StopCoroutine(breathingCoroutine);
        }

        // Start the breathing animation
        Debug.Log("<color=green>BreathingVisualizer: Starting BreathingCycleRoutine...</color>");
        breathingCoroutine = StartCoroutine(BreathingCycleRoutine());
    }

    public void StopStep()
    {
        Debug.Log("<color=red>BreathingVisualizer: StopStep() called by SessionController.</color>");
        StopBreathingVisualization();
    }

    // MODIFY StopBreathingVisualization to ADD BACK SetActive(false)
    public void StopBreathingVisualization()
    {
         Debug.Log("<color=red>BreathingVisualizer: StopBreathingVisualization() called.</color>");
        if (!isActive)
        {
            // If already inactive, ensure object is hidden too, then return
            if (!gameObject.activeSelf) return;
             // Otherwise, proceed to stop routines and hide
        }
        isActive = false;

        if (breathingCoroutine != null)
        {
            Debug.Log("<color=orange>BreathingVisualizer: Stopping breathing coroutine in Stop function.</color>");
            StopCoroutine(breathingCoroutine);
            breathingCoroutine = null;
        }

        if (breathAudioSource != null && breathAudioSource.isPlaying) {
            breathAudioSource.Stop();
        }
        if (breathParticles != null && breathParticles.isPlaying) {
            breathParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // ADD THIS BACK - Hide the GameObject when stopped
        Debug.Log("<color=red>BreathingVisualizer: Setting GameObject Active = false in Stop function.</color>");
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Main coroutine that controls the breathing cycle animation
    /// </summary>
    // --- Add Logs inside BreathingVisualizer.cs coroutines ---

    private IEnumerator BreathingCycleRoutine()
    {
        Debug.Log("<color=yellow>BreathingCycleRoutine: Starting loop.</color>"); // ADDED
        while (isActive && (totalBreathCycles <= 0 || currentCycle < totalBreathCycles))
        {
            currentCycle++;
            Debug.Log($"<color=yellow>BreathingCycleRoutine: Starting Cycle {currentCycle}/{totalBreathCycles}</color>"); // ADDED

            yield return StartCoroutine(InhaleRoutine());
            if (!isActive) break; // Exit if stopped during inhale

            yield return StartCoroutine(HoldRoutine());
             if (!isActive) break; // Exit if stopped during hold

            yield return StartCoroutine(ExhaleRoutine());
             if (!isActive) break; // Exit if stopped during exhale

            Debug.Log($"<color=yellow>BreathingCycleRoutine: Cycle {currentCycle} complete. Pausing.</color>"); // ADDED
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("<color=yellow>BreathingCycleRoutine: Loop finished or stopped.</color>"); // ADDED
        isActive = false; // This was potentially missing, ensure state resets if loop finishes
    }

    private IEnumerator InhaleRoutine()
    {
        Debug.Log("<color=lightblue>InhaleRoutine: Started.</color>"); // ADDED
        // ... (audio/particle logic) ...

        if (breathCircleImage != null) breathCircleImage.color = inhaleColor;
        // ADDED Log color
        Debug.Log($"<color=lightblue>InhaleRoutine: Set color to {inhaleColor}</color>");


        float elapsed = 0f;
        while (elapsed < inhaleDuration)
        {
            if (!isActive) yield break; // Check if stopped mid-animation

            float t = elapsed / inhaleDuration;
            float curvedT = breathCurve.Evaluate(t);
            float scale = Mathf.Lerp(minScale, maxScale, curvedT);

            if (breathCircle != null)
            {
                breathCircle.localScale = new Vector3(scale, scale, scale);
                 // ADDED Log applied scale periodically
                 if(Mathf.Approximately(elapsed % 0.5f, 0f) || elapsed == 0f) Debug.Log($"<color=lightblue>InhaleRoutine: Applying scale {scale:F2} (t={t:F2})</color>");
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (breathCircle != null) breathCircle.localScale = new Vector3(maxScale, maxScale, maxScale);
        Debug.Log("<color=lightblue>InhaleRoutine: Finished.</color>"); // ADDED
    }

    private IEnumerator HoldRoutine()
    {
        Debug.Log("<color=lightgreen>HoldRoutine: Started.</color>"); // ADDED
        // ... (audio/particle logic) ...

        if (breathCircleImage != null) breathCircleImage.color = holdColor;
         // ADDED Log color
        Debug.Log($"<color=lightgreen>HoldRoutine: Set color to {holdColor}</color>");

        if (breathCircle != null) breathCircle.localScale = new Vector3(maxScale, maxScale, maxScale);

        Debug.Log($"<color=lightgreen>HoldRoutine: Holding for {holdDuration}s.</color>"); // ADDED
        yield return new WaitForSeconds(holdDuration);
        Debug.Log("<color=lightgreen>HoldRoutine: Finished.</color>"); // ADDED
    }

    private IEnumerator ExhaleRoutine()
    {
         Debug.Log("<color=cyan>ExhaleRoutine: Started.</color>"); // ADDED
        // ... (audio/particle logic) ...

        if (breathCircleImage != null) breathCircleImage.color = exhaleColor;
         // ADDED Log color
        Debug.Log($"<color=cyan>ExhaleRoutine: Set color to {exhaleColor}</color>");


        float elapsed = 0f;
        while (elapsed < exhaleDuration)
        {
            if (!isActive) yield break; // Check if stopped mid-animation

            float t = elapsed / exhaleDuration;
            float curvedT = breathCurve.Evaluate(t);
            float scale = Mathf.Lerp(maxScale, minScale, curvedT);

            if (breathCircle != null)
            {
                breathCircle.localScale = new Vector3(scale, scale, scale);
                 // ADDED Log applied scale periodically
                 if(Mathf.Approximately(elapsed % 0.5f, 0f) || elapsed == 0f) Debug.Log($"<color=cyan>ExhaleRoutine: Applying scale {scale:F2} (t={t:F2})</color>");
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (breathCircle != null) breathCircle.localScale = new Vector3(minScale, minScale, minScale);
         Debug.Log("<color=cyan>ExhaleRoutine: Finished.</color>"); // ADDED
    }
    
}