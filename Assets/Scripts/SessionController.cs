using System.Collections;
using UnityEngine;
using TMPro;
// Remove the UI.Enhanced namespace import since it's not recognized

public enum SessionState
{
    Idle,
    Active,
    Complete
}

public class SessionController : MonoBehaviour
{
    [Header("Session Configuration")]
    [SerializeField] private float commandTimeoutDuration = 30f;
    [SerializeField] private TherapyStep[] therapySteps;
    
    [Header("References")]
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private VoiceCommandManager voiceCommandManager;
    [SerializeField] private FeedbackManager feedbackManager;
    [SerializeField] private MonoBehaviour enhancedUI; // Change to MonoBehaviour to avoid namespace issues

    [Header("UI Positioning")]
    [SerializeField] private Transform therapyEnvironmentRoot; // Drag your 'Therapy Environment' GameObject here
    [SerializeField] private float defaultDistance = 2.0f; // How far in front to place it
    [SerializeField] private float defaultHeight = 0.0f; // Vertical offset from camera height
    
    private SessionState currentState = SessionState.Idle;
    private int currentStepIndex = -1;
    private Coroutine timeoutCoroutine;
    
    // --- Replace the logging loop inside SessionController.Start() ---
    private void Start()
    {
        Debug.Log("--- SessionController: Checking Initial Step Behaviors ---");
        if (therapySteps != null) {
            for (int i = 0; i < therapySteps.Length; i++) {
                if (therapySteps[i] != null) {
                    // Log the assigned MonoBehaviour component
                    string behaviorName = therapySteps[i].stepBehaviorComponent != null ? $"{therapySteps[i].stepBehaviorComponent.GetType().Name} on {therapySteps[i].stepBehaviorComponent.gameObject.name}" : "None";
                    // Also check if it actually implements the interface
                    bool implementsInterface = therapySteps[i].stepBehaviorComponent is StepBehavior; // Check if component implements interface
                    Debug.Log($"Step {i}: Behavior Component = {behaviorName} (Implements StepBehavior? {implementsInterface})");
                    if (therapySteps[i].stepBehaviorComponent != null && !implementsInterface) {
                        // Add Error log if the assigned component is wrong type
                        Debug.LogError($"<color=red>Step {i}: Assigned component '{behaviorName}' does NOT implement the StepBehavior interface!</color>");
                    }
                } else {
                    Debug.LogWarning($"Step {i}: TherapyStep array element is null!");
                }
            }
        } else {
            Debug.LogWarning("TherapySteps array is null!");
        }
        Debug.Log("-------------------------------------------------------");

        ShowIdleInstructions(); // Keep original Start() code
    }
    
    public SessionState GetCurrentState()
    {
        return currentState;
    }
    public int GetCurrentStepIndex()
    {
        return currentStepIndex; // Simply return the value of the private variable
    }
    
    public void StartSession()
    {
        if (currentState == SessionState.Active)
        {
            feedbackManager.PlayErrorFeedback("Session already in progress");
            return;
        }
        
        currentState = SessionState.Active;
        currentStepIndex = -1;
        AdvanceToNextStep();
    }
    
    public void AdvanceToNextStep()
    {
        if (currentState != SessionState.Active)
        {
            feedbackManager.PlayErrorFeedback("No active session");
            return;
        }
        
        // Cancel existing timeout if there is one
        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
        }
        
        currentStepIndex++;
        
        // Check if we've completed all steps
        if (currentStepIndex >= therapySteps.Length)
        {
            CompleteSession();
            return;
        }
        
        // Display the current step
        DisplayCurrentStep();
        
        // Start timeout for this step
        timeoutCoroutine = StartCoroutine(CommandTimeoutRoutine());
        
        // Update enhanced UI if available
        if (enhancedUI != null)
        {
            // Use SendMessage to avoid direct type references
            enhancedUI.SendMessage("ShowActiveState", therapySteps[currentStepIndex].instructions, SendMessageOptions.DontRequireReceiver);
            
            // For progress update, create a message with both parameters
            object[] progressParams = new object[] { currentStepIndex + 1, therapySteps.Length };
            enhancedUI.SendMessage("UpdateProgressBar", progressParams, SendMessageOptions.DontRequireReceiver);
        }
    }
    
    public void EndSession()
    {
        CompleteSession();
    }
    
    private void CompleteSession()
    {
        currentState = SessionState.Complete;
        
        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null;
        }
        
        instructionText.text = "Session complete. Thank you for participating.\nSay \"Start therapy\" to begin again.";
        feedbackManager.PlaySuccessFeedback("Session completed successfully");
        
        // Update enhanced UI if available
        if (enhancedUI != null)
        {
            enhancedUI.SendMessage("ShowWelcomeState", null, SendMessageOptions.DontRequireReceiver);
        }
    }
    
    // --- CORRECTED SessionController.DisplayCurrentStep ---
    // Replace the entire method in SessionController.cs with this corrected version:
    private void DisplayCurrentStep()
    {
        // Check for invalid index first
        if (currentStepIndex < 0 || currentStepIndex >= therapySteps.Length)
        {
            Debug.LogWarning($"SessionController: DisplayCurrentStep called with invalid index: {currentStepIndex}. Deactivating all steps.");
            // Use the CORRECT helper method name here:
            ActivateCurrentStepAndDeactivateOthers(-1); // Deactivate all if index is invalid
            if (instructionText != null) instructionText.text = "Session ended or invalid state."; // Show message
            return; // Stop further processing
        }

        // Index is valid, proceed
        TherapyStep currentStepData = therapySteps[currentStepIndex];
        Debug.Log($"<color=cyan>SessionController: Displaying Step {currentStepIndex}. Instructions: '{currentStepData.instructions}'</color>");

        // Update Instructions UI
        if (instructionText != null) {
            instructionText.text = currentStepData.instructions;
        } else {
            Debug.LogWarning("SessionController: instructionText reference is missing!");
        }

        // Activate the current step's behavior AND deactivate all others
        // Use the CORRECT helper method name here:
        ActivateCurrentStepAndDeactivateOthers(currentStepIndex);

        // Now execute the current step's behavior (it should have been activated by the helper method if needed)
        if (currentStepData.stepBehaviorComponent != null)
        {
            StepBehavior behavior = currentStepData.stepBehaviorComponent as StepBehavior;
            if (behavior != null)
            {
                // Only call ExecuteStep if the component's GameObject is active in the hierarchy
                if(currentStepData.stepBehaviorComponent.gameObject.activeInHierarchy) {
                    string behaviorName = $"{currentStepData.stepBehaviorComponent.GetType().Name} on {currentStepData.stepBehaviorComponent.gameObject.name}";
                    Debug.Log($"<color=cyan>SessionController: Calling ExecuteStep() for Step {currentStepIndex} ({behaviorName})...</color>");
                    try {
                        behavior.ExecuteStep();
                    } catch (System.Exception e) {
                        Debug.LogError($"<color=red>SessionController: Error calling ExecuteStep for Step {currentStepIndex} ({behaviorName}): {e.Message}\n{e.StackTrace}</color>");
                    }
                } else {
                    // This case should ideally not happen if ActivateCurrentStepAndDeactivateOthers worked correctly
                    Debug.LogWarning($"SessionController: Behavior component for step {currentStepIndex} is assigned but its GameObject is not active in hierarchy after activation attempt. ExecuteStep skipped.");
                }
            }
            else {
                Debug.LogError($"<color=red>SessionController: Step {currentStepIndex}: Assigned component '{currentStepData.stepBehaviorComponent.GetType().Name}' does NOT implement StepBehavior!</color>");
            }
        } else {
            Debug.Log($"<color=grey>SessionController: Step {currentStepIndex} has no Step Behavior Component assigned.</color>");
        }
    }

    // --- Ensure this ENTIRE method is included inside your SessionController class: ---
    private void ShowIdleInstructions()
    {
        if (therapyEnvironmentRoot != null && Camera.main != null)
        {
            Transform cameraTransform = Camera.main.transform;
            // Calculate position in front of camera
            Vector3 targetPosition = cameraTransform.position + (cameraTransform.forward * defaultDistance);
            // Adjust height relative to camera
            targetPosition.y = cameraTransform.position.y + defaultHeight;

            // Set the root object's position
            therapyEnvironmentRoot.position = targetPosition;

            // Make the root object face the camera (optional, good for consistent orientation)
            // Get rotation that looks at camera, but only rotate around Y axis
            Vector3 lookPos = cameraTransform.position;
            lookPos.y = therapyEnvironmentRoot.position.y; // Keep UI level
            therapyEnvironmentRoot.LookAt(lookPos);
            therapyEnvironmentRoot.forward *= -1f; // Point towards camera correctly

            Debug.Log($"Positioned Therapy Environment at {targetPosition} relative to camera.");
        }
        else {
            if(therapyEnvironmentRoot == null) Debug.LogWarning("Therapy Environment Root not assigned in SessionController!");
            if(Camera.main == null) Debug.LogWarning("Camera.main is null! Cannot position UI relative to camera.");
        }

        currentState = SessionState.Idle;
        currentStepIndex = -1; // Optional: Ensure index is reset for idle state

        if (instructionText != null) { // Added null check
            instructionText.text = "Welcome to Voice-Driven Therapy.\nSay \"Start therapy\" or \"Begin session\" to start.";
        } else {
            Debug.LogWarning("SessionController: instructionText reference is missing!");
        }

        // Update enhanced UI if available
        if (enhancedUI != null)
        {
            enhancedUI.SendMessage("ShowWelcomeState", null, SendMessageOptions.DontRequireReceiver);
        }

        // --- ADD THESE LINES ---
        // Deactivate all step behaviors when entering Idle state
        Debug.Log("<color=orange>ShowIdleInstructions: Deactivating all step behaviors for Idle state.</color>");
        ActivateCurrentStepAndDeactivateOthers(-1); // Pass -1 to deactivate all steps
        // --- END OF ADDED LINES ---
    }

    // --- Add this ENTIRE method inside your SessionController class ---
    // Make sure it's inside the class braces { }, for example, after the
    // ShowIdleInstructions() method or before the final closing brace } of the class.

    private void ActivateCurrentStepAndDeactivateOthers(int activeIndex)
    {
        if (therapySteps == null) return;

        for (int i = 0; i < therapySteps.Length; i++)
        {
            if (therapySteps[i]?.stepBehaviorComponent != null)
            {
                StepBehavior behavior = therapySteps[i].stepBehaviorComponent as StepBehavior;
                if (behavior == null) continue; // Skip if component doesn't implement interface

                GameObject targetObject = therapySteps[i].stepBehaviorComponent.gameObject;

                if (i == activeIndex)
                {
                    // Activate the CURRENT step's GameObject if it's not already active
                    if (!targetObject.activeSelf) // Check activeSelf, not activeInHierarchy, for direct activation call
                    {
                        Debug.Log($"<color=lime>ActivateCurrentStep: Activating Step {i} ({targetObject.name})</color>");
                        targetObject.SetActive(true);
                        // Note: ExecuteStep is called separately in DisplayCurrentStep after this function runs
                    }
                }
                else
                {
                    // Deactivate all OTHER steps
                    if (targetObject.activeSelf) // Only stop/deactivate if currently active
                    {
                        Debug.Log($"<color=orange>ActivateCurrentStep: Stopping/Deactivating Step {i} ({targetObject.name})</color>");
                        try {
                            behavior.StopStep(); // Call the interface method (which should now handle SetActive(false))
                        } catch (System.Exception e) {
                             Debug.LogError($"<color=red>SessionController: Error calling StopStep for Step {i} ({targetObject.name}): {e.Message}\n{e.StackTrace}</color>");
                             // Fallback: Force deactivate if StopStep failed but object still active
                             if(targetObject.activeSelf) targetObject.SetActive(false);
                        }
                        // Ensure inactive even if StopStep forgot or failed without error
                         if(targetObject.activeSelf) targetObject.SetActive(false);
                    }
                }
            }
        }
    }
    
    private IEnumerator CommandTimeoutRoutine()
    {
        yield return new WaitForSeconds(commandTimeoutDuration);
        
        // When timeout occurs, provide feedback
        feedbackManager.PlayTimeoutFeedback("I haven't heard a command in a while. Say \"Continue\" to move forward or \"End session\" to stop.");
        
        // Update enhanced UI if available
        if (enhancedUI != null)
        {
            enhancedUI.SendMessage("UpdateStatusText", "Haven't heard a command. Say \"Continue\" or use manual controls.", SendMessageOptions.DontRequireReceiver);
        }
        
        // Wait for additional time before auto-advancing
        yield return new WaitForSeconds(commandTimeoutDuration);
        
        // Auto-advance if still no response
        feedbackManager.ShowMessage("Auto-advancing to next step due to inactivity");
        AdvanceToNextStep();
    }
}