using UnityEngine;
using System;
using SpeakEasy.Therapy;

public class TherapyStepsPreset : MonoBehaviour
{
    [SerializeField] private SessionController sessionController;
    
    [Serializable]
    public class TherapyStepData
    {
        public string stepName;
        public string instruction;
        public string detailedInstructions;
        public float minimumStepDuration = 5f;
    }
    
    [SerializeField] private TherapyStepData[] presetSteps = new TherapyStepData[]
    {
        new TherapyStepData()
        {
            stepName = "Introduction",
            instruction = "Welcome to your therapy session. Take a deep breath.",
            detailedInstructions = "Find a comfortable position. We'll begin with some gentle breathing to help you relax.",
            minimumStepDuration = 10f
        },
        new TherapyStepData()
        {
            stepName = "Deep Breathing",
            instruction = "Breathe in slowly for 4 counts, hold for 2, then out for 6.",
            detailedInstructions = "Focus on your breath. Let your chest and belly expand as you inhale. Feel tension leave as you exhale.",
            minimumStepDuration = 30f
        },
        new TherapyStepData()
        {
            stepName = "Calm Visualization",
            instruction = "Imagine a peaceful place that makes you feel safe and relaxed.",
            detailedInstructions = "It could be a beach, forest, or anywhere you feel at peace. Notice the details around you in this place.",
            minimumStepDuration = 45f
        },
        new TherapyStepData()
        {
            stepName = "Completion",
            instruction = "Gently bring your awareness back to the room.",
            detailedInstructions = "Take one final deep breath. When you're ready, say 'End Session' to complete your therapy.",
            minimumStepDuration = 15f
        }
    };
    
    void Start()
    {
        if (sessionController == null)
        {
            sessionController = GetComponent<SessionController>();
        }
        
        Debug.Log("Therapy Steps Preset loaded with " + presetSteps.Length + " steps");
    }
}