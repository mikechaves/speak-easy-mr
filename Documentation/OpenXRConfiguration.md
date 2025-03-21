# OpenXR Configuration Guide

This document provides step-by-step instructions for configuring OpenXR in the Voice-Driven MR Accessibility Prototype.

## 1. Install Required Packages

1. Open Unity Package Manager (Window > Package Manager)
2. Select "Packages: Unity Registry" from the dropdown
3. Install the following packages:
   - XR Plugin Management
   - OpenXR Plugin
   - XR Interaction Toolkit (for XR Origin and setup)

## 2. Configure XR Plugin Management

1. Go to Edit > Project Settings > XR Plugin Management
2. Click "Install XR Plugin Management" if not already installed
3. Check "Initialize XR on Startup"
4. Select the "OpenXR" tab
5. Enable OpenXR for your target platforms (PC, Android for Quest, etc.)
6. Click "Apply"

## 3. Configure OpenXR Features

1. In the Project Settings window, select "XR Plugin Management > OpenXR"
2. Under "OpenXR Features", enable the following:
   - Microsoft Mixed Reality Motion Controller Profile
   - Meta Quest Support (if targeting Meta Quest devices)
   - Mixed Reality Features (if targeting HoloLens)
   - Oculus Touch Controller Profile (for Meta Quest)
   - Eye Gaze Interaction Profile (if using eye tracking)

## 4. Set Up Interaction Profiles

1. In the OpenXR settings, go to the "Interaction Profiles" section
2. Enable the profiles appropriate for your target device:
   - For Meta Quest: Meta Touch Controller Profile
   - For HoloLens: Microsoft Hand Interaction Profile
   - For general MR: Microsoft Motion Controller Profile

## 5. Set Up XR Camera Rig

1. In your Unity scene, create a new empty GameObject named "XR Origin"
2. Add the "XR Origin" component to this GameObject
3. Create a Camera object as a child of XR Origin (or use the Main Camera)
4. Configure the Camera:
   - Set Tag to "MainCamera"
   - Position at (0, 0, 0) relative to XR Origin
   - Add required camera components (e.g., Audio Listener)
   - Set Clear Flags to "Solid Color" with an appropriate background color
   - Configure Field of View and other settings as needed

## 6. Configure Input System for Voice Commands

1. Go to Edit > Project Settings > Input System Package (install if prompted)
2. Create a new Input Action Asset or use the default one
3. Add actions for voice commands (optional, as our main input is through Wit.ai)
4. Link these actions to the voice command processing system

## 7. Test OpenXR Configuration

1. In the Unity Editor, go to Window > XR > OpenXR > Runtime Debugger
2. Verify that your configuration is valid
3. Address any warnings or errors that appear

## 8. Platform-Specific Settings

### For Meta Quest

1. In Player Settings > Android:
   - Set "Graphics APIs" to OpenGLES3
   - Enable "Multithreaded Rendering"
   - Set minimum API level to Android 10.0 (API level 29) or higher
   - Configure package name and other Android settings

2. In XR Plugin Management > Android:
   - Ensure OpenXR is enabled
   - Configure Meta Quest-specific features

### For Windows Mixed Reality

1. In Player Settings > PC:
   - Set "Graphics APIs" to Direct3D11
   - Configure appropriate quality settings

2. In XR Plugin Management > Windows:
   - Ensure OpenXR is enabled
   - Configure Windows MR-specific features

## 9. Build and Test

1. Configure Build Settings for your target platform
2. Include only the necessary scenes
3. Build and deploy to test device
4. Verify that the XR environment initializes correctly
5. Test that the camera follows head movement properly

## Troubleshooting

- **OpenXR Validation Errors**: Check the Runtime Debugger for specific issues
- **Missing Controllers**: Verify the correct interaction profiles are enabled
- **Initialization Failures**: Ensure the XR Plugin Management is set to initialize on startup
- **Tracking Issues**: Verify that your XR Origin is configured correctly
- **Performance Problems**: Adjust quality settings for your target platform

## Notes for Voice-Only Interaction

Since this project focuses on voice commands rather than controllers, you can:

1. Disable controller input in the Input System settings
2. Configure the XR Origin without controller models or hand tracking
3. Focus on optimizing the camera and voice input systems
4. Still ensure the OpenXR configuration is correct for proper head tracking and environment rendering