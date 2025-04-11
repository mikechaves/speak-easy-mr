using System.Collections;
using UnityEngine;
using TMPro;

public enum SessionState
{
    Idle,
    Active,
    Complete
}

public class SessionController : MonoBehaviour
{
    [Header("Session Messages")]
    [Tooltip("The welcome message displayed and spoken when the session is idle.")]
    [TextArea(3, 5)] // Allow multi-line editing in Inspector
    [SerializeField] private string welcomeMessageText = "Welcome to Voice-Driven Therapy.\nSay \"Start therapy\" or \"Begin session\" to start.";
    [Tooltip("The completion message displayed and spoken when the session ends.")]
    [TextArea(3, 5)]
    [SerializeField] private string completionMessageText = "Session complete. Thank you for participating.";

    [Header("Therapy Steps Configuration")]
    [Tooltip("The array containing the actual therapy steps (e.g., Breathing, Visualization). Should NOT include Welcome or Completion steps.")]
    [SerializeField] private TherapyStep[] therapySteps; // Should contain the 5 active steps
    [SerializeField] private float commandTimeoutDuration = 30f;


    [Header("References")]
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private VoiceCommandManager voiceCommandManager;
    [SerializeField] private FeedbackManager feedbackManager;
    [SerializeField] private MonoBehaviour enhancedUI;

    [Header("UI Positioning")]
    [SerializeField] private Transform therapyEnvironmentRoot;
    [SerializeField] private float defaultDistance = 2.0f;
    [SerializeField] private float defaultHeight = 0.0f;

    private SessionState currentState = SessionState.Idle;
    // Index for the therapySteps array (0 to Length-1 for the active steps)
    private int currentStepIndex = -1;
    private Coroutine timeoutCoroutine;

    private void Start()
    {
        Debug.Log("--- SessionController: Checking Initial Step Behaviors ---");
        // Check the actual therapy steps array
        if (therapySteps != null) {
            for (int i = 0; i < therapySteps.Length; i++) { // Loop through the 5 active steps
                if (therapySteps[i] != null && therapySteps[i].stepBehaviorComponent != null) {
                    string behaviorName = $"{therapySteps[i].stepBehaviorComponent.GetType().Name} on {therapySteps[i].stepBehaviorComponent.gameObject.name}";
                    StepBehavior behavior = therapySteps[i].stepBehaviorComponent.GetComponent<StepBehavior>();
                    bool implementsInterface = (behavior != null);
                    Debug.Log($"Therapy Step {i}: Behavior Component = {behaviorName} (GetComponent<StepBehavior> found? {implementsInterface})");
                    if (!implementsInterface) {
                        Debug.LogError($"<color=red>Therapy Step {i}: Assigned component '{behaviorName}' does NOT provide StepBehavior via GetComponent!</color>");
                    }
                } else if (therapySteps[i] == null) {
                    Debug.LogWarning($"Therapy Step {i}: Array element is null!");
                } else {
                     Debug.LogWarning($"Therapy Step {i}: Has no stepBehaviorComponent assigned!");
                }
            }
        } else {
            Debug.LogWarning("TherapySteps array is null or empty!");
        }
        Debug.Log("-------------------------------------------------------");

        ShowIdleInstructions(); // Show the separate welcome message
    }

    public SessionState GetCurrentState()
    {
        return currentState;
    }
    public int GetCurrentStepIndex()
    {
        // Return index relative to the active therapy steps (0-4)
        return currentStepIndex;
    }

    public void StartSession()
    {
        if (currentState == SessionState.Active)
        {
            feedbackManager.PlayErrorFeedback("Session already in progress");
            return;
        }
        // Check if there are actual therapy steps defined
        if (therapySteps == null || therapySteps.Length == 0)
        {
             Debug.LogError("StartSession: No therapy steps defined in the array!");
             feedbackManager.PlayErrorFeedback("Session configuration error.");
             return;
        }

        currentState = SessionState.Active;
        // Start at -1 so the first AdvanceToNextStep goes to index 0 (first actual therapy step)
        currentStepIndex = -1;
        Debug.Log($"StartSession: Set currentStepIndex to {currentStepIndex}. Calling AdvanceToNextStep to go to the first therapy step (index 0).");
        AdvanceToNextStep();
    }


    public void AdvanceToNextStep()
    {
        if (currentState != SessionState.Active)
        {
            if (currentState != SessionState.Complete) {
                 feedbackManager.PlayErrorFeedback("No active session");
            }
            return;
        }

        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null;
        }

        currentStepIndex++; // Increment index for the therapySteps array (0-4)
        Debug.Log($"AdvanceToNextStep: Incremented index to {currentStepIndex}");

        // Check if we have completed all steps in the therapySteps array
        if (currentStepIndex >= therapySteps.Length)
        {
            Debug.Log($"AdvanceToNextStep: Completed last therapy step (index {currentStepIndex - 1}). Completing session.");
            CompleteSession(); // Call completion which uses the separate message
            return;
        }

        // Check if the target step index is valid within the array
         if (therapySteps == null || currentStepIndex < 0 || currentStepIndex >= therapySteps.Length || therapySteps[currentStepIndex] == null)
         {
             Debug.LogError($"AdvanceToNextStep: Invalid step index or data ({currentStepIndex}) after increment. Completing session.");
             CompleteSession();
             return;
         }

        DisplayCurrentStep(); // Display the therapy step (index 0-4)

        // Start timeout only for steps with behavior components
        if (therapySteps[currentStepIndex].stepBehaviorComponent != null) {
             Debug.Log($"AdvanceToNextStep: Starting timeout for therapy step {currentStepIndex}");
             timeoutCoroutine = StartCoroutine(CommandTimeoutRoutine());
        } else {
             Debug.Log($"AdvanceToNextStep: Therapy Step {currentStepIndex} has no behavior, not starting timeout.");
        }

        // Update Enhanced UI (adjust progress based on 5 steps)
        if (enhancedUI != null)
        {
            if (currentStepIndex >= 0 && currentStepIndex < therapySteps.Length && therapySteps[currentStepIndex] != null) {
                 int displayStepNum = currentStepIndex + 1; // Show 1-5 for progress
                 int totalDisplaySteps = therapySteps.Length; // Total is 5
                 enhancedUI.SendMessage("ShowActiveState", therapySteps[currentStepIndex].instructions, SendMessageOptions.DontRequireReceiver);
                 object[] progressParams = new object[] { displayStepNum, totalDisplaySteps };
                 enhancedUI.SendMessage("UpdateProgressBar", progressParams, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public void EndSession()
    {
         Debug.Log("EndSession called.");
        CompleteSession();
    }

    private void CompleteSession()
    {
        if (currentState == SessionState.Complete) return;

        Debug.Log("CompleteSession executing...");
        currentState = SessionState.Complete;
        currentStepIndex = -1; // Reset index

        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null;
        }

        ActivateCurrentStepAndDeactivateOthers(-1); // Deactivate any active therapy step

        // *** Use the separate completion message text field ***
        string finalCompletionMessage = string.IsNullOrEmpty(completionMessageText) ? "Session completed." : completionMessageText;
        // *****************************************************

        // Update UI Text
        if(instructionText != null) {
            instructionText.text = finalCompletionMessage;
        } else {
             Debug.LogWarning("CompleteSession: instructionText reference is missing!");
        }

        // *** Speak the separate completion message ***
        TTSManager.Instance.Speak(finalCompletionMessage);
        // *********************************************

        if (enhancedUI != null)
        {
            enhancedUI.SendMessage("ShowWelcomeState", null, SendMessageOptions.DontRequireReceiver);
        }
    }

    // Displays the active therapy step (index 0-4)
    private void DisplayCurrentStep()
    {
        // Validity check for therapySteps array index
        if (therapySteps == null || currentStepIndex < 0 || currentStepIndex >= therapySteps.Length || therapySteps[currentStepIndex] == null)
        {
            Debug.LogWarning($"SessionController: DisplayCurrentStep called with invalid therapy step index or null data: {currentStepIndex}.");
            // Optionally complete session or handle error
            CompleteSession();
            return;
        }

        TherapyStep currentStepData = therapySteps[currentStepIndex];
        string stepInstructions = currentStepData.instructions; // Get instructions from Inspector
        Debug.Log($"<color=cyan>SessionController: Displaying Therapy Step {currentStepIndex}. Instructions: '{stepInstructions}'</color>");

        // Update UI Text
        if (instructionText != null) {
            instructionText.text = stepInstructions;
        } else {
            Debug.LogWarning("SessionController: instructionText reference is missing!");
        }

        // Activate visuals/behavior for this therapy step *BEFORE* speaking
        ActivateCurrentStepAndDeactivateOthers(currentStepIndex);

        // Announce Behavior Name (if applicable) and Speak Instructions
        string behaviorName = null;
        StepBehavior behavior = null;

        if (currentStepData.stepBehaviorComponent != null)
        {
            behavior = currentStepData.stepBehaviorComponent.GetComponent<StepBehavior>();
            if (behavior != null) {
                 behaviorName = currentStepData.stepBehaviorComponent.GetType().Name;
                 behaviorName = behaviorName.Replace("Visualizer", "").Replace("Environment", "").Replace("Display", ""); // Simplify name
                 TTSManager.Instance.Speak($"Starting {behaviorName}."); // Announce step type
            } else {
                 Debug.LogError($"<color=red>SessionController: Therapy Step {currentStepIndex}: Assigned component '{currentStepData.stepBehaviorComponent.GetType().Name}' does NOT provide StepBehavior via GetComponent!</color>");
            }
        } else {
            // This therapy step might be informational only
            Debug.Log($"<color=grey>SessionController: Therapy Step {currentStepIndex} has no Step Behavior Component assigned.</color>");
        }

        // Speak the step instructions from Inspector after potential announcement delay
        float instructionDelay = string.IsNullOrEmpty(behaviorName) ? 0f : 1.0f; // Adjust delay if needed
        StartCoroutine(SpeakAfterDelay(stepInstructions, instructionDelay));

        // Execute the step's behavior *if it exists and was found*
        if (behavior != null)
        {
            if(currentStepData.stepBehaviorComponent.gameObject.activeInHierarchy) {
                Debug.Log($"<color=cyan>SessionController: Calling ExecuteStep() for Step {currentStepIndex} ({behaviorName})...</color>");
                try {
                    behavior.ExecuteStep();
                } catch (System.Exception e) {
                    Debug.LogError($"<color=red>SessionController: Error calling ExecuteStep for Step {currentStepIndex} ({behaviorName}): {e.Message}\n{e.StackTrace}</color>");
                }
            } else {
                Debug.LogWarning($"SessionController: Behavior component for step {currentStepIndex} ({currentStepData.stepBehaviorComponent.name}) is assigned but its GameObject is not active in hierarchy. ExecuteStep skipped.");
            }
        }
    }

    // Helper coroutine to speak instructions after a delay
    private IEnumerator SpeakAfterDelay(string text, float delay) {
        if (string.IsNullOrEmpty(text)) yield break; // Don't speak empty text
        if (delay > 0) {
            yield return new WaitForSeconds(delay);
        }
        TTSManager.Instance.Speak(text);
    }

    // Shows the initial idle state with the separate welcome message
    private void ShowIdleInstructions()
    {
        // Position UI
        if (therapyEnvironmentRoot != null && Camera.main != null)
        {
            Transform cameraTransform = Camera.main.transform;
            Vector3 targetPosition = cameraTransform.position + (cameraTransform.forward * defaultDistance);
            targetPosition.y = cameraTransform.position.y + defaultHeight;
            therapyEnvironmentRoot.position = targetPosition;
            Vector3 lookPos = cameraTransform.position;
            lookPos.y = therapyEnvironmentRoot.position.y;
            therapyEnvironmentRoot.LookAt(lookPos);
            therapyEnvironmentRoot.forward *= -1f;
            Debug.Log($"Positioned Therapy Environment at {targetPosition} relative to camera.");
        }
        else {
            if(therapyEnvironmentRoot == null) Debug.LogWarning("Therapy Environment Root not assigned in SessionController!");
            if(Camera.main == null) Debug.LogWarning("Camera.main is null! Cannot position UI relative to camera.");
        }

        currentState = SessionState.Idle;
        currentStepIndex = -1; // Reset therapy step index

        // *** Use the separate welcome message text field ***
        string initialWelcomeMessage = string.IsNullOrEmpty(welcomeMessageText) ? "Welcome." : welcomeMessageText;
        // **************************************************

        // Update UI Text and Speak Welcome Message
        if (instructionText != null) {
            instructionText.text = initialWelcomeMessage;
            TTSManager.Instance.Speak(initialWelcomeMessage);
        } else {
            Debug.LogWarning("SessionController: instructionText reference is missing!");
        }

        if (enhancedUI != null) {
            enhancedUI.SendMessage("ShowWelcomeState", null, SendMessageOptions.DontRequireReceiver);
        }

        Debug.Log("<color=orange>ShowIdleInstructions: Deactivating all therapy step behaviors for Idle state.</color>");
        ActivateCurrentStepAndDeactivateOthers(-1); // Ensure all therapy step behaviors are off
    }

    // Activates the therapy step at activeIndex (0-4) and deactivates others
    private void ActivateCurrentStepAndDeactivateOthers(int activeIndex)
    {
        if (therapySteps == null) {
             Debug.LogWarning("ActivateCurrentStepAndDeactivateOthers: therapySteps array is null.");
             return;
        }

        // Loop through the therapySteps array (indices 0-4)
        for (int i = 0; i < therapySteps.Length; i++)
        {
             if (therapySteps[i] == null) continue;

            // Only process steps with behavior components
            if (therapySteps[i].stepBehaviorComponent != null)
            {
                GameObject targetObject = therapySteps[i].stepBehaviorComponent.gameObject;
                if (targetObject == null) {
                    Debug.LogError($"ActivateCurrentStepAndDeactivateOthers: GameObject is null for stepBehaviorComponent at therapy step index {i}!");
                    continue;
                }
                StepBehavior behavior = targetObject.GetComponent<StepBehavior>();

                if (i == activeIndex) // Activate
                {
                    if (!targetObject.activeSelf) {
                        Debug.Log($"<color=lime>ActivateCurrentStep: Activating Therapy Step {i} ({targetObject.name})</color>");
                        targetObject.SetActive(true);
                    }
                }
                else // Deactivate
                {
                    if (targetObject.activeSelf) {
                        Debug.Log($"<color=orange>ActivateCurrentStep: Stopping/Deactivating Therapy Step {i} ({targetObject.name})</color>");
                        if (behavior != null) {
                             try { behavior.StopStep(); } catch (System.Exception e) {
                                Debug.LogError($"<color=red>SessionController: Error calling StopStep for Therapy Step {i} ({targetObject.name}): {e.Message}</color>");
                                if(targetObject.activeSelf) targetObject.SetActive(false);
                            }
                        } else {
                             Debug.LogWarning($"<color=orange>ActivateCurrentStep: Could not get StepBehavior for Therapy Step {i} ({targetObject.name}). Forcing SetActive(false).</color>");
                             targetObject.SetActive(false);
                        }
                        if(targetObject.activeSelf) { // Final check
                            Debug.LogWarning($"<color=orange>ActivateCurrentStep: Forcing SetActive(false) for Therapy Step {i} ({targetObject.name}) after checks.</color>");
                            targetObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }

    private IEnumerator CommandTimeoutRoutine()
    {
         if (currentState != SessionState.Active) yield break;
        int stepIndexWhenStarted = currentStepIndex;
        Debug.Log($"CommandTimeoutRoutine: Starting wait for {commandTimeoutDuration}s for therapy step {stepIndexWhenStarted}");

        yield return new WaitForSeconds(commandTimeoutDuration);

        if (currentState != SessionState.Active || currentStepIndex != stepIndexWhenStarted || timeoutCoroutine == null) {
             Debug.Log($"CommandTimeoutRoutine: State/Step changed or coroutine stopped during wait for step {stepIndexWhenStarted}. Exiting timeout.");
             yield break;
        }

        Debug.Log($"CommandTimeoutRoutine: Timeout reached for therapy step {stepIndexWhenStarted}. Playing feedback.");
        string timeoutMessage = "I haven't heard a command in a while. Say \"Continue\" to move forward or \"End session\" to stop.";
        feedbackManager.PlayTimeoutFeedback(timeoutMessage); // Speaks via TTSManager

        if (enhancedUI != null) {
            enhancedUI.SendMessage("UpdateStatusText", "Haven't heard a command. Say \"Continue\" or use manual controls.", SendMessageOptions.DontRequireReceiver);
        }
        timeoutCoroutine = null;
    }

    // Ensure interface/class definitions are handled correctly (likely in TherapyStep.cs)
    /*
    public interface StepBehavior { ... }
    [System.Serializable] public class TherapyStep { ... }
    */
}
