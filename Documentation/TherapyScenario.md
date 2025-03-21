# Therapy Scenario Example

This document outlines a sample 5-step therapy session that can be implemented in the Voice-Driven MR Accessibility Prototype. This scenario is designed to demonstrate the voice command functionality while providing a simple therapeutic experience.

## Session Overview

The sample therapy session is a brief relaxation and mindfulness exercise that guides the user through breathing, visualization, and affirmation practices. Each step is controlled by voice commands, allowing users with limited mobility to progress at their own pace.

## Detailed Steps

### Step 1: Introduction

**Instruction Text:**
"Welcome to the relaxation session. Find a comfortable position and prepare to follow along. Say 'Continue' when you're ready."

**Environment:**
- Soft, calming ambient music begins playing
- Simple, uncluttered environment with gentle colors
- Clear instruction panel visible in front of the user

**User Action:**
Say "Continue" or "Next step" to proceed

### Step 2: Breathing Exercise

**Instruction Text:**
"Let's start with a breathing exercise. Breathe in slowly for 4 counts, hold for 2, and breathe out for 6. Follow along with the visual guide. Say 'Continue' when you're ready to move on."

**Environment:**
- Visual breathing guide appears (pulsing circle that expands and contracts)
- Gentle audio cue to help with timing
- Optional: subtle visual particles that move with the breath rhythm

**User Action:**
Say "Continue" or "Next step" to proceed after practicing the breathing technique

### Step 3: Guided Visualization

**Instruction Text:**
"Imagine yourself in a peaceful place. It could be a beach, a forest, or anywhere you feel safe and calm. Take a moment to notice the details around you. What do you see? What do you hear? Say 'Continue' when you're ready."

**Environment:**
- Peaceful natural environment appears (beach scene with gentle waves)
- Ambient nature sounds (waves, birds)
- Soft lighting with warmth to create calming atmosphere

**User Action:**
Say "Continue" or "Next step" to proceed after spending time in visualization

### Step 4: Affirmation Practice

**Instruction Text:**
"Now we'll practice some positive affirmations. Repeat after me: 'I am calm. I am strong. I am capable.' Take your time with each phrase. Say 'Continue' when you're ready."

**Environment:**
- Text affirmations appear one by one with gentle animations
- Each affirmation is highlighted as it's meant to be spoken
- Subtle audio cue when each new affirmation appears

**User Action:**
Say "Continue" or "Next step" to proceed after practicing the affirmations

### Step 5: Conclusion

**Instruction Text:**
"Great job completing today's session. Take a moment to notice how you feel now compared to when we started. When you're ready to finish, say 'End session'."

**Environment:**
- Return to the calming environment from the beginning
- Gentle music fades in
- Visual representation of the session completion (e.g., a glowing orb or peaceful imagery)

**User Action:**
Say "End session" or "Stop therapy" to complete the session

## Technical Implementation

### Environment Assets
- Simple, low-poly 3D models for the therapy space
- Particle effects for breathing visualization
- Beach/nature scene for visualization step
- Animated text system for affirmations

### Audio Assets
- Ambient background music (looping)
- Nature sounds for visualization (waves, birds)
- Breath timing audio cues
- Success/completion sounds for transitions

### Script Configuration

In the `SessionController` script, the `therapySteps` array should be configured with these five steps, each with appropriate instructions and step-specific behaviors.

```csharp
// Example configuration in Unity Inspector
therapySteps[0].instructions = "Welcome to the relaxation session...";
therapySteps[0].stepBehavior = introStepBehavior;

therapySteps[1].instructions = "Let's start with a breathing exercise...";
therapySteps[1].stepBehavior = breathingStepBehavior;

// etc.
```

## Accessibility Considerations

- All text should be large and high-contrast for easy readability
- Audio cues should be distinct but gentle
- Visuals should avoid rapid movements or flashing elements
- Comfortable viewing distance for all UI elements
- Extended timeout duration to allow users plenty of time to respond

## Variations

This basic structure can be adapted for different therapeutic purposes:

1. **Pain Management Focus**: Adjust visualization to focus on pain reduction techniques
2. **Anxiety Reduction**: Incorporate more extensive breathing patterns and anxiety-specific affirmations
3. **Creative Expression**: Modify to guide users through an imaginative creative exercise
4. **Physical Therapy Support**: Adapt to guide through gentle movement or visualization of movement