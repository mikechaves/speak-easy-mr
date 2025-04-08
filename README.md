# SpeakEasy MR - Voice-Driven Therapy in Mixed Reality

## Overview

SpeakEasy MR is an accessibility-focused Mixed Reality (MR) application developed as part of a Master of Design in Experience Design thesis. It aims to deliver engaging therapy sessions using natural voice commands as the primary interaction method, moving beyond simple preset commands. The project explores how intuitive voice control within an immersive MR environment (using Meta Quest Passthrough) can enhance therapeutic experiences, particularly for users who may benefit from hands-free, accessible interfaces. It provides guided therapy modules including breathing exercises, interactive visualization, and positive affirmations.

## Thesis Goals & Design Philosophy

* **Accessibility First:** Design interactions that are intuitive and usable for individuals with varying physical abilities, prioritizing hands-free voice control.
* **Natural Language Interaction:** Move beyond rigid command structures towards understanding more natural user phrasing for controlling the experience.
* **Enhanced Immersion through Interaction:** Allow users to actively influence their therapeutic environment (e.g., visualization scene) via voice, rather than just passively consuming content.
* **Experience Design Focus:** Prioritize the quality of the user experience within the MR therapeutic context.

## Key Features

* **Voice Command Interface**: Control session flow (start, next, end) and interact with specific modules (e.g., change light color/intensity in visualization) using voice. Built using Meta's Voice SDK (Wit.ai).
* **Multi-Step Therapy Flow**: Guided progression through distinct therapy modules:
    * **Breathing Exercise:** Visual guide (scaling/coloring circle) synchronized with timed breathing patterns.
    * **Interactive Visualization:** User is placed in a simple environment (e.g., low-poly forest) where they can modify elements like lighting via voice commands.
    * **Affirmation Display:** Presentation of positive affirmations with timed transitions and subtle visual effects.
* **MR World Space UI**: User interface elements (instructions, visualizers) are placed in world space for stability and presence within the Passthrough view. Consistent initial positioning relative to the user.
* **(Planned/Conceptual)** Accessibility features like high contrast modes, adjustable text sizes.
* **(Planned/Conceptual)** Privacy features.

## Technical Architecture

The application utilizes Unity and Meta's SDKs, built upon these core components:

* **`SessionController`**: Manages the therapy session sequence using an array of `TherapyStep` data objects. Responsible for activating/deactivating the appropriate step behavior for the current step.
* **`VoiceCommandManager`**: Handles voice input, communication with Wit.ai (via `AppVoiceExperience`), intent/entity parsing (`modify_light`, `light_color`, `intensity_direction`, navigation commands), and calls appropriate methods on other managers/behaviors. Manages listener lifecycle (auto-start, auto-restart between active steps).
* **`StepBehavior` Interface**: Defines `ExecuteStep()` and `StopStep()` methods implemented by components responsible for each therapy module's logic and visuals.
* **`TherapyStep` Class**: A `[System.Serializable]` class holding instructions and a `MonoBehaviour stepBehaviorComponent` reference for each step, configured in the `SessionController` Inspector. (Uses `MonoBehaviour` field for reliable serialization).
* **Step Behavior Implementations**:
    * `BreathingVisualizer`: Manages the scaling/coloring UI animation for breathing guidance.
    * `VisualizationEnvironment`: Manages activation/deactivation of child environment objects, ambient audio/lighting fades, and responds to voice commands for light modification.
    * `AffirmationDisplay`: Manages the display, timing, and transitions of affirmation text.
* **UI System**: Currently uses World Space Canvases parented under a `Session UIs Root` object, positioned consistently at startup. Includes `MainCanvas` for instructions and specific canvases/elements for step behaviors.

## Getting Started

1.  Clone the repository.
2.  Open the project in Unity 2022.3 or newer.
3.  **Wit.ai Setup:**
    * Configure a Wit.ai application. Follow instructions in `Documentation/WitAiSetup.md` for initial setup.
    * Ensure the app is trained with necessary intents (e.g., `next_step`, `end_session`, `modify_light`) and relevant entities (custom `light_color`, `intensity_direction`, potentially `therapy_command`). Add varied utterances for robust recognition.
    * Link the corresponding `WitConfiguration` asset within the Unity project (assigned to `AppVoiceExperience` component).
4.  Build for your target platform (Meta Quest 2/3/Pro recommended for Passthrough).

## Documentation

*(Consider updating these or adding new ones)*
* [Implementation Notes](Documentation/ImplementationNotes.md)
* [UI Components Setup](Documentation/UIComponentsSetup.md)
* [Wit.ai Integration Guide](Documentation/WitAiSetup.md) *(Rename suggestion)*
* [OpenXR Configuration](Documentation/OpenXRConfiguration.md)
* [Testing Guide](Documentation/TestingGuide.md)
* [Therapy Scenario](Documentation/TherapyScenario.md)

## Recent Updates (Since Initial README)

* Implemented core session flow using `SessionController` and `StepBehavior` interface pattern.
* Fixed step behavior activation/deactivation lifecycle managed by `SessionController`.
* Implemented `BreathingVisualizer` with scaling/color animation.
* Implemented `VisualizationEnvironment` with activation/deactivation and **interactive voice control** for light color and intensity using Wit.ai intents/entities.
* Added initial assets (low poly forest) to the visualization step.
* Implemented `AffirmationDisplay` with cycling text and transitions.
* Fixed numerous bugs related to Voice SDK integration (`AppVoiceExperience` event handling, API usage), command processing (double triggers, recognition issues for "Continue"), coroutine activation on inactive objects, and Inspector reference serialization.
* Refactored UI placement using a root object (`Session UIs Root`) for consistent initial positioning in World Space.
* Adjusted scale and layout of UI elements.
* Fixed Scene view gizmo visibility issues for easier UI design.

## Controls

**Voice Commands:**
* `Start therapy` / `Begin session` / `Ready`: Start the session from the idle state.
* `Next` / `Continue` / `Okay`: Advance to the next step in the therapy sequence.
* `End session` / `Stop`: End the current therapy session.
* **(During Visualization Step)** `Make the light [color]` (e.g., "Make the light blue"): Change environment light color.
* **(During Visualization Step)** `Make it brighter` / `Increase brightness`: Increase environment light intensity.
* **(During Visualization Step)** `Dim the light` / `Make it dimmer`: Decrease environment light intensity.
* *(Planned)* `Repeat`: Repeat current instruction/step.

**Keyboard Fallbacks (Unity Editor Only):**
* `S`: Start session
* `N` or `C`: Next/Continue step
* `E`: End session
* `D`: Log Debug status / Force listener restart

## License

This project is available under MIT License.

## Acknowledgments

* Meta's Voice SDK and Wit.ai for speech recognition and NLU.
* Unity Engine & TextMeshPro.