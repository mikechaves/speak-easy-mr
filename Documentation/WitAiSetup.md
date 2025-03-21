# Wit.ai Setup Guide

This guide provides step-by-step instructions for setting up Wit.ai for the Voice-Driven MR Accessibility Prototype.

## 1. Create a Wit.ai Account

1. Visit [Wit.ai](https://wit.ai/) and sign up for an account
2. Log in to the Wit.ai dashboard

## 2. Create a New App

1. Click "New App" on the Wit.ai dashboard
2. Provide the following details:
   - App name: "Voice MR Accessibility"
   - Language: English (or your preferred language)
   - Privacy: Private (recommended)
3. Click "Create"

## 3. Define Intents

Create the following intents in your Wit.ai app:

### start_session Intent

1. Navigate to "Intents" in the sidebar
2. Click "+ Add Intent" and name it "start_session"
3. Add the following utterances:
   - "start therapy"
   - "begin session"
   - "let's start"
   - "start"
   - "begin"
   - "ready to start"
   - "I'm ready"

### next_step Intent

1. Click "+ Add Intent" and name it "next_step"
2. Add the following utterances:
   - "next step"
   - "continue"
   - "go on"
   - "next"
   - "proceed"
   - "move forward"
   - "I'm ready for the next step"

### end_session Intent

1. Click "+ Add Intent" and name it "end_session"
2. Add the following utterances:
   - "end session"
   - "stop therapy"
   - "finish"
   - "I'm done"
   - "complete session"
   - "exit"
   - "quit"

## 4. Train Your Model

1. Click "Train" to build your model with the defined intents
2. Test your intents using the "Utterance" text box at the top
3. Continue adding utterances and training until you achieve satisfactory results

## 5. Export App Settings

1. Go to "Settings" in the sidebar
2. Under "App Details", find your Client Access Token
3. Copy this token for use in Unity

## 6. Import to Unity

1. Open your Unity project
2. Install the Wit.ai SDK for Unity via the Package Manager
3. Configure the Wit.ai service in Unity:
   - Go to Project Settings > Wit.ai (or equivalent)
   - Paste your Client Access Token
   - Click "Apply"

## 7. Configure Voice Activation

1. Add a Wit.ai Voice Service component to your scene's VoiceCommandManager GameObject
2. Configure the following settings:
   - Speech activation: Voice activated
   - Enable Active/Deactivate methods for manual control
   - Set confidence threshold to 0.85 (adjust based on testing)

## 8. Local Processing Setup (Optional)

If you want to enable local processing for enhanced privacy:

1. Check if your target platform supports on-device processing
2. In the Wit.ai dashboard, enable "Local Processing" if available
3. Download the compatible on-device model package
4. Import the model into your Unity project
5. Configure the VoiceCommandManager to use local processing when available

## 9. Testing and Refinement

1. Test voice commands in the Unity Editor
2. Analyze recognition accuracy
3. Return to Wit.ai to add more utterances or adjust settings as needed
4. Continue iterating until voice recognition meets your accessibility requirements

## Troubleshooting

- **Recognition Issues**: Add more varied utterances to your intents
- **Performance Problems**: Try adjusting the confidence threshold
- **Integration Errors**: Ensure your Wit.ai SDK is up to date
- **Permission Issues**: Verify microphone permissions are properly configured for your target platform