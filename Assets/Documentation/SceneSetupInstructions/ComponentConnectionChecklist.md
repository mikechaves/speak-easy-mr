# Component Connection Checklist

Use this checklist to verify all components are properly connected in the TherapyEnvironment scene.

## VoiceCommandManager Connections

- [ ] Assign `VoiceService` component reference
- [ ] Assign `VoiceEvents` component reference
- [ ] Assign `SessionController` reference
- [ ] Assign `FeedbackManager` reference
- [ ] Set confidence threshold (recommended: 0.85)
- [ ] Configure command arrays:
  - [ ] Start Session Commands: "start therapy", "begin session"
  - [ ] Next Step Commands: "next step", "continue"
  - [ ] End Session Commands: "end session", "stop therapy"

## SessionController Connections

- [ ] Assign `InstructionText` (TextMeshPro component from InstructionPanel)
- [ ] Assign `VoiceCommandManager` reference
- [ ] Assign `FeedbackManager` reference
- [ ] Set command timeout duration (recommended: 30-45 seconds)
- [ ] Configure Therapy Steps array (5 steps):
  - [ ] Step 1: Introduction
    - Instructions text set
    - No specific StepBehavior
  - [ ] Step 2: Breathing Exercise
    - Instructions text set
    - Assign BreathingVisualizer to stepBehavior
  - [ ] Step 3: Guided Visualization
    - Instructions text set
    - Assign VisualizationEnvironment to stepBehavior
  - [ ] Step 4: Affirmation Practice
    - Instructions text set
    - Assign AffirmationDisplay to stepBehavior
  - [ ] Step 5: Conclusion
    - Instructions text set
    - No specific StepBehavior

## FeedbackManager Connections

- [ ] Assign `StatusText` reference
- [ ] Assign `SuggestionText` reference
- [ ] Assign `MessageText` reference
- [ ] Assign `StatusIndicator` reference
- [ ] Assign `FeedbackAudioSource` reference
- [ ] Assign audio clips:
  - [ ] Success Sound
  - [ ] Error Sound
  - [ ] Timeout Sound
- [ ] Set colors:
  - [ ] Listening Color: Green
  - [ ] Idle Color: Gray
  - [ ] Error Color: Red
- [ ] Set message fade time (recommended: 3 seconds)

## InstructionPanel Configuration

- [ ] Properly positioned in Canvas
- [ ] InstructionPanel script attached
- [ ] InstructionText component assigned
- [ ] PanelBackground component assigned
- [ ] Default font size set (recommended: 0.05)
- [ ] Text color set (white)
- [ ] Background color set (dark with transparency)

## StatusIndicator Configuration

- [ ] Properly positioned below InstructionPanel
- [ ] StatusIndicator script attached
- [ ] StatusText component assigned
- [ ] StatusBackground component assigned
- [ ] StatusIcon component assigned
- [ ] Colors configured (listening, idle, error, processing)
- [ ] Pulse effect enabled for visual feedback

## BreathingVisualizer Configuration

- [ ] Properly positioned in front of user
- [ ] Initially set inactive
- [ ] BreathingVisualizer script attached
- [ ] BreathCircle transform assigned
- [ ] BreathCircleImage component assigned
- [ ] BreathAudioSource assigned
- [ ] Breathing pattern configured:
  - [ ] Inhale Duration: 4 seconds
  - [ ] Hold Duration: 2 seconds
  - [ ] Exhale Duration: 6 seconds
  - [ ] Total Breath Cycles: 3
- [ ] Visual settings configured:
  - [ ] Min Scale: 0.4
  - [ ] Max Scale: 1.0
  - [ ] Inhale Color: Light blue
  - [ ] Hold Color: Green
  - [ ] Exhale Color: Purple-blue

## VisualizationEnvironment Configuration

- [ ] Environment objects created (beach/forest elements)
- [ ] Initially set inactive
- [ ] VisualizationEnvironment script attached
- [ ] EnvironmentObject assigned
- [ ] EnvironmentLight assigned
- [ ] AmbientAudioSource assigned
- [ ] Transition settings configured:
  - [ ] Visual Fade Duration: 1.5 seconds
  - [ ] Audio Fade Duration: 2.0 seconds
- [ ] Animation settings configured (if using)

## AffirmationDisplay Configuration

- [ ] Properly positioned in front of user
- [ ] Initially set inactive
- [ ] AffirmationDisplay script attached
- [ ] AffirmationText component assigned
- [ ] AffirmationBackground component assigned
- [ ] AffirmationPanel transform assigned
- [ ] Affirmations array populated with positive phrases
- [ ] Display settings configured:
  - [ ] Display Duration: 5 seconds
  - [ ] Transition Duration: 1 second
  - [ ] Auto Advance: Enabled
- [ ] Visual settings configured:
  - [ ] Affirmation Colors array populated
  - [ ] Pulse effect settings

## General Scene Checks

- [ ] XR Origin properly configured
- [ ] Main Camera settings correct (position, clear flags, etc.)
- [ ] Lighting setup properly
- [ ] Skybox material assigned
- [ ] World Space Canvas properly positioned
- [ ] All UI elements readable from user's position
- [ ] TherapyEnvironment scene added to build settings
- [ ] Scene saves successfully without errors