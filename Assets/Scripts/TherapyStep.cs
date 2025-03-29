using UnityEngine;

// --- TherapyStep Class Definition ---
// Contains data for one step in the therapy session
[System.Serializable]
public class TherapyStep
{
    [Tooltip("Instructions displayed to the user for this step.")]
    public string instructions;

    // Reference to the MonoBehaviour component that handles the behavior for this step.
    // Drag the GameObject here that has the script implementing StepBehavior (e.g., BreathingVisualizer)
    [Tooltip("Drag the GameObject here that has the script implementing StepBehavior (e.g., BreathingVisualizer)")]
    public MonoBehaviour stepBehaviorComponent;
}

// --- StepBehavior Interface Definition ---
// Interface that all step behavior components must implement
public interface StepBehavior
{
    // Method called when the step should start its action
    void ExecuteStep();

    // Method called when the step should stop its action (e.g., when moving to the next step)
    void StopStep();
}

// --- NOTE: The example implementation classes below this line have been removed ---
// (BreathingExerciseStep, VisualizationStep, AffirmationStep were removed as they caused errors and are not used)