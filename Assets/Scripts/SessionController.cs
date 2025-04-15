using System.Collections;
using UnityEngine;
using TMPro;

public enum SessionState
{
    Idle,
    Active,
    Complete
}

/// <summary>
/// Manages the therapy session flow. Relies on VoiceCommandManager being activated
/// externally (e.g., by a keyword "Okay") when commands are expected during the Active state.
/// </summary>
public class SessionController : MonoBehaviour
{
    [Header("Session Messages")]
    [Tooltip("The welcome message displayed and spoken when the session is idle.")]
    [TextArea(3, 5)]
    [SerializeField] private string welcomeMessageText = "Welcome to Voice-Driven Therapy.\nSay \"Okay\" then \"Begin session\" to start.";
    [Tooltip("The completion message displayed and spoken when the session ends.")]
    [TextArea(3, 5)]
    [SerializeField] private string completionMessageText = "Session complete. Thank you for participating.";

    [Header("Therapy Steps Configuration")]
    [Tooltip("The array containing the actual therapy steps (e.g., Breathing, Visualization). Should NOT include Welcome or Completion steps.")]
    [SerializeField] private TherapyStep[] therapySteps; // Should contain the 5 active steps

    [Header("References")]
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private VoiceCommandManager voiceCommandManager; // Still needed for StopListening on EndSession
    [SerializeField] private FeedbackManager feedbackManager;
    [SerializeField] private MonoBehaviour enhancedUI; // Optional enhanced UI reference

    [Header("UI Positioning")]
    [SerializeField] private Transform therapyEnvironmentRoot; // Parent object for UI/environment
    [SerializeField] private float defaultDistance = 2.0f; // Initial distance from user
    [SerializeField] private float defaultHeight = 0.0f; // Initial height relative to user eye level

    private SessionState currentState = SessionState.Idle;
    // Index for the therapySteps array (0 to Length-1 for the active steps)
    private int currentStepIndex = -1;

    /// <summary>
    /// Called when the script instance is first loaded.
    /// Performs checks and initializes the idle state.
    /// </summary>
    private void Start()
    {
        if (!PerformChecks()) {
             Debug.LogError("SessionController startup checks failed. Disabling component.", this);
             enabled = false; // Disable component if critical references are missing
             return;
        }
        ShowIdleInstructions(); // Show the separate welcome message
    }

    /// <summary>
    /// Performs initial reference and configuration checks.
    /// </summary>
    /// <returns>True if essential checks pass, false otherwise.</returns>
    private bool PerformChecks() {
        bool checksPassed = true;
        if (instructionText == null) { Debug.LogError("Instruction Text not assigned!", this); checksPassed = false; }
        if (voiceCommandManager == null) { Debug.LogError("Voice Command Manager not assigned!", this); checksPassed = false; }
        if (feedbackManager == null) { Debug.LogError("Feedback Manager not assigned!", this); checksPassed = false; }
        if (therapyEnvironmentRoot == null) { Debug.LogWarning("Therapy Environment Root not assigned! UI positioning may fail.", this); } // Warning, not critical error
        if (therapySteps == null || therapySteps.Length == 0) { Debug.LogError("Therapy Steps array is not assigned or empty!", this); checksPassed = false; }

        // Check individual steps
        if (therapySteps != null) {
            for (int i = 0; i < therapySteps.Length; i++) {
                if (therapySteps[i] == null) {
                     Debug.LogWarning($"Therapy Step {i} in the array is null!", this);
                } else if (therapySteps[i].stepBehaviorComponent != null) {
                     if (therapySteps[i].stepBehaviorComponent.GetComponent<StepBehavior>() == null) {
                         Debug.LogError($"Therapy Step {i}: Assigned component '{therapySteps[i].stepBehaviorComponent.name}' does NOT implement StepBehavior!", this);
                         checksPassed = false;
                     }
                }
            }
        }
        return checksPassed;
     }

    /// <summary>
    /// Gets the current state of the session.
    /// </summary>
    public SessionState GetCurrentState() { return currentState; }

    /// <summary>
    /// Gets the index of the currently active therapy step within the therapySteps array.
    /// Returns -1 if the session is Idle or Complete.
    /// </summary>
    public int GetCurrentStepIndex() { return currentStepIndex; }

    /// <summary>
    /// Starts the therapy session, advancing to the first step.
    /// Called by VoiceCommandManager when a start command is detected.
    /// </summary>
    public void StartSession()
    {
        if (currentState == SessionState.Active) {
            Debug.LogWarning("StartSession called but session is already active.", this);
            feedbackManager?.PlayErrorFeedback("Session already in progress");
            return;
        }
        if (therapySteps == null || therapySteps.Length == 0) {
             Debug.LogError("StartSession: Cannot start, no therapy steps defined!", this);
             feedbackManager?.PlayErrorFeedback("Session configuration error.");
             return;
        }

        currentState = SessionState.Active;
        currentStepIndex = -1; // Start before the first step (index 0)
        Debug.Log($"StartSession: State set to Active. Calling AdvanceToNextStep to go to first therapy step.", this);
        AdvanceToNextStep();
    }


    /// <summary>
    /// Advances the session to the next therapy step in the array.
    /// Called by VoiceCommandManager when a "next step" command is detected.
    /// </summary>
    public void AdvanceToNextStep()
    {
        if (currentState != SessionState.Active) {
            if (currentState != SessionState.Complete) {
                 Debug.LogWarning("AdvanceToNextStep called while session not active.", this);
                 feedbackManager?.PlayErrorFeedback("No active session");
            }
            return;
        }

        currentStepIndex++; // Move to the next index
        Debug.Log($"AdvanceToNextStep: Incremented index to {currentStepIndex}");

        // Check if we've finished the last step in the array
        if (currentStepIndex >= therapySteps.Length) {
            Debug.Log($"AdvanceToNextStep: Completed last therapy step (index {currentStepIndex - 1}). Completing session.", this);
            CompleteSession(); // Trigger session completion
            return;
        }

        // Check if the target step data is valid
         if (therapySteps == null || currentStepIndex < 0 || currentStepIndex >= therapySteps.Length || therapySteps[currentStepIndex] == null) {
             Debug.LogError($"AdvanceToNextStep: Invalid step index or data ({currentStepIndex}) after increment. Cannot display step. Completing session.", this);
             CompleteSession(); // End session if configuration is broken
             return;
         }

        DisplayCurrentStep(); // Display the therapy step (index 0 to Length-1)

        // Update Enhanced UI Logic
        if (enhancedUI != null)
        {
            // Ensure index is valid before accessing instructions
            if (currentStepIndex >= 0 && currentStepIndex < therapySteps.Length && therapySteps[currentStepIndex] != null) {
                 int displayStepNum = currentStepIndex + 1; // Show 1-5 for progress
                 int totalDisplaySteps = therapySteps.Length; // Total is 5 active steps

                 enhancedUI.SendMessage("ShowActiveState", therapySteps[currentStepIndex].instructions, SendMessageOptions.DontRequireReceiver);
                 object[] progressParams = new object[] { displayStepNum, totalDisplaySteps };
                 enhancedUI.SendMessage("UpdateProgressBar", progressParams, SendMessageOptions.DontRequireReceiver);
                 // <<< --- REMOVED: Debug Log that used verboseDiagnostics --- >>>
                 // if(verboseDiagnostics) Debug.Log($"AdvancedToNextStep: Sent messages to Enhanced UI for step {displayStepNum}/{totalDisplaySteps}");
            }
        }
    }

    /// <summary>
    /// Ends the therapy session immediately.
    /// Can be called by VoiceCommandManager or other triggers.
    /// </summary>
    public void EndSession() {
        Debug.Log("EndSession called.", this);
        voiceCommandManager?.StopListening();
        CompleteSession();
    }

    /// <summary>
    /// Handles the transition to the 'Complete' state. Displays completion message.
    /// </summary>
    private void CompleteSession()
    {
        if (currentState == SessionState.Complete) { Debug.LogWarning("CompleteSession called but state is already Complete.", this); return; }

        Debug.Log("CompleteSession executing...", this);
        currentState = SessionState.Complete;
        currentStepIndex = -1;

        voiceCommandManager?.StopListening();
        ActivateCurrentStepAndDeactivateOthers(-1);

        string finalCompletionMessage = string.IsNullOrEmpty(completionMessageText) ? "Session completed." : completionMessageText;
        if(instructionText != null) { instructionText.text = finalCompletionMessage; }
        else { Debug.LogWarning("CompleteSession: instructionText reference is missing!", this); }
        TTSManager.Instance?.Speak(finalCompletionMessage);

        if (enhancedUI != null) { enhancedUI.SendMessage("ShowWelcomeState", null, SendMessageOptions.DontRequireReceiver); }
        // Consider stopping Keyword Recognizer here
    }

    /// <summary>
    /// Displays the instructions and activates the behavior for the current therapy step.
    /// Also speaks the instructions and prompts the user for keyword activation.
    /// </summary>
    private void DisplayCurrentStep()
    {
        if (therapySteps == null || currentStepIndex < 0 || currentStepIndex >= therapySteps.Length || therapySteps[currentStepIndex] == null) {
             Debug.LogError($"DisplayCurrentStep: Invalid index ({currentStepIndex}) or therapy step data is null. Cannot display.", this);
             CompleteSession(); return;
        }

        TherapyStep currentStepData = therapySteps[currentStepIndex];
        string stepInstructions = currentStepData.instructions ?? "No instructions provided.";
        Debug.Log($"<color=cyan>SessionController: Displaying Therapy Step {currentStepIndex}. Instructions: '{stepInstructions}'</color>", this);

        if (instructionText != null) { instructionText.text = stepInstructions; }
        else { Debug.LogWarning("SessionController: instructionText reference is missing!", this); }

        ActivateCurrentStepAndDeactivateOthers(currentStepIndex); // Activate visuals

        string behaviorName = null;
        StepBehavior behavior = null;
        bool hasBehavior = currentStepData.stepBehaviorComponent != null;

        if (hasBehavior) {
            behavior = currentStepData.stepBehaviorComponent.GetComponent<StepBehavior>();
            if (behavior != null) {
                 behaviorName = currentStepData.stepBehaviorComponent.GetType().Name.Replace("Visualizer", "").Replace("Environment", "").Replace("Display", "");
                 TTSManager.Instance?.Speak($"Starting {behaviorName}."); // Announce step
            } else { Debug.LogError($"SessionController: Step {currentStepIndex}: Assigned component '{currentStepData.stepBehaviorComponent.name}' does NOT provide StepBehavior!", this); }
        } else { Debug.Log($"<color=grey>SessionController: Therapy Step {currentStepIndex} has no Step Behavior Component assigned.</color>", this); }

        // Prepare instructions to be spoken, including the keyword prompt
        string instructionsToSpeak = stepInstructions;
        if (currentStepIndex < therapySteps.Length -1) {
             instructionsToSpeak += "";
        } else {
             instructionsToSpeak += "";
        }

        float instructionDelay = string.IsNullOrEmpty(behaviorName) ? 0.1f : 1.0f;
        StartCoroutine(SpeakAfterDelay(instructionsToSpeak, instructionDelay));

        // Execute behavior if it exists
        if (behavior != null) {
            if(currentStepData.stepBehaviorComponent.gameObject.activeInHierarchy) {
                 Debug.Log($"<color=cyan>SessionController: Calling ExecuteStep() for Step {currentStepIndex} ({behaviorName})...</color>", this);
                 try { behavior.ExecuteStep(); } catch (System.Exception e) { Debug.LogError($"<color=red>SessionController: Error calling ExecuteStep for Step {currentStepIndex} ({behaviorName}): {e.Message}\n{e.StackTrace}</color>", this); }
            } else { Debug.LogWarning($"SessionController: Behavior component for step {currentStepIndex} ({currentStepData.stepBehaviorComponent.name}) is inactive. ExecuteStep skipped.", this); }
        }
        Debug.Log($"DisplayCurrentStep {currentStepIndex}: Finished setup. Waiting for keyword ('Okay') activation.", this);
    }

    /// <summary>
    /// Helper coroutine to speak text after a delay, checking if session is still active.
    /// </summary>
    private IEnumerator SpeakAfterDelay(string text, float delay) {
        if (string.IsNullOrEmpty(text)) yield break;
        if (delay > 0) { yield return new WaitForSeconds(delay); }
        if (currentState == SessionState.Active) { TTSManager.Instance?.Speak(text); }
    }

    /// <summary>
    /// Sets up the initial Idle state, positions UI, displays and speaks welcome message.
    /// </summary>
    private void ShowIdleInstructions()
    {
        // --- Position UI ---
        if (therapyEnvironmentRoot != null && Camera.main != null) {
             try {
                 Transform cameraTransform = Camera.main.transform;
                 Vector3 targetPosition = cameraTransform.position + (cameraTransform.forward * defaultDistance);
                 targetPosition.y = cameraTransform.position.y + defaultHeight;
                 therapyEnvironmentRoot.position = targetPosition;
                 Vector3 lookPos = cameraTransform.position;
                 lookPos.y = therapyEnvironmentRoot.position.y;
                 therapyEnvironmentRoot.LookAt(lookPos);
                 therapyEnvironmentRoot.forward *= -1f;
                 // Removed verbose log from here for brevity, positioning confirmed working earlier
                 // if(verboseDiagnostics) Debug.Log($"Positioned Therapy Environment at {targetPosition} relative to camera.", this);
             } catch (System.Exception e) { Debug.LogError($"Error positioning UI: {e.Message}", this); }
        } else {
            if(therapyEnvironmentRoot == null) Debug.LogWarning("Therapy Environment Root not assigned! Cannot position UI.", this);
            if(Camera.main == null) Debug.LogWarning("Camera.main is null! Cannot position UI. Ensure camera has 'MainCamera' tag.", this);
        }
        // --- End Position UI ---

        currentState = SessionState.Idle;
        currentStepIndex = -1;

        string initialWelcomeMessage = string.IsNullOrEmpty(welcomeMessageText) ? "Welcome." : welcomeMessageText;
        if (instructionText != null) {
            instructionText.text = initialWelcomeMessage;
            TTSManager.Instance?.Speak(initialWelcomeMessage);
        } else { Debug.LogWarning("SessionController: instructionText reference is missing!", this); }

        if (enhancedUI != null) { enhancedUI.SendMessage("ShowWelcomeState", null, SendMessageOptions.DontRequireReceiver); }

        Debug.Log("<color=orange>ShowIdleInstructions: Deactivating all therapy step behaviors for Idle state.</color>", this);
        ActivateCurrentStepAndDeactivateOthers(-1);

        // NOTE: VoiceCommandManager Start() activates listener initially. Keyword recognizer should also be active.
    }


    /// <summary>
    /// Activates the GameObject associated with the therapy step at 'activeIndex'
    /// and deactivates all others in the therapySteps array.
    /// </summary>
    private void ActivateCurrentStepAndDeactivateOthers(int activeIndex) {
        if (therapySteps == null) { Debug.LogWarning("ActivateCurrentStepAndDeactivateOthers: therapySteps array is null.", this); return; }

        for (int i = 0; i < therapySteps.Length; i++) {
             if (therapySteps[i] == null) continue;
            if (therapySteps[i].stepBehaviorComponent != null) {
                GameObject targetObject = therapySteps[i].stepBehaviorComponent.gameObject;
                if (targetObject == null) { Debug.LogError($"ActivateCurrentStepAndDeactivateOthers: GameObject is null for step {i}!", this); continue; }
                StepBehavior behavior = null; // Get only if needed

                if (i == activeIndex) { // Activate
                    if (!targetObject.activeSelf) {
                        // Removed verbose log for brevity
                        // if(verboseDiagnostics) Debug.Log($"<color=lime>ActivateCurrentStep: Activating Therapy Step {i} ({targetObject.name})</color>", this);
                        targetObject.SetActive(true);
                    }
                } else { // Deactivate
                    if (targetObject.activeSelf) {
                        // if(verboseDiagnostics) Debug.Log($"<color=orange>ActivateCurrentStep: Stopping/Deactivating Therapy Step {i} ({targetObject.name})</color>", this);
                        behavior = targetObject.GetComponent<StepBehavior>();
                        if (behavior != null) {
                             try { behavior.StopStep(); } catch (System.Exception e) { Debug.LogError($"<color=red>SessionController: Error calling StopStep for Step {i} ({targetObject.name}): {e.Message}</color>", this); }
                        } else { Debug.LogWarning($"<color=orange>ActivateCurrentStep: Could not get StepBehavior for Step {i} ({targetObject.name}).</color>", this); }
                        // Ensure deactivation
                        if(targetObject.activeSelf) {
                            // if(verboseDiagnostics) Debug.LogWarning($"<color=orange>ActivateCurrentStep: Forcing SetActive(false) for Step {i} ({targetObject.name}).</color>", this);
                            targetObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }

    // Interface and Class definitions should be in a separate file (e.g., TherapyStep.cs)
    /*
    public interface StepBehavior { void ExecuteStep(); void StopStep(); }
    [System.Serializable] public class TherapyStep {
        public string instructions;
        public MonoBehaviour stepBehaviorComponent;
    }
    */
}
