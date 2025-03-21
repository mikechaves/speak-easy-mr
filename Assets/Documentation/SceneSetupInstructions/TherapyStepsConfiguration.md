# Therapy Steps Configuration

This document details how to configure the 5 therapy steps in the SessionController for the Voice-Driven MR Accessibility Prototype.

## Step 1: Introduction

**Instructions Text:**
```
Welcome to the relaxation session. Find a comfortable position and prepare to follow along. Say 'Continue' when you're ready.
```

**Configuration:**
- No specific StepBehavior needed for this step
- Duration: User-controlled (proceeds when user says "Continue")
- Audio: Start gentle background music

## Step 2: Breathing Exercise

**Instructions Text:**
```
Let's start with a breathing exercise. Breathe in slowly for 4 counts, hold for 2, and breathe out for 6. Follow along with the visual guide. Say 'Continue' when you're ready to move on.
```

**Configuration:**
- StepBehavior: BreathingVisualizer
- Configuration:
  * Inhale Duration: 4 seconds
  * Hold Duration: 2 seconds
  * Exhale Duration: 6 seconds
  * Total Breath Cycles: 3 (user can say "Continue" at any time)
- Color settings:
  * Inhale Color: #3399FF (light blue)
  * Hold Color: #33CC66 (green)
  * Exhale Color: #6666EE (purple-blue)

**Implementation Notes:**
- The BreathingVisualizer prefab should show a circle that expands during inhale, stays steady during hold, and contracts during exhale
- Text in the center of the circle should indicate the current phase (Inhale/Hold/Exhale)
- Audio cues can be added for each phase if desired

## Step 3: Guided Visualization

**Instructions Text:**
```
Imagine yourself in a peaceful place. It could be a beach, a forest, or anywhere you feel safe and calm. Take a moment to notice the details around you. What do you see? What do you hear? Say 'Continue' when you're ready.
```

**Configuration:**
- StepBehavior: VisualizationEnvironment
- Environment settings:
  * Scene: Peaceful beach or forest environment
  * Ambient audio: Gentle waves or forest sounds
  * Lighting: Warm, soft lighting
  * Animation: Subtle movements (waves, leaves, clouds)

**Implementation Notes:**
- The VisualizationEnvironment should fade in gently
- Ambient audio should start quietly and increase in volume
- Visual elements should have gentle, soothing movements
- Environment should be simple and not visually overwhelming
- Light particles or effects can enhance the peaceful atmosphere

## Step 4: Affirmation Practice

**Instructions Text:**
```
Now we'll practice some positive affirmations. Repeat after me: 'I am calm. I am strong. I am capable.' Take your time with each phrase. Say 'Continue' when you're ready.
```

**Configuration:**
- StepBehavior: AffirmationDisplay
- Affirmations to display:
  * "I am calm"
  * "I am strong"
  * "I am capable"
  * "I embrace peace"
  * "I am worthy"
- Display settings:
  * Display Duration: 5 seconds per affirmation
  * Transition Duration: 1 second fade between affirmations
  * Auto Advance: True (cycles through affirmations automatically)
  * Colors: Cycle through calming colors for each affirmation

**Implementation Notes:**
- Each affirmation should appear with a gentle fade-in/fade-out
- Text should be large, clear, and high-contrast
- A subtle pulse effect on the text can help with focus
- Audio cue can mark transitions between affirmations
- User can say "Continue" at any time to move to the next step

## Step 5: Conclusion

**Instructions Text:**
```
Great job completing today's session. Take a moment to notice how you feel now compared to when we started. Say 'End session' when you're ready to finish.
```

**Configuration:**
- No specific StepBehavior needed for this step
- Duration: User-controlled (ends when user says "End session")
- Audio: Calming music that gently fades out when session ends

**Implementation Notes:**
- Background music should be soothing and create a sense of completion
- A subtle visual effect (like a gentle glow) can enhance the sense of accomplishment
- When user says "End session", all visualizations should fade out gracefully

## Session Controller Settings

**Global Settings:**
- Command Timeout Duration: 45 seconds (how long to wait before suggesting action)
- Auto Advance: Enable after second timeout (90 seconds total of inactivity)
- Audio: Include gentle audio cues for transitions between steps

**Step Transitions:**
- Each transition should have:
  * Visual feedback (success message)
  * Audio feedback (success sound)
  * Smooth transition between visualizations (fade out previous, fade in next)
  * Clear instruction update

## UI Elements for Steps

For each step, ensure:
1. Instruction text is clear and readable
2. Status indicator shows when listening for commands
3. Feedback messages appear for recognized commands
4. Visual elements are positioned comfortably in the user's field of view
5. All elements maintain high contrast and accessibility standards