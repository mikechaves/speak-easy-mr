# Testing Guide for Voice-Driven MR Accessibility Prototype

This guide provides comprehensive testing procedures to ensure the prototype functions correctly and meets accessibility requirements.

## 1. Voice Recognition Testing

### Basic Voice Command Recognition

1. **Start Session Commands**
   - Test each command: "start therapy", "begin session", "let's start"
   - Verify correct session initiation for each variant
   - Test with different voice volumes and speaking speeds
   - Document recognition success rate

2. **Next Step Commands**
   - Test each command: "next step", "continue", "go on"
   - Verify correct step progression for each variant
   - Test during various stages of the therapy session
   - Document recognition success rate

3. **End Session Commands**
   - Test each command: "end session", "stop therapy", "I'm done"
   - Verify correct session termination for each variant
   - Document recognition success rate

### Advanced Voice Testing

1. **Varied Speech Patterns**
   - Test with different accents
   - Test with varied speaking speeds (slow, normal, fast)
   - Test with different voice pitches and volumes
   - Document how well the system handles variations

2. **Background Noise Testing**
   - Test in quiet environment (baseline)
   - Test with ambient noise (e.g., low conversation, TV in background)
   - Test with intermittent noise (e.g., door closing)
   - Document impact of noise on recognition accuracy

3. **Confidence Threshold Testing**
   - Test with default confidence threshold (0.85)
   - Adjust threshold up and down in 0.05 increments
   - Find optimal balance between false positives and missed commands
   - Document recommended threshold based on testing

## 2. User Experience Testing

### Session Flow Testing

1. **Full Session Run-through**
   - Complete entire therapy/creative session using only voice commands
   - Document any points where flow is interrupted
   - Verify smooth transitions between steps

2. **Timeout Testing**
   - Test the automatic timeout functionality
   - Verify appropriate feedback when timeout occurs
   - Test auto-advance feature after prolonged inactivity
   - Verify session can be resumed after timeout

3. **Error Recovery Testing**
   - Intentionally use incorrect or unrecognized commands
   - Verify helpful error messages appear
   - Test suggestion system for alternative commands
   - Verify ability to recover and continue session

### Feedback System Testing

1. **Visual Feedback**
   - Verify text updates are clear and readable
   - Test status indicator for all states (idle, listening, error)
   - Verify appropriate colors are used for different states
   - Test visibility in different lighting conditions

2. **Audio Feedback**
   - Verify all audio cues play correctly
   - Test volume levels are appropriate
   - Verify distinct sounds for success/error/timeout
   - Test audio feedback without visual feedback

3. **Combined Feedback Testing**
   - Test synchronization of audio and visual feedback
   - Verify one feedback mode can compensate if another fails
   - Document overall effectiveness of feedback system

## 3. Accessibility Testing

### Inclusive Design Testing

1. **Variable Speech Testing**
   - Test with users who have speech differences or difficulties
   - Document recognition accuracy with diverse speech patterns
   - Adjust settings based on findings

2. **Cognitive Accessibility**
   - Evaluate clarity of instructions
   - Verify error messages are understandable and not technical
   - Test with users who have different cognitive abilities
   - Document areas for improvement

3. **Sensory Considerations**
   - Test with different visual settings (contrast, text size)
   - Verify audio feedback is effective without visual cues
   - Test visual feedback is effective without audio cues

### Privacy Settings Testing

1. **Local Processing Option**
   - Test voice recognition with local processing enabled
   - Compare accuracy to cloud processing
   - Verify data isn't sent externally when local processing is enabled
   - Document any performance differences

2. **Data Sharing Controls**
   - Verify data sharing toggles function correctly
   - Test that settings persist across sessions
   - Confirm appropriate data handling based on settings

## 4. Performance Testing

### System Performance

1. **Resource Usage**
   - Monitor CPU/GPU usage during session
   - Test for memory leaks during extended use
   - Verify stable frame rate throughout the experience

2. **Latency Testing**
   - Measure time between voice command and system response
   - Test response time for local vs. cloud processing
   - Document acceptable latency thresholds

### Device-Specific Testing

1. **Meta Quest Testing**
   - Test on Meta Quest 2 and newer devices
   - Verify OpenXR configuration works properly
   - Document any device-specific issues

2. **Other MR Devices**
   - Test on Windows Mixed Reality headsets if available
   - Test on HoloLens if available
   - Document compatibility issues and solutions

## 5. Test Reporting

For each test performed, document the following:

1. **Test Details**
   - Test name and description
   - Testing environment and conditions
   - Device and software versions used

2. **Results**
   - Success/failure status
   - Quantitative metrics (recognition rate, response time)
   - Qualitative observations

3. **Issues Identified**
   - Description of any problems found
   - Severity rating (Critical, Major, Minor, Cosmetic)
   - Steps to reproduce

4. **Recommendations**
   - Suggested fixes or improvements
   - Prioritization of issues
   - Further testing needed

## Accessibility Test Metrics

When evaluating accessibility, record the following metrics:

1. **Voice Recognition Success Rate**
   - Percentage of commands correctly recognized
   - Broken down by command type and user characteristics

2. **Error Recovery Rate**
   - How often users can successfully recover from errors
   - Average number of attempts needed

3. **User Satisfaction**
   - Subjective rating of ease of use
   - Comfort level during extended use
   - Overall accessibility rating

## Final Validation Checklist

Before considering the prototype ready for demonstration:

- [ ] All critical voice commands function with >90% success rate
- [ ] Error handling provides clear guidance to the user
- [ ] Feedback systems (visual and audio) work independently and together
- [ ] Session flow progresses properly through all steps
- [ ] Privacy options function as expected
- [ ] System performs well on target MR devices
- [ ] All critical accessibility requirements are met