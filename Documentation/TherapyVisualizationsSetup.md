# Therapy Visualizations Setup Guide

This guide explains how to set up and configure the therapy step visualizations in the Voice-Driven MR Accessibility Prototype.

## 1. Breathing Visualization

The breathing visualizer provides a visual guide for breathing exercises, with an expanding and contracting circle that guides the user through inhale, hold, and exhale phases.

### Setup Instructions

1. Add the BreathingVisualizer prefab to your scene:
   - Drag `Prefabs/Therapy/BreathingVisualizer.prefab` into your Canvas
   - Position it centered in front of the user's view

2. Configure the breathing pattern:
   - Select the BreathingVisualizer GameObject
   - In the Inspector, adjust the following parameters:
     * Inhale Duration: Time in seconds for the inhale phase (default: 4s)
     * Hold Duration: Time in seconds for the breath hold (default: 2s)
     * Exhale Duration: Time in seconds for the exhale phase (default: 6s)
     * Total Breath Cycles: Number of full breath cycles to perform (default: 3)

3. Customize visual appearance:
   - Adjust the Min Scale and Max Scale to control how much the circle expands
   - Modify the Inhale Color, Hold Color, and Exhale Color as needed
   - Connect a ParticleSystem to the Breath Particles field for enhanced visuals

4. Add audio feedback:
   - Create an AudioSource component on the BreathingVisualizer GameObject
   - Assign it to the Breath Audio Source field
   - Add inhale and exhale sound clips to the respective fields

## 2. Guided Visualization Environment

The visualization environment creates a peaceful scene for the guided visualization step, with ambient sounds and gentle animations.

### Setup Instructions

1. Add the VisualizationEnvironment prefab to your scene:
   - Drag `Prefabs/Therapy/VisualizationEnvironment.prefab` into your scene
   - Position it to surround the user

2. Configure the environment:
   - Select the VisualizationEnvironment GameObject
   - Assign your 3D models to the Environment Object field
   - Add a directional light to the Environment Light field
   - Connect a particle system for atmospheric effects

3. Set up audio:
   - Add an AudioSource component for ambient sounds
   - Assign it to the Ambient Audio Source field
   - Set Audio Fade Duration to control how smoothly audio fades in/out
   - Import and assign appropriate nature/ambient sound files

4. Configure animations:
   - Enable Animate Elements to activate gentle movements
   - Adjust Animation Speed and Wave Height to control movement intensity
   - Elements will automatically animate when the environment is shown

## 3. Affirmation Display

The affirmation display shows positive affirmations that fade in and out during the affirmation practice step.

### Setup Instructions

1. Add the AffirmationDisplay prefab to your scene:
   - Drag `Prefabs/Therapy/AffirmationDisplay.prefab` into your Canvas
   - Position it at eye level in front of the user

2. Configure affirmations:
   - Select the AffirmationDisplay GameObject
   - In the Inspector, edit the Affirmations array to add your custom affirmations
   - Default affirmations include:
     * "I am calm"
     * "I am strong"
     * "I am capable"
     * "I embrace peace"
     * "I am worthy"

3. Adjust timing and transitions:
   - Set Display Duration for how long each affirmation is shown
   - Set Transition Duration for fade in/out time
   - Enable/disable Auto Advance to control whether affirmations change automatically

4. Customize appearance:
   - Modify the Affirmation Colors array to set colors for each affirmation
   - Adjust Pulse Amount and Pulse Speed to control the gentle pulsing animation
   - Add audio feedback by connecting an AudioSource and transition sound

## 4. Integration with Session Controller

To integrate these visualizations with the therapy flow:

1. Create TherapyStep instances in the SessionController:
   ```csharp
   [SerializeField] private TherapyStep[] therapySteps = new TherapyStep[5];
   ```

2. Assign the visualization behaviors to each step:
   - In the Unity Inspector, expand the therapySteps array
   - For the breathing step, assign your BreathingVisualizer component to the stepBehavior field
   - For the visualization step, assign your VisualizationEnvironment component
   - For the affirmation step, assign your AffirmationDisplay component

3. Configure instructions for each step in the Inspector:
   - Set appropriate instruction text for each TherapyStep
   - Ensure the instructions match what the visualization is showing

## 5. Testing the Visualizations

1. Test each visualization independently:
   - Select the GameObject and use the context menu to test
   - For BreathingVisualizer, call StartBreathingVisualization()
   - For VisualizationEnvironment, call ShowEnvironment()
   - For AffirmationDisplay, call StartAffirmationDisplay()

2. Test the complete flow:
   - Enter Play mode in the Unity Editor
   - Use the voice commands to progress through the therapy session
   - Verify that each visualization appears and behaves correctly

## 6. Optimization Tips

1. **Performance optimization:**
   - Use simple, low-poly models for the visualization environment
   - Limit particle effects to maintain good performance on XR devices
   - Use TextMeshPro for text rendering efficiency

2. **Accessibility enhancements:**
   - Ensure high contrast between text and backgrounds
   - Make animations gentle and non-distracting
   - Provide clear audio cues that match visual transitions