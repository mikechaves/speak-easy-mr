# Unity Project Setup Guide

This guide provides step-by-step instructions for setting up the Unity project for the Voice-Driven MR Accessibility Prototype.

## 1. Create a New Unity Project

1. Open Unity Hub
2. Click "New Project"
3. Select "3D Core" template
4. Set Unity Version to 2022.3 LTS or newer
5. Name the project "VoiceDrivenMRAccessibility"
6. Choose a location for your project
7. Click "Create Project"

## 2. Install Required Packages

1. Open Package Manager (Window > Package Manager)
2. Select "Packages: Unity Registry" from the dropdown
3. Install the following packages:
   - TextMeshPro
   - XR Plugin Management
   - OpenXR Plugin
   - XR Interaction Toolkit

4. Import Meta XR SDK:
   - Download Meta XR SDK from https://developer.oculus.com/downloads/
   - Import the package via Assets > Import Package > Custom Package
   - Select all components when prompted

5. Import Wit.ai for Unity:
   - Download Wit.ai SDK from https://github.com/wit-ai/wit-unity
   - Or install via the Unity Asset Store if available
   - Import the package

## 3. Configure Project Settings

1. Configure XR Settings:
   - Go to Edit > Project Settings > XR Plugin Management
   - Enable OpenXR (see OpenXRConfiguration.md for details)

2. Configure Input System:
   - Go to Edit > Project Settings > Input System Package
   - Create a new Input Action Asset
   - Configure basic inputs for testing

3. Configure Player Settings:
   - Go to Edit > Project Settings > Player
   - Set Company Name and Product Name
   - Configure appropriate quality settings
   - Enable appropriate XR settings

## 4. Create Folder Structure

Create the following folders in your Assets directory:
- Scenes
- Scripts
- Prefabs
- Materials
- Audio
- Textures
- Documentation
- Resources

## 5. Create Main Scene

1. Create a new scene (File > New Scene)
2. Save it as "TherapyEnvironment" in the Scenes folder
3. Set up basic environment:
   - Add XR Origin (GameObject > XR > XR Origin)
   - Configure the Main Camera
   - Add a simple environment (floor, walls, basic objects)
   - Set appropriate lighting

## 6. Set Up UI Canvas

1. Create a Canvas (GameObject > UI > Canvas)
2. Configure Canvas:
   - Set Render Mode to "World Space"
   - Position in front of the camera
   - Set appropriate size and scale

3. Add UI Elements:
   - Add Text elements for instructions using TextMeshPro
   - Add status indicator panel
   - Add privacy settings UI elements
   - Add feedback message area

## 7. Add Core Scripts

1. Create script files in the Scripts folder:
   - VoiceCommandManager.cs
   - SessionController.cs
   - FeedbackManager.cs
   - PrivacySettings.cs
   - TherapyStep.cs

2. Attach scripts to appropriate GameObjects:
   - Add VoiceCommandManager to a new GameObject named "VoiceManager"
   - Add SessionController to a new GameObject named "SessionManager"
   - Add FeedbackManager to the UI Canvas
   - Add PrivacySettings to the Settings UI panel

## 8. Configure Wit.ai Integration

1. Set up Wit.ai component:
   - Add Wit.ai speech component to the VoiceManager GameObject
   - Configure the component with your App ID and Client Token
   - Set appropriate activation mode (see WitAiSetup.md)

2. Connect events:
   - Connect the Wit.ai response events to your VoiceCommandManager
   - Set up error handling and response processing

## 9. Create Therapy Session Content

1. Create TherapyStep prefabs:
   - Define 3-5 therapy steps in the SessionController
   - Create any visual elements needed for each step
   - Set up step-specific behaviors if needed

2. Add audio feedback:
   - Add AudioSource components for feedback sounds
   - Import and assign audio clips for success, error, and completion events

## 10. Testing and Optimization

1. Test in the Unity Editor:
   - Use Unity's XR simulation if available
   - Test voice recognition using the microphone
   - Verify all session steps work correctly

2. Build and deploy:
   - Configure build settings for your target platform
   - Build and deploy to test device
   - Follow the testing guide to verify functionality

## 11. Create Documentation

Add documentation files to your project:
- README.md with project overview
- WitAiSetup.md with Wit.ai configuration details
- OpenXRConfiguration.md with OpenXR setup instructions
- TestingGuide.md with testing procedures
- Code comments throughout your scripts

## 12. Version Control Setup

1. Initialize git repository:
   ```bash
   git init
   ```

2. Add appropriate .gitignore file for Unity
3. Make initial commit:
   ```bash
   git add .
   git commit -m "Initial commit: Project setup"
   ```

## Building Blocks Integration

When using Meta SDK Building Blocks:

1. Import the Building Blocks package
2. In the scene, navigate to the Building Blocks panel
3. Drag and drop the following useful blocks:
   - Voice Service Block (for Wit.ai integration)
   - Hand Tracking Blocks (optional for gesture combination)
   - UI Blocks for accessible interfaces
4. Configure each block according to documentation
5. Connect blocks to your custom scripts as needed