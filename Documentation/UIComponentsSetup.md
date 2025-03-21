# UI Components Setup Guide

This document provides instructions for setting up and configuring the UI components in the Voice-Driven MR Accessibility Prototype.

## 1. Instruction Panel

The Instruction Panel is the primary UI element that displays therapy session instructions to the user.

### Setup Instructions

1. Add the InstructionPanel prefab to your scene:
   - Drag `Prefabs/UI/InstructionPanel.prefab` into your Canvas
   - Position it at eye level in front of the user

2. Configure the panel:
   - Select the InstructionPanel GameObject
   - In the Inspector, adjust the following parameters:
     * Default Font Size: Size of the instruction text (default: 0.05)
     * Text Color: Color of the instruction text (default: white)
     * Background Color: Color and opacity of the panel (default: dark gray, 80% opacity)
     * Panel Width/Height: Dimensions of the panel (default: 0.6 x 0.3)

3. Connect to SessionController:
   - Assign the InstructionPanel's instructionText field to the SessionController's instructionText reference
   - This allows the SessionController to update instructions as steps progress

4. Test positioning:
   - Call PositionInFrontOfCamera(2.0f) to place the panel 2m in front of the user
   - Adjust the distance parameter as needed for comfort in your XR environment

## 2. Status Indicator

The Status Indicator shows the current state of voice recognition and provides feedback about system status.

### Setup Instructions

1. Add the StatusIndicator prefab to your scene:
   - Drag `Prefabs/UI/StatusIndicator.prefab` into your Canvas
   - Position it below the Instruction Panel

2. Configure appearance:
   - Select the StatusIndicator GameObject
   - Adjust colors for different states:
     * Listening Color: When actively listening (default: green)
     * Idle Color: When ready but not listening (default: gray)
     * Error Color: When an error occurs (default: red)
     * Processing Color: When processing a command (default: yellow)

3. Configure animation:
   - Enable/disable Pulse When Listening for visual feedback
   - Adjust Pulse Speed and alpha range for the pulsing effect

4. Connect to VoiceCommandManager:
   - Reference the StatusIndicator in your VoiceCommandManager
   - Update the status using UpdateStatus(VoiceRecognitionStatus status, string message)

5. Position relative to Instruction Panel:
   - Call PositionRelativeToPanel(instructionPanel.transform, 0.15f) to position below
   - Adjust the vertical offset as needed

## 3. Feedback Message

The Feedback Message provides temporary notifications in response to user actions or system events.

### Setup Instructions

1. Create a FeedbackMessage GameObject:
   - Add the FeedbackMessage script to a new GameObject in your Canvas
   - Configure a background Image and TextMeshPro Text component

2. Configure appearance:
   - Set message colors for different types:
     * Success Color: For successful operations (default: green)
     * Error Color: For error messages (default: red)
     * Neutral Color: For informational messages (default: blue)

3. Configure animation:
   - Set Fade In Duration for how quickly messages appear
   - Set Display Duration for how long messages remain visible
   - Set Fade Out Duration for how quickly messages disappear

4. Connect to FeedbackManager:
   - Reference the FeedbackMessage in your FeedbackManager
   - Call appropriate methods:
     * ShowSuccessMessage(string message)
     * ShowErrorMessage(string message)
     * ShowInfoMessage(string message)

5. Position relative to other UI:
   - Call PositionRelativeToPanel(instructionPanel.transform, -0.15f) to position above
   - Adjust the vertical offset as needed

## 4. Privacy Panel

The Privacy Panel allows users to control privacy settings related to voice processing and data sharing.

### Setup Instructions

1. Create a Privacy Panel:
   - Create a new Panel GameObject in your Canvas
   - Add the PrivacyPanel script
   - Add Toggle components for local processing and data sharing options

2. Configure toggles:
   - Set up the Local Processing Toggle with appropriate label
   - Set up the Data Sharing Toggle with appropriate label
   - Connect toggles to the PrivacyPanel script references

3. Add status text:
   - Add a TextMeshPro Text component to display current privacy settings
   - Assign it to the Status Text field in the PrivacyPanel script

4. Connect to VoiceCommandManager:
   - Subscribe to the PrivacyPanel's events in VoiceCommandManager:
     * OnLocalProcessingChanged
     * OnDataSharingChanged

5. Add show/hide functionality:
   - Create a button to toggle the privacy panel visibility
   - Connect the button's onClick event to the TogglePanel method

## 5. UI Layout and Hierarchy

Organize your UI components in a clear hierarchy:

```
Canvas (World Space)
├── InstructionPanel
│   └── InstructionText
├── StatusIndicator
│   ├── StatusIcon
│   └── StatusText
├── FeedbackMessage
│   └── MessageText
└── PrivacyPanel
    ├── LocalProcessingToggle
    ├── DataSharingToggle
    ├── StatusText
    └── CloseButton
```

## 6. Accessibility Considerations

1. **High contrast and readability:**
   - Use high contrast colors (white text on dark backgrounds)
   - Set appropriate text sizes (0.04-0.08 units for comfortable reading)
   - Position UI elements at comfortable viewing distances

2. **Multi-modal feedback:**
   - Pair visual status changes with audio cues
   - Ensure status changes can be perceived through multiple channels

3. **Customization options:**
   - Allow adjustment of text size
   - Provide high-contrast mode option
   - Support different viewing distances

4. **Testing:**
   - Test UI visibility in different lighting conditions
   - Verify readability at different viewing angles
   - Ensure UI remains visible during head movements