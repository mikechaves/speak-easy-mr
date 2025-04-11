SpeakEasy MR - Voice-Driven Therapy in Mixed Reality
Overview
SpeakEasy MR is an accessibility-focused Mixed Reality (MR) application developed as part of a Master of Design in Experience Design thesis. It aims to deliver engaging therapy sessions using natural voice commands as the primary interaction method, moving beyond simple preset commands. The project explores how intuitive voice control within an immersive MR environment (using Meta Quest Passthrough) can enhance therapeutic experiences, particularly for users who may benefit from hands-free, accessible interfaces. It provides guided therapy modules including breathing exercises, interactive visualization, and positive affirmations, now featuring Text-to-Speech (TTS) output for instructions and feedback.

Thesis Goals & Design Philosophy
Accessibility First: Design interactions that are intuitive and usable for individuals with varying physical abilities, prioritizing hands-free voice control and spoken feedback (TTS).

Natural Language Interaction: Move beyond rigid command structures towards understanding more natural user phrasing for controlling the experience.

Enhanced Immersion through Interaction: Allow users to actively influence their therapeutic environment (e.g., visualization scene) via voice, rather than just passively consuming content. Utilize supportive audio-visual and spoken elements.

Experience Design Focus: Prioritize the quality of the user experience within the MR therapeutic context.

Key Features
Voice Command Interface: Control session flow (start, next, end) and interact with specific modules (e.g., change light color/intensity in visualization) using voice. Built using Meta's Voice SDK (Wit.ai).

Text-to-Speech (TTS) Output: Provides spoken feedback for instructions (welcome, steps, completion), command results (success, error), timeouts, and suggestions using Meta Voice SDK's TTS capabilities. Enhances accessibility.

Multi-Step Therapy Flow: Guided progression through distinct therapy modules:

Breathing Exercise: Visual guide (scaling/coloring circle) synchronized with timed breathing patterns.

Interactive Visualization: User is placed in a simple environment (e.g., low-poly forest) where they can modify elements like lighting via voice commands.

Affirmation Display: Presentation of positive affirmations with timed transitions and subtle visual effects.

MR World Space UI: User interface elements (instructions, visualizers) are placed in world space for stability and presence within the Passthrough view. Consistent initial positioning relative to the user.

Ambient Background Music: Includes a calming background audio track (generated via SUNO AI) managed by a dedicated AudioManager to enhance immersion.

Audio Feedback: Plays confirmation/error sounds upon voice command execution/failure, providing immediate auditory feedback.

(Planned/Conceptual) Accessibility features like high contrast modes, adjustable text sizes, selectable TTS voices.

(Planned/Conceptual) Privacy features.

Technical Architecture
The application utilizes Unity and Meta's SDKs, built upon these core components:

SessionController: Manages the therapy session sequence using an array of TherapyStep data objects (for active steps) and separate Inspector fields for welcome/completion messages. Responsible for activating/deactivating the appropriate step behavior. Triggers TTS output for instructions via TTSManager.

VoiceCommandManager: Handles voice input, communication with Wit.ai (via AppVoiceExperience), intent/entity parsing, and calls appropriate methods on other managers/behaviors. Manages listener lifecycle. Triggers audio confirmation feedback via AudioManager.

FeedbackManager: Handles visual feedback (status indicators, messages) and plays audio cues (success/error sounds). Triggers TTS output for feedback messages via TTSManager.

TTSManager: (New) Singleton managing Text-to-Speech requests using Meta Voice SDK components (TTSSpeaker, TTSWit). Synthesizes and speaks text provided by SessionController and FeedbackManager.

AudioManager: Manages playback of ambient background music and one-shot UI sounds (like voice command confirmations/errors).

StepBehavior Interface: Defines ExecuteStep() and StopStep() methods implemented by components responsible for each therapy module's logic and visuals.

TherapyStep Class: A [System.Serializable] class holding instructions (for active steps) and a MonoBehaviour stepBehaviorComponent reference, configured in the SessionController Inspector.

Step Behavior Implementations:

BreathingVisualizer: Manages the scaling/coloring UI animation.

VisualizationEnvironment: Manages activation/deactivation of child environment objects, lighting, and responds to voice commands.

AffirmationDisplay: Manages the display and transitions of affirmation text.

UI System: Uses World Space Canvases parented under a Session UIs Root object, positioned consistently at startup. Includes MainCanvas for instructions and specific canvases/elements for step behaviors.

Getting Started
Clone the repository.

Open the project in Unity 2022.3 or newer.

Wit.ai Setup:

Configure a Wit.ai application. Follow instructions in Documentation/WitAiSetup.md.

Ensure necessary intents/entities are trained.

Link the WitConfiguration asset to the AppVoiceExperience component.

TTS Setup:

Ensure the necessary Meta Voice SDK TTS components (TTSSpeaker, TTSWit) are present in the scene (e.g., on a dedicated 'TextToSpeech' GameObject).

Assign the scene's TTSSpeaker component to the Speaker field in the TTSManager component's Inspector.

(Optional) Configure desired voice presets on the TTSSpeaker component.

Audio Setup:

Ensure an AudioManager prefab/instance exists in your main scene.

Assign audio clips to the Background Music Clip and Confirmation Sound Clip (and potentially error/timeout clips) fields in the AudioManager Inspector.

Session Controller Setup:

Assign the Therapy Steps array with your active step configurations (Breathing, Visualization, Affirmation, etc.).

Fill in the Welcome Message Text and Completion Message Text fields in the Inspector.

Build for your target platform (Meta Quest 2/3/Pro recommended for Passthrough).

Documentation
(Consider updating these or adding new ones)

Implementation Notes

UI Components Setup

Wit.ai Integration Guide

OpenXR Configuration

Testing Guide

Therapy Scenario

Recent Updates (Since Initial README)
Implemented core session flow using SessionController and StepBehavior interface pattern.

Implemented BreathingVisualizer, VisualizationEnvironment (with interactive light control), and AffirmationDisplay.

Added AudioManager and ambient background music.

Implemented audio confirmation sounds for voice commands.

Implemented Text-to-Speech (TTS) via TTSManager for instructions (welcome, steps, completion) and feedback (success, error, timeout, suggestions) using Meta Voice SDK.

Made welcome/completion messages configurable via Inspector fields.

Added spoken announcement of therapy step type (e.g., "Starting Breathing").

Fixed StepBehavior interface recognition errors by using GetComponent.

Refactored session start flow for smoother transition.

Resolved initial step visibility/activation issues.

Fixed UI positioning logic/references.

Fixed numerous bugs related to Voice SDK integration, command processing, coroutines, and Inspector references.

Refactored UI placement using a root object (Session UIs Root).

Adjusted scale and layout of UI elements.

Fixed Scene view gizmo visibility issues.

Controls
Voice Commands:

Start therapy / Begin session / Ready: Start the session from the idle state.

Next / Continue / Okay: Advance to the next step in the therapy sequence.

End session / Stop: End the current therapy session.

(During Visualization Step) Make the light [color] (e.g., "Make the light blue"): Change environment light color.

(During Visualization Step) Make it brighter / Increase brightness: Increase environment light intensity.

(During Visualization Step) Dim the light / Make it dimmer: Decrease environment light intensity.

(Planned) Repeat: Repeat current instruction/step.

Keyboard Fallbacks (Unity Editor Only):

S: Start session

N or C: Next/Continue step

E: End session

D: Log Debug status / Force listener restart

License
This project is available under the MIT License. (Ensure LICENSE file exists)

Acknowledgments
Meta's Voice SDK (Wit.ai, TTS)

Unity Engine & TextMeshPro

SUNO AI for generating the ambient background music track.

[Asset Store Attributions, if applicable]

[Thesis Advisor/Committee, if applicable]
