# TherapyEnvironment Scene Setup Guide

This guide will help you finish setting up the TherapyEnvironment scene for the Voice-Driven MR Accessibility Prototype.

## 1. XR Origin and Camera Setup

1. If not already in the scene, add an XR Origin:
   - Right-click in Hierarchy → XR → XR Origin
   - Position it at (0, 0, 0)
   - Make sure it has the following components:
     * XR Origin component
     * Tracked Pose Driver component (if needed)

2. Configure the Main Camera:
   - Ensure it's a child of the XR Origin
   - Position at (0, 1.6, 0) relative to the XR Origin
   - Rotation at (0, 0, 0)
   - Add Audio Listener component
   - Set Clear Flags to "Solid Color"
   - Set Background to a calming color (e.g., #2A3B4C)

## 2. World Environment Setup

1. Create a simple environment:
   - Add a Floor plane:
     * Right-click in Hierarchy → 3D Object → Plane
     * Scale to (5, 1, 5)
     * Position at (0, 0, 0)
     * Add a simple material (use SkyMaterial or create a new one)

   - Add ambient lighting:
     * Right-click in Hierarchy → Light → Directional Light
     * Position it at (0, 3, 0)
     * Rotation at (50, -30, 0)
     * Set Intensity to 0.8
     * Set Color to warm white (#FFF5E8)

2. Setup skybox:
   - Go to Window → Rendering → Lighting
   - In Environment tab, set Skybox Material to SkyMaterial

## 3. UI Canvas Setup

1. Create a World Space Canvas:
   - Right-click in Hierarchy → UI → Canvas
   - Rename to "MainCanvas"
   - Set Render Mode to "World Space"
   - Set Canvas size to width=1, height=0.6
   - Position it at (0, 1.6, 1.5) - in front of the camera
   - Set the scale to (0.001, 0.001, 0.001) for proper sizing in world space

2. Add Canvas components:
   - Add "Canvas Group" component (allows opacity control)
   - Add "Billboard" component (if available) or create a script to make it face the camera

3. Add InstructionPanel:
   - Add the InstructionPanel prefab as child of MainCanvas
   - Position it at (0, 0.1, 0) relative to the Canvas
   - Make sure the InstructionPanel script component is assigned

4. Add StatusIndicator:
   - Add the StatusIndicator prefab as child of MainCanvas
   - Position it at (0, -0.15, 0) relative to the Canvas
   - Make sure the StatusIndicator script component is assigned

5. Create Feedback Message area:
   - Create an empty GameObject named "FeedbackMessage" as child of MainCanvas
   - Position it at (0, 0.25, 0) relative to the Canvas
   - Add Image component for background
   - Add TextMeshPro - Text component for message text
   - Add the FeedbackMessage script component
   - Configure as needed

6. Create Privacy Panel:
   - Create an empty GameObject named "PrivacyPanel" as child of MainCanvas
   - Position it at (0, 0, 0) relative to the Canvas
   - Initially set it to inactive
   - Add necessary Toggle components for privacy settings
   - Add the PrivacyPanel script component

## 4. Therapy Visualizations Setup

1. Add Breathing Visualizer:
   - Add the BreathingVisualizer prefab to the scene
   - Position it at (0, 1.6, 1.3) - in front of the camera
   - Initially set it to inactive
   - Make sure the BreathingVisualizer script component is assigned

2. Create Visualization Environment:
   - Create an empty GameObject named "VisualizationEnvironment"
   - Add the VisualizationEnvironment script component
   - Create/assign child objects for the peaceful environment (beach, forest, etc.)
   - Create a directional light specific to this environment
   - Add an AudioSource for ambient sounds
   - Initially set it to inactive

3. Add Affirmation Display:
   - Add the AffirmationDisplay prefab to the scene
   - Position it at (0, 1.6, 1.3) - in front of the camera
   - Initially set it to inactive
   - Make sure the AffirmationDisplay script component is assigned

## 5. Voice and Session Management

1. Add Voice Command Manager:
   - Create an empty GameObject named "VoiceManager"
   - Add the VoiceCommandManager script component
   - Add Meta.Wit.VoiceService component
   - Configure VoiceService:
     * Assign the WitTherapyConfiguration asset
     * Set Understanding to "Auto activation mode"
     * Set Max Recording Time to 10-15 seconds
     * Ensure "Voice Activation" is enabled

2. Add Session Controller:
   - Create an empty GameObject named "SessionManager"
   - Add the SessionController script component
   - Configure Therapy Steps array with 5 steps (as per TherapyScenario.md)
   - Assign references to UI elements and visualizations

3. Add Feedback Manager:
   - Create an empty GameObject named "FeedbackManager"
   - Add the FeedbackManager script component
   - Add AudioSource component for feedback sounds
   - Import and assign sound files for success, error, and timeout feedback
   - Assign references to UI elements

## 6. Connect Everything

1. Connect the Voice Command Manager:
   - Assign the SessionController reference
   - Assign the FeedbackManager reference
   - Set confidence threshold (0.8-0.9)
   - Configure command arrays if needed

2. Connect the Session Controller:
   - Assign the InstructionPanel text reference
   - Assign the VoiceCommandManager reference
   - Assign the FeedbackManager reference
   - Configure timeout duration (30-60 seconds)

3. Configure Therapy Steps:
   - For each of the 5 steps:
     * Set the instruction text (use content from TherapyScenario.md)
     * Assign the appropriate StepBehavior (BreathingVisualizer, VisualizationEnvironment, or AffirmationDisplay)

4. Set up feedback connections:
   - Connect the StatusIndicator to the VoiceCommandManager
   - Connect the FeedbackMessage to the FeedbackManager
   - Set up audio connections for feedback sounds

## 7. Final Checks

1. Ensure all script references are properly assigned
2. Test the basic scene layout in the editor
3. Verify that UI elements are correctly positioned
4. Check that all visualization components are initially inactive
5. Verify the VoiceCommandManager has correct Wit.ai configuration
6. Make sure the scene is set as the active scene in build settings