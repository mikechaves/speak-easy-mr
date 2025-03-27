using UnityEngine;
using System.Collections;
using System;

public class SimpleSessionController : MonoBehaviour
{
    [Header("Session Configuration")]
    [SerializeField] private string sessionName = "Voice Therapy Session";
    
    [Serializable]
    public class TherapyStep
    {
        public string stepName;
        public string instruction;
        [TextArea(3, 10)]
        public string detailedInstructions;
        public float minimumStepDuration = 5f;
    }
    
    [SerializeField] private TherapyStep[] therapySteps;
    
    [Header("Dependencies")]
    [SerializeField] private SimpleVoiceCommandManager voiceManager;
    
    // Session state
    private bool isSessionActive = false;
    private int currentStepIndex = -1;
    private float currentStepStartTime;
    
    void Start()
    {
        if (voiceManager == null)
        {
            voiceManager = FindObjectOfType<SimpleVoiceCommandManager>();
            if (voiceManager == null)
            {
                Debug.LogError("No SimpleVoiceCommandManager found. Voice commands will not work.");
                return;
            }
        }
        
        // Register for voice command events
        voiceManager.OnStartTherapyCommand.AddListener(StartSession);
        voiceManager.OnNextStepCommand.AddListener(AdvanceToNextStep);
        voiceManager.OnEndSessionCommand.AddListener(EndSession);
        
        Debug.Log("SimpleSessionController initialized with " + therapySteps.Length + " steps");
    }
    
    public void StartSession()
    {
        isSessionActive = true;
        currentStepIndex = -1;
        Debug.Log("Session started!");
        AdvanceToNextStep();
    }
    
    public void AdvanceToNextStep()
    {
        if (!isSessionActive) return;
        
        currentStepIndex++;
        
        if (currentStepIndex >= therapySteps.Length)
        {
            EndSession();
            return;
        }
        
        currentStepStartTime = Time.time;
        TherapyStep currentStep = therapySteps[currentStepIndex];
        Debug.Log("Step " + (currentStepIndex + 1) + ": " + currentStep.stepName);
        Debug.Log("Instruction: " + currentStep.instruction);
    }
    
    public void EndSession()
    {
        isSessionActive = false;
        Debug.Log("Session ended!");
    }
    
    // For the inspector to add some default therapy steps if none are provided
    void Reset()
    {
        if (therapySteps == null || therapySteps.Length == 0)
        {
            therapySteps = new TherapyStep[4];
            
            therapySteps[0] = new TherapyStep
            {
                stepName = "Introduction",
                instruction = "Welcome to your therapy session. Take a deep breath.",
                detailedInstructions = "Find a comfortable position. We'll begin with some gentle breathing to help you relax.",
                minimumStepDuration = 10f
            };
            
            therapySteps[1] = new TherapyStep
            {
                stepName = "Deep Breathing",
                instruction = "Breathe in slowly for 4 counts, hold for 2, then out for 6.",
                detailedInstructions = "Focus on your breath. Let your chest and belly expand as you inhale. Feel tension leave as you exhale.",
                minimumStepDuration = 30f
            };
            
            therapySteps[2] = new TherapyStep
            {
                stepName = "Calm Visualization",
                instruction = "Imagine a peaceful place that makes you feel safe and relaxed.",
                detailedInstructions = "It could be a beach, forest, or anywhere you feel at peace. Notice the details around you in this place.",
                minimumStepDuration = 45f
            };
            
            therapySteps[3] = new TherapyStep
            {
                stepName = "Completion",
                instruction = "Gently bring your awareness back to the room.",
                detailedInstructions = "Take one final deep breath. When you're ready, say 'End Session' to complete your therapy.",
                minimumStepDuration = 15f
            };
        }
    }
}