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
    
    private SessionState currentState = SessionState.Idle;
    private int currentStepIndex = -1;
    private Coroutine timeoutCoroutine;
    
    private void Start()
    {
        ShowIdleInstructions();
    }
    
    public SessionState GetCurrentState()
    {
        return currentState;
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
    
    private void DisplayCurrentStep()
    {
        if (currentStepIndex >= 0 && currentStepIndex < therapySteps.Length)
        {
            TherapyStep step = therapySteps[currentStepIndex];
            instructionText.text = step.instructions;
            
            // If there's specific step behavior, trigger it
            if (step.stepBehavior != null)
            {
                step.stepBehavior.ExecuteStep();
            }
        }
    }
    
    private void ShowIdleInstructions()
    {
        currentState = SessionState.Idle;
        instructionText.text = "Welcome to Voice-Driven Therapy.\nSay \"Start therapy\" or \"Begin session\" to start.";
        
        // Update enhanced UI if available
        if (enhancedUI != null)
        {
            enhancedUI.SendMessage("ShowWelcomeState", null, SendMessageOptions.DontRequireReceiver);
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