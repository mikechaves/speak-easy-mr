using UnityEngine;

[System.Serializable]
public class TherapyStep
{
    public string instructions;
    public StepBehavior stepBehavior;
}

// Interface for custom step behaviors
public interface StepBehavior
{
    void ExecuteStep();
}

// Example implementation of a step behavior for breathing exercise
public class BreathingExerciseStep : MonoBehaviour, StepBehavior
{
    [SerializeField] private GameObject breathingVisual;
    [SerializeField] private AudioSource breathingAudio;
    
    public void ExecuteStep()
    {
        // Activate visual guide
        if (breathingVisual != null)
        {
            breathingVisual.SetActive(true);
        }
        
        // Start audio guidance if available
        if (breathingAudio != null)
        {
            breathingAudio.Play();
        }
    }
    
    public void EndStep()
    {
        // Hide visual guide
        if (breathingVisual != null)
        {
            breathingVisual.SetActive(false);
        }
        
        // Stop audio
        if (breathingAudio != null && breathingAudio.isPlaying)
        {
            breathingAudio.Stop();
        }
    }
}

// Example implementation for guided visualization
public class VisualizationStep : MonoBehaviour, StepBehavior
{
    [SerializeField] private GameObject visualizationEnvironment;
    [SerializeField] private AudioSource ambientAudio;
    
    public void ExecuteStep()
    {
        // Show the visualization environment
        if (visualizationEnvironment != null)
        {
            visualizationEnvironment.SetActive(true);
        }
        
        // Start ambient audio
        if (ambientAudio != null)
        {
            ambientAudio.Play();
        }
    }
    
    public void EndStep()
    {
        // Hide the visualization environment
        if (visualizationEnvironment != null)
        {
            visualizationEnvironment.SetActive(false);
        }
        
        // Fade out ambient audio
        if (ambientAudio != null && ambientAudio.isPlaying)
        {
            // Could implement a fade out here
            ambientAudio.Stop();
        }
    }
}

// Example implementation for affirmation practice
public class AffirmationStep : MonoBehaviour, StepBehavior
{
    [SerializeField] private GameObject affirmationDisplay;
    [SerializeField] private string[] affirmations = 
    {
        "I am calm",
        "I am strong",
        "I am capable"
    };
    
    public void ExecuteStep()
    {
        // Show the affirmation display
        if (affirmationDisplay != null)
        {
            affirmationDisplay.SetActive(true);
            
            // Could set up the affirmations in the UI here
            // e.g., GetComponent<AffirmationDisplay>().SetAffirmations(affirmations);
        }
    }
    
    public void EndStep()
    {
        // Hide the affirmation display
        if (affirmationDisplay != null)
        {
            affirmationDisplay.SetActive(false);
        }
    }
}