# SpeakEasy MR - Voice-Driven Therapy in Mixed Reality

## Overview

SpeakEasy MR is an accessibility-focused application for delivering therapy sessions in mixed reality using voice commands as the primary interaction method. It provides a hands-free, immersive experience for guided therapy including breathing exercises, visualization, and affirmations.

## Key Features

- **Voice Command Interface**: Control the entire experience using just your voice
- **Accessibility-First Design**: High contrast mode, adjustable text sizes, and multi-modal feedback
- **Enhanced UI System**: Intuitive interface that follows user's gaze in MR
- **Privacy Focused**: Options for local processing and transparent data handling
- **Therapy Progression**: Step-by-step guided therapy with visual and audio cues

## Technical Architecture

The application is built on these core components:

- **VoiceCommandManager**: Handles voice recognition and command processing using Meta's Wit.ai SDK
- **SessionController**: Manages the therapy session flow and step progression
- **UI System**: Available in both basic and enhanced versions for different needs
- **TherapyStep Behaviors**: Implementations for different therapy techniques

## Getting Started

1. Clone the repository
2. Open the project in Unity 2022.3 or newer
3. Configure the Wit.ai integration following the instructions in `Documentation/WitAiSetup.md`
4. Build for your target platform (Meta Quest recommended)

## Documentation

- [Implementation Notes](Documentation/ImplementationNotes.md)
- [UI Components Setup](Documentation/UIComponentsSetup.md)
- [OpenXR Configuration](Documentation/OpenXRConfiguration.md)
- [Testing Guide](Documentation/TestingGuide.md)
- [Therapy Scenario](Documentation/TherapyScenario.md)

## Recent Updates

- Improved UI organization with proper namespace usage 
- Enhanced UI components now properly contained in the `UI.Enhanced` namespace
- Better documentation for UI components and system architecture
- Fixed integration between SessionController and EnhancedUIController
- Improved accessibility features and visual feedback

## Controls

Voice commands include:
- "Begin" or "Start therapy" to start a session
- "Continue" or "Next" to move to the next step
- "Repeat" to repeat the current instruction
- "End" to complete the session

Keyboard fallbacks (development only):
- S: Start session
- N or C: Next/Continue
- E: End session
- D: Debug status

## License

This project is available under [license information].

## Acknowledgments

- Meta's Voice SDK and Wit.ai for speech recognition
- Unity's XR Interaction Toolkit
- TextMeshPro for improved text rendering
- [Other libraries and acknowledgments]