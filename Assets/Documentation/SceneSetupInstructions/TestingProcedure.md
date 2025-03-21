# Testing Procedure for TherapyEnvironment Scene

Use this guide to test the TherapyEnvironment scene and verify that all components are working properly.

## Editor Testing

### 1. Basic Scene Validation

1. Open the TherapyEnvironment scene
2. Enter Play mode
3. Verify that:
   - The scene loads without errors
   - The XR Origin and camera are positioned correctly
   - The UI canvas and elements are visible
   - The initial instruction text is displayed

### 2. Voice Command Testing

Since voice commands might be difficult to test in the editor, you can manually trigger them:

1. Select the VoiceManager GameObject
2. In the Inspector, locate the VoiceCommandManager component
3. Use Debug buttons or create a UI Test Panel with these buttons:
   - Test "Start Session" command
   - Test "Next Step" command
   - Test "End Session" command

### 3. Session Flow Testing

1. Trigger "Start Session" command
2. Verify:
   - Introduction text appears
   - Status indicator shows correct state
   - Success feedback is displayed

3. Trigger "Next Step" command to enter Breathing Exercise
4. Verify:
   - Breathing Visualizer appears
   - Instructions update correctly
   - Breathing animation works (circle expands/contracts)
   - Breathing timing is correct (4-2-6 pattern)

5. Trigger "Next Step" command to enter Guided Visualization
6. Verify:
   - Visualization Environment appears
   - Ambient sounds play
   - Lighting and visuals look correct
   - Instructions update correctly

7. Trigger "Next Step" command to enter Affirmation Practice
8. Verify:
   - Affirmation Display appears
   - Affirmations cycle correctly
   - Text is clearly readable
   - Visual effects work properly
   - Instructions update correctly

9. Trigger "Next Step" command to enter Conclusion
10. Verify:
    - Instructions update to conclusion text
    - Previous visualizations disappear
    - Status indicator shows correct state

11. Trigger "End Session" command
12. Verify:
    - Session completes successfully
    - UI returns to initial state
    - Success feedback is displayed

### 4. Error Handling Testing

1. Test timeout functionality:
   - Start a session
   - Let it sit without interaction
   - Verify timeout messages appear
   - Verify session auto-advances after extended timeout

2. Test error feedback:
   - Simulate a recognition error
   - Verify error message appears
   - Verify status indicator shows error state
   - Verify error sound plays

## Device Testing

### 1. XR Device Setup

1. Build the project for your target XR device
2. Install on the device
3. Launch the application

### 2. Voice Command Testing

1. Test "Start Therapy" command
   - Say "Start therapy" or "Begin session"
   - Verify session starts correctly

2. Progress through therapy steps
   - Say "Continue" or "Next step" at each step
   - Verify each visualization appears correctly
   - Verify voice recognition works consistently

3. End the session
   - Say "End session" or "Stop therapy"
   - Verify session ends correctly

### 3. Accessibility Testing

1. Test with different voice volumes
   - Speak commands at normal volume
   - Speak commands quietly
   - Verify recognition still works

2. Test with different speaking speeds
   - Speak commands at normal pace
   - Speak commands slowly
   - Verify recognition still works

3. Test UI readability
   - Check text size and contrast from different angles
   - Verify all instructions are clearly readable
   - Check that status indicators are easily visible

4. Test with background noise
   - Add some ambient noise
   - Verify voice commands still work
   - Adjust confidence threshold if needed

### 4. Performance Testing

1. Monitor frame rate
   - Ensure stable performance throughout the session
   - Watch for drops during visualization transitions

2. Check battery usage
   - Monitor battery drain during extended sessions
   - Optimize if excessive

3. Test voice processing latency
   - Measure time between command and response
   - Verify feedback is timely and responsive

## Troubleshooting Common Issues

1. **Voice commands not recognized:**
   - Check microphone permissions
   - Verify Wit.ai service is properly configured
   - Test with simpler commands
   - Adjust confidence threshold

2. **Visualizations not appearing:**
   - Check that prefabs are correctly assigned
   - Verify step behaviors are properly connected
   - Check for null references in the console

3. **UI positioning issues:**
   - Adjust Canvas position and scale
   - Use world space anchoring for consistent placement
   - Check billboard/facing behavior

4. **Performance problems:**
   - Simplify visualization effects
   - Reduce audio quality if needed
   - Check for memory leaks in repeated transitions

5. **Inconsistent behavior:**
   - Verify all connections in the Component Connection Checklist
   - Check for race conditions in coroutines
   - Ensure proper state management in SessionController