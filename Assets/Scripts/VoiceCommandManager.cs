using UnityEngine;
using UnityEngine.Events;
using Meta.WitAi; // Needed for VoiceService base? Check if still needed.
using Meta.WitAi.Json; // Needed for WitResponseNode and direct access
using Meta.Voice; // Needed for AppVoiceExperience? Check specific SDK version.
using Oculus.Voice; // Needed for AppVoiceExperience based on earlier fix.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // Used for ContainsAny optimization if added back

public class VoiceCommandManager : MonoBehaviour
{
    [Header("Voice Service")]
    // Ensure this is assigned an AppVoiceExperience component in the Inspector
    [SerializeField] private VoiceService voiceService;

    [Tooltip("Check this to print additional diagnostic info")]
    [SerializeField] private bool verboseDiagnostics = true;
    [Tooltip("Set to lower value (0.5-0.7) if having trouble with command recognition")]
    [SerializeField] private float matchThreshold = 0.65f; // Note: Current ContainsAny doesn't use this; uses Levenshtein directly

    [Header("References")]
    [SerializeField] private SessionController sessionController;
    [SerializeField] private FeedbackManager feedbackManager;

    [Header("Command Settings")]
    [SerializeField] private string[] startSessionCommands = { "start therapy", "begin session", "start", "begin", "therapy", "ready" }; // Added "ready" based on logs
    [SerializeField] private string[] nextStepCommands = { "next step", "continue", "next", "go on", "proceed", "forward", "advance", "cont", "move on", "go ahead", "keep going", "okay", "ok" }; // Added okay/ok
    [SerializeField] private string[] endSessionCommands = { "end session", "stop therapy", "end", "stop", "finish", "exit", "quit" };

    [Header("Step Behavior References")] // Optional header for organization
    [SerializeField] private VisualizationEnvironment visualizationEnvironment;
    // Add references to other behaviors here if needed for other custom commands later

    // Common mistaken transcriptions and their correct mappings
    private readonly Dictionary<string, string> commonMistranscriptions = new Dictionary<string, string>
    {
        // "Next" often gets transcribed as...
        { "next up", "next" }, { "text", "next" }, { "nest", "next" }, { "necks", "next" },
        { "next to", "next" }, { "next please", "next" }, { "next one", "next" },
        { "next time", "next" }, { "next day", "next" },

        // "Begin" often gets transcribed as...
        { "began", "begin" }, { "beginning", "begin" }, { "begins", "begin" },

        // "Start" often gets transcribed as...
        { "started", "start" }, { "starting", "start" }, { "starts", "start" },

        // "End" often gets transcribed as...
        { "and", "end" }, { "ending", "end" }, { "ends", "end" }
    };

    private bool isListening = false;
    private int retryCount = 0;
    private const int MAX_RETRIES = 3;
    private AppVoiceExperience appVoiceExperience; // Store the cast reference

    private void Start()
    {
        // --- Reference Checks ---
        if (feedbackManager == null) {
            Debug.LogError("Feedback Manager is not assigned! User feedback will not work.");
            return;
        }
        if (sessionController == null) {
            Debug.LogError("Session Controller is not assigned! Session management will not work.");
            feedbackManager.UpdateStatusIndicator(false, "Session controller missing");
            return;
        }
        if (voiceService == null) {
            Debug.LogError("Voice Service is not assigned! Voice commands will not work.");
            feedbackManager.UpdateStatusIndicator(false, "Voice service missing");
            return;
        }

        // --- Cast and Verify Voice Service Type ---
        appVoiceExperience = voiceService as AppVoiceExperience;
        if (appVoiceExperience == null) {
            Debug.LogError($"Assigned VoiceService is NOT an AppVoiceExperience. Found type: {voiceService.GetType().Name}. Cannot proceed.");
            feedbackManager.UpdateStatusIndicator(false, "Incorrect voice service type");
            return;
        }
        if(verboseDiagnostics) Debug.Log($"Voice service confirmed as AppVoiceExperience: {appVoiceExperience.name}");

        // --- Check Wit Configuration ---
        if (appVoiceExperience.RuntimeConfiguration == null || string.IsNullOrEmpty(appVoiceExperience.RuntimeConfiguration.witConfiguration?.GetClientAccessToken()))
        {
             Debug.LogError("Wit configuration asset or client access token might be missing on AppVoiceExperience!");
             feedbackManager.UpdateStatusIndicator(false, "Wit config/token error");
             // Consider returning here if critical
        }
        else {
             if(verboseDiagnostics) Debug.Log("Found Wit configuration with token on AppVoiceExperience.");
        }

        // --- Setup Event Listeners ---
        SetupTranscriptionEvents();

        // --- Initial State ---
        feedbackManager.UpdateStatusIndicator(false, "Voice recognition ready");
        Debug.Log("VoiceCommandManager initialized.");

        // --- Automatic Activation ---
        Debug.Log("Attempting to automatically activate voice recognition on start.");
        StartListening();
    }

    private void SetupTranscriptionEvents()
    {
        if (appVoiceExperience == null) return;

        try
        {
            if(verboseDiagnostics) Debug.Log("Adding event listeners to AppVoiceExperience.VoiceEvents");

            // Subscribe using the correct event properties exposed by AppVoiceExperience.VoiceEvents
            // Ensure handler method signatures match your SDK version!
            appVoiceExperience.VoiceEvents.OnSend?.AddListener(OnSend);
            appVoiceExperience.VoiceEvents.OnPartialTranscription?.AddListener(OnPartialTranscriptionReceived);
            appVoiceExperience.VoiceEvents.OnFullTranscription?.AddListener(OnFullTranscriptionReceived);
            appVoiceExperience.VoiceEvents.OnStartListening?.AddListener(OnStartListening);
            appVoiceExperience.VoiceEvents.OnStoppedListening?.AddListener(OnStoppedListening);
            appVoiceExperience.VoiceEvents.OnError?.AddListener(OnError);
            appVoiceExperience.VoiceEvents.OnResponse?.AddListener(OnResponse);
            appVoiceExperience.VoiceEvents.OnAborted?.AddListener(OnAborted);

            if(verboseDiagnostics) Debug.Log("Successfully added listeners.");
        }
        catch (Exception e)
        {
             Debug.LogError($"Error setting up voice events: {e.Message}\n{e.StackTrace}");
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        if (appVoiceExperience != null && appVoiceExperience.VoiceEvents != null)
        {
            try
            {
                 if(verboseDiagnostics) Debug.Log("Removing event listeners from AppVoiceExperience.VoiceEvents");
                 appVoiceExperience.VoiceEvents.OnSend?.RemoveListener(OnSend);
                 appVoiceExperience.VoiceEvents.OnPartialTranscription?.RemoveListener(OnPartialTranscriptionReceived);
                 appVoiceExperience.VoiceEvents.OnFullTranscription?.RemoveListener(OnFullTranscriptionReceived);
                 appVoiceExperience.VoiceEvents.OnStartListening?.RemoveListener(OnStartListening);
                 appVoiceExperience.VoiceEvents.OnStoppedListening?.RemoveListener(OnStoppedListening);
                 appVoiceExperience.VoiceEvents.OnError?.RemoveListener(OnError);
                 appVoiceExperience.VoiceEvents.OnResponse?.RemoveListener(OnResponse);
                 appVoiceExperience.VoiceEvents.OnAborted?.RemoveListener(OnAborted);
            }
             catch (Exception e)
            {
                 Debug.LogWarning($"Error removing voice event listeners: {e.Message}");
            }
        }
    }

    // --- Event Handlers ---

    private void OnSend(Meta.WitAi.Requests.VoiceServiceRequest request)
    {
        if(verboseDiagnostics) Debug.Log($"Voice Request Sent: {request?.Options?.RequestId}");
    }

    private void OnStartListening()
    {
        isListening = true;
        feedbackManager?.UpdateStatusIndicator(true, "Listening...");
        if(verboseDiagnostics) Debug.Log("Event: Started listening for voice commands");
    }

    private void OnStoppedListening()
    {
        isListening = false;
        feedbackManager?.UpdateStatusIndicator(false, "Processing...");
         if(verboseDiagnostics) Debug.Log("Event: Stopped listening for voice commands");
    }

    // Ensure signature (string code, string message) matches your SDK's VoiceEvents.OnError
    private void OnError(string code, string message)
    {
        Debug.LogError($"Event: Voice recognition error: {code} - {message}");
        isListening = false; // Ensure state is correct
        feedbackManager?.UpdateStatusIndicator(false, "Error: " + message);
        feedbackManager?.PlayErrorFeedback("Voice recognition error. Please try again.");

        if (sessionController != null && sessionController.GetCurrentState() == SessionState.Active)
        {
            Debug.Log("In active session - attempting auto-restart after error");
            StartCoroutine(RestartListeningAfterDelay(2.0f));
        }
    }

    // This method now primarily handles SPECIFIC intents or early responses.
    // Transcript processing for navigation is done in OnFullTranscriptionReceived.
    private void OnResponse(WitResponseNode response)
    {
        if (response == null) {
            Debug.LogWarning("OnResponse called with null response node.");
            HandleLowConfidence(); // Assume low confidence if response is null
            return;
        }

        if(verboseDiagnostics) Debug.Log($"FULL RESPONSE NODE: {response.ToString()}");

        bool processedIntentInResponse = false; // Flag if we handled a specific intent HERE

        // --- Check for Visualization Step Specific Commands ---
        int visualizationStepIndex = 2; // Make sure this index matches your SessionController setup
        bool isVisualizationStepActive = sessionController != null &&
                                         sessionController.GetCurrentState() == SessionState.Active &&
                                         sessionController.GetCurrentStepIndex() == visualizationStepIndex;

        if (isVisualizationStepActive)
        {
            // Access Intents using JSON-style indexing
            string intentName = null;
            float intentConfidence = 0f;
            try {
                if (response["intents"] != null && response["intents"].Count > 0) {
                    intentName = response["intents"][0]["name"]?.Value;
                    intentConfidence = response["intents"][0]["confidence"]?.AsFloat ?? 0f;
                    if(verboseDiagnostics && !string.IsNullOrEmpty(intentName)) Debug.Log($"Intent Detected: {intentName} (Confidence: {intentConfidence:F2})");
                }
            } catch (Exception e) {
                Debug.LogWarning($"Error accessing intents: {e.Message}");
            }

            // Check for modify_light intent
            if (!string.IsNullOrEmpty(intentName) && intentName.Equals("modify_light", StringComparison.OrdinalIgnoreCase) && intentConfidence > 0.7f)
            {
                 if (visualizationEnvironment == null) {
                     Debug.LogError("Detected modify_light intent but VisualizationEnvironment reference is missing!");
                 } else {
                    bool actionTaken = false;
                     // Access Entities using JSON-style indexing
                     string colorEntityKey = "light_color:light_color"; // Use the exact name from Wit.ai
                     string intensityEntityKey = "intensity_direction:intensity_direction"; // Use the exact name from Wit.ai
                     try {
                         // Check for color entity
                         if (response["entities"] != null && response["entities"][colorEntityKey] != null && response["entities"][colorEntityKey].Count > 0) {
                             string colorName = response["entities"][colorEntityKey][0]["value"]?.Value;
                             if (!string.IsNullOrEmpty(colorName)) {
                                 Debug.Log($"Extracted color entity value: {colorName}");
                                 Color targetColor = ParseColor(colorName);
                                 visualizationEnvironment.SetLightColor(targetColor);
                                 feedbackManager?.PlaySuccessFeedback($"Light color set to {colorName}");
                                 actionTaken = true;
                             }
                         }
                         // Check for intensity direction entity
                         if (response["entities"] != null && response["entities"][intensityEntityKey] != null && response["entities"][intensityEntityKey].Count > 0) {
                             string direction = response["entities"][intensityEntityKey][0]["value"]?.Value;
                             if (!string.IsNullOrEmpty(direction)) {
                                 Debug.Log($"Extracted intensity direction entity value: {direction}");
                                 if (direction.Equals("brighter", StringComparison.OrdinalIgnoreCase)) {
                                     visualizationEnvironment.AdjustLightIntensity(1.0f);
                                     feedbackManager?.PlaySuccessFeedback("Light brighter");
                                     actionTaken = true;
                                 } else if (direction.Equals("dimmer", StringComparison.OrdinalIgnoreCase)) {
                                     visualizationEnvironment.AdjustLightIntensity(-1.0f);
                                     feedbackManager?.PlaySuccessFeedback("Light dimmer");
                                     actionTaken = true;
                                 }
                             }
                         }
                     } catch (Exception e) {
                          Debug.LogWarning($"Error accessing entities for modify_light: {e.Message}");
                     }

                     if (actionTaken) {
                        processedIntentInResponse = true; // Mark intent processed
                        if (!isListening) StartCoroutine(RestartListeningAfterDelay(1.0f)); // Restart listener
                     }
                 }
                 // Mark as processed even if reference was missing or no entities found, to prevent fall-through
                 processedIntentInResponse = true;
            }
        } // End of visualization step check

        // --- Handle cases where NO specific intent was processed ---
        // If we didn't handle an intent here, AND we also failed to get a transcript later
        // in OnFullTranscriptionReceived, THEN maybe flag low confidence.
        // No call to ProcessTranscript here.
        if (!processedIntentInResponse)
        {
            string transcript = response["text"]?.Value ?? response["_text"]?.Value; // Check if transcript exists for logging
            if (string.IsNullOrEmpty(transcript) && verboseDiagnostics) {
                 Debug.Log("OnResponse: No specific intent processed in this handler and no transcript found in typical fields (will rely on OnFullTranscriptionReceived).");
                 // Only call HandleLowConfidence if OnFullTranscriptionReceived ALSO doesn't get a transcript?
                 // Might be safer to let OnFullTranscriptionReceived handle it.
            } else if (verboseDiagnostics) {
                 Debug.Log($"OnResponse: No specific intent processed. Transcript '{transcript}' should be handled by OnFullTranscriptionReceived.");
            }
        }
    }

    // This method handles the FINAL transcription and processes navigation commands.
    private void OnFullTranscriptionReceived(string transcript)
    {
        // Log even if transcript is empty, helps debugging
        Debug.Log($"****** OnFullTranscriptionReceived FIRED with transcript: '{transcript}' ******");
        if(verboseDiagnostics) Debug.Log($"Event: Full transcription received: '{transcript}'");

        // Only process if we actually got some text
        if (!string.IsNullOrEmpty(transcript))
        {
            // Process the transcript for navigation commands (Start, Next, End)
            bool commandProcessedSuccessfully = ProcessTranscript(transcript);

            SessionState stateAfterProcessing = sessionController != null ? sessionController.GetCurrentState() : SessionState.Idle;
            Debug.Log($"<color=#FFBF00>OnFullTranscriptionReceived: State after processing='{stateAfterProcessing}', isListening flag (before check)='{isListening}'</color>");

            // --- Restart Logic ---
            if (commandProcessedSuccessfully && stateAfterProcessing == SessionState.Active)
            {
                Debug.Log("<color=magenta>Command processed & session active. Explicitly calling StopListening() before scheduling restart.</color>");
                StopListening();
                Debug.Log("<color=green>Scheduling listener restart via coroutine...</color>");
                StartCoroutine(RestartListeningAfterDelay(0.5f)); // Short delay seems okay now
            }
            else
            {
                 if (!commandProcessedSuccessfully) Debug.LogWarning("<color=orange>OnFullTranscriptionReceived: Not restarting (command not processed successfully).</color>");
                 else if (stateAfterProcessing != SessionState.Active) Debug.LogWarning($"<color=orange>OnFullTranscriptionReceived: Not restarting (session not Active, state={stateAfterProcessing}).</color>");
            }
        }
        else
        {
             // If OnFullTranscription itself received an empty transcript, treat as low confidence
             Debug.LogWarning("OnFullTranscriptionReceived: Received empty transcript.");
             HandleLowConfidence();
        }
    }

    private void OnPartialTranscriptionReceived(string transcript)
    {
        if(verboseDiagnostics) Debug.Log($"Event: Partial transcription: '{transcript}'");
        // feedbackManager?.UpdatePartialTranscript(transcript); // Disabled based on previous steps
    }

    private void OnAborted()
    {
        if(verboseDiagnostics) Debug.Log("Event: Voice recognition aborted");
        isListening = false;
        feedbackManager?.UpdateStatusIndicator(false, "Voice recognition ready");

        if (sessionController != null && sessionController.GetCurrentState() == SessionState.Active)
        {
            Debug.Log("In active session - attempting auto-restart after abort");
            StartCoroutine(RestartListeningAfterDelay(2.0f));
        }
    }


    // --- Core Logic ---

    // Returns true if a command was successfully matched and processed, false otherwise.
    private bool ProcessTranscript(string transcript)
    {
        // This null/empty check should technically be redundant now if called only from OnFullTranscriptionReceived
        // But keep it for safety.
        if (string.IsNullOrEmpty(transcript))
        {
            if(verboseDiagnostics) Debug.LogWarning("ProcessTranscript called with empty transcript.");
            // HandleLowConfidence(); // Avoid calling HandleLowConfidence twice if OnFullTranscriptionReceived already did
            return false;
        }

        string lowerTranscript = transcript.ToLower();
        SessionState currentState = sessionController.GetCurrentState();
        string correctedTranscript = ApplyTranscriptionCorrections(lowerTranscript);
        bool commandMatched = false;

        if (verboseDiagnostics && lowerTranscript != correctedTranscript) {
             Debug.Log($"Corrected transcript: '{lowerTranscript}' -> '{correctedTranscript}'");
        }
        if (verboseDiagnostics) {
            Debug.Log("---COMMAND DETECTION ANALYSIS---");
            Debug.Log($"Original: '{transcript}'");
            Debug.Log($"Corrected: '{correctedTranscript}'");
            Debug.Log($"Current Session State: {currentState}");
        }

        // Prioritize based on state - Check for continue/next first if active
        if (currentState == SessionState.Active && IsPotentialContinueCommand(correctedTranscript)) {
             if (verboseDiagnostics) Debug.Log($"PRIORITY CONTINUE COMMAND DETECTED: '{correctedTranscript}'");
             sessionController.AdvanceToNextStep();
             feedbackManager?.PlaySuccessFeedback("Moving to next step");
             commandMatched = true;
        }
        // Check for start only if idle
        else if (currentState == SessionState.Idle && IsPotentialStartCommand(correctedTranscript)) {
             if (verboseDiagnostics) Debug.Log($"PRIORITY START COMMAND DETECTED: '{correctedTranscript}'");
             sessionController.StartSession();
             feedbackManager?.PlaySuccessFeedback("Session started");
             commandMatched = true;
        }
        // Check for end session
        else if (ContainsAny(correctedTranscript, endSessionCommands)) {
             Debug.Log($"COMMAND DETECTED: END SESSION - '{correctedTranscript}'");
             sessionController.EndSession();
             feedbackManager?.PlaySuccessFeedback("Session completed");
             commandMatched = true;
        }
        // Fallback checks if specific priorities didn't match
        else if (!commandMatched) // Only check these if no priority match occurred
        {
             // Check start commands again (covers cases like "start" said during Active state, which should maybe do nothing or give feedback)
             if (currentState == SessionState.Idle && ContainsAny(correctedTranscript, startSessionCommands)) {
                 Debug.Log($"COMMAND DETECTED: START SESSION (Fallback) - '{correctedTranscript}'");
                 sessionController.StartSession();
                 feedbackManager?.PlaySuccessFeedback("Session started");
                 commandMatched = true;
             }
             // Check next commands again (covers cases where IsPotentialContinueCommand was too broad/narrow)
             else if (currentState == SessionState.Active && ContainsAny(correctedTranscript, nextStepCommands)) {
                 Debug.Log($"COMMAND DETECTED: NEXT STEP (Fallback) - '{correctedTranscript}'");
                 sessionController.AdvanceToNextStep();
                 feedbackManager?.PlaySuccessFeedback("Moving to next step");
                 commandMatched = true;
             }
             // Last resort simple words for continue
             else if (currentState == SessionState.Active && IsLastResortContinueWord(correctedTranscript)) {
                 Debug.Log($"LAST RESORT CONTINUE MATCH: '{correctedTranscript}'");
                 sessionController.AdvanceToNextStep();
                 feedbackManager?.PlaySuccessFeedback("Moving to next step");
                 commandMatched = true;
             }
        }

        // Reset retry count only if a command was matched
        if (commandMatched) {
            retryCount = 0;
        }

        if (!commandMatched)
        {
            Debug.Log($"NO COMMAND MATCH: '{correctedTranscript}'");
            feedbackManager?.PlayErrorFeedback("I didn't understand that command");
            SuggestAlternativeCommands();
            // HandleLowConfidence might be called by OnFullTranscriptionReceived if transcript was empty, avoid calling twice.
        }

        return commandMatched;
    }

    // Helper for priority check
    private bool IsPotentialContinueCommand(string correctedTranscript) {
        // More specific check for common single words
        if (correctedTranscript == "next" || correctedTranscript == "continue" || correctedTranscript == "okay" || correctedTranscript == "ok") return true;
        // Check for phrases containing key indicators
        return correctedTranscript.Contains("next step") || correctedTranscript.Contains("go on") ||
               correctedTranscript.Contains("proceed") || correctedTranscript.Contains("move on") ||
               correctedTranscript.Contains("go ahead");
    }

    // Helper for priority check
    private bool IsPotentialStartCommand(string correctedTranscript) {
         if (correctedTranscript == "start" || correctedTranscript == "begin" || correctedTranscript == "ready") return true;
        return correctedTranscript.Contains("start therapy") || correctedTranscript.Contains("begin session");
    }

    // Helper for last resort check - maybe remove if priority/ContainsAny are sufficient
    private bool IsLastResortContinueWord(string correctedTranscript) {
        string[] words = correctedTranscript.Split(new char[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string word in words)
        {
            // Simplified - rely on nextStepCommands array mostly
            if (word.Equals("yes") || word.Equals("yeah") || word.Equals("fine") || word.Equals("sure") || word.Equals("alright"))
            {
                return true;
            }
        }
        return false;
    }

    private string ApplyTranscriptionCorrections(string transcript)
    {
        if (string.IsNullOrEmpty(transcript)) return transcript;
        string lowerTranscript = transcript.ToLower();
        // Simple dictionary lookup/replace
        foreach (var kvp in commonMistranscriptions) {
            if (lowerTranscript.Contains(kvp.Key)) { // Basic check, could be more robust
                lowerTranscript = lowerTranscript.Replace(kvp.Key, kvp.Value);
                if (verboseDiagnostics) Debug.Log($"Applied correction: '{kvp.Key}' -> '{kvp.Value}'");
                // Consider breaking after first match if appropriate
            }
        }
        // Removed fuzzy matching for simplicity/performance unless specifically needed
        return lowerTranscript;
    }

    private bool ContainsAny(string source, string[] keywords)
    {
        if (string.IsNullOrEmpty(source) || keywords == null || keywords.Length == 0) return false;
        // Use word boundary check for more accuracy
        // Regex might be more robust: System.Text.RegularExpressions.Regex.IsMatch(source, $@"\b({string.Join("|", keywords.Select(k => System.Text.RegularExpressions.Regex.Escape(k)))})\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        string paddedSource = " " + source.ToLower().Replace(".", "").Replace(",", "").Replace("!", "").Replace("?", "") + " ";
        foreach (string keyword in keywords) {
            if (string.IsNullOrEmpty(keyword)) continue;
            string spacedKeyword = " " + keyword.ToLower() + " ";
            if (paddedSource.Contains(spacedKeyword)) {
                return true;
            }
        }
        return false;
    }


    // --- Utility & Helper Methods ---

    public void StartListening()
    {
        if (appVoiceExperience == null) { Debug.LogError("Cannot start listening, AppVoiceExperience is not set."); return; }

        if (verboseDiagnostics) Debug.Log($"<color=yellow>StartListening Check: Current isListening flag = {isListening}, AppVoiceExperience.Active = {appVoiceExperience.Active}</color>");

        if (!isListening && !appVoiceExperience.Active)
        {
            if(verboseDiagnostics) Debug.Log("<color=green>Condition Met: Attempting to Activate voice service...</color>");
            try
            {
                var runtimeConfig = appVoiceExperience.RuntimeConfiguration;
                var witConfig = runtimeConfig?.witConfiguration;
                string token = witConfig?.GetClientAccessToken();
                if (runtimeConfig == null || string.IsNullOrEmpty(token)) {
                    Debug.LogError("<color=red>Cannot Activate: Wit configuration or token missing/invalid.</color>");
                    feedbackManager?.UpdateStatusIndicator(false, "Config/Token Error");
                    return;
                }
                if(verboseDiagnostics) Debug.Log("<color=lime>Calling appVoiceExperience.Activate()...</color>");
                appVoiceExperience.Activate();
            }
            catch (Exception e) {
                Debug.LogError($"<color=red>Error during appVoiceExperience.Activate(): {e.Message}\n{e.StackTrace}</color>");
                isListening = false;
                feedbackManager?.UpdateStatusIndicator(false, "Error activating");
            }
        } else {
             string reason = (isListening ? "isListening=true; " : "") + (appVoiceExperience.Active ? "appVoiceExperience.Active=true; " : "");
             if(verboseDiagnostics) Debug.LogWarning($"<color=orange>Condition NOT Met for Activation: {reason}Skipping Activate call.</color>");
             if (appVoiceExperience.Active && !isListening) {
                 if(verboseDiagnostics) Debug.LogWarning("<color=orange>Service is active but flag is false. Relying on OnStartListening event.</color>");
             }
        }
    }
    public void StopListening()
    {
         if (appVoiceExperience == null) return;
         if (isListening || appVoiceExperience.Active) {
             if(verboseDiagnostics) Debug.Log("Deactivating voice service...");
             try { appVoiceExperience.Deactivate(); }
             catch (Exception e) {
                Debug.LogError($"Error deactivating voice service: {e.Message}");
                isListening = false; // Force state update on error
                feedbackManager?.UpdateStatusIndicator(false, "Error stopping");
             }
         } else {
              if(verboseDiagnostics) Debug.Log("StopListening called but service was not active/listening.");
         }
    }

    private void HandleLowConfidence()
    {
        retryCount++;
        Debug.Log($"Low confidence or empty transcript. Retry count: {retryCount}/{MAX_RETRIES}");
        if (retryCount >= MAX_RETRIES) {
            feedbackManager?.PlayErrorFeedback("I'm still having trouble understanding.");
            SuggestAlternativeCommands();
            retryCount = 0;
        } else {
            feedbackManager?.PlayErrorFeedback("I didn't quite catch that. Could you repeat?");
            // Consider if we should auto-restart listening here
             if (sessionController != null && sessionController.GetCurrentState() == SessionState.Active && !isListening) {
                 StartCoroutine(RestartListeningAfterDelay(0.5f)); // Quick restart after asking to repeat
             }
        }
    }

    private IEnumerator RestartListeningAfterDelay(float delay)
    {
        if(verboseDiagnostics) Debug.Log($"<color=#00FFFF>RestartListeningAfterDelay: Coroutine started. Waiting for {delay}s...</color>");
        yield return new WaitForSeconds(delay);
        if(verboseDiagnostics) Debug.Log("<color=#00FFFF>RestartListeningAfterDelay: Wait finished.</color>");

        SessionState currentState = sessionController != null ? sessionController.GetCurrentState() : SessionState.Idle;
        if(verboseDiagnostics) Debug.Log($"<color=#00FFFF>RestartListeningAfterDelay: Checking session state. Current State = {currentState}</color>");

        if (currentState == SessionState.Active) {
            if(verboseDiagnostics) Debug.Log($"<color=green>RestartListeningAfterDelay: Session is Active. Calling StartListening()...</color>");
            StartListening();
        } else {
            if(verboseDiagnostics) Debug.LogWarning($"<color=orange>RestartListeningAfterDelay: Session not active (State={currentState}). Not restarting listener.</color>");
        }
    }

    private Color ParseColor(string colorName)
    {
        if (string.IsNullOrEmpty(colorName)) return Color.white;
        switch (colorName.ToLower()) {
            case "red": return Color.red;
            case "blue": return Color.blue;
            case "green": return Color.green;
            case "yellow": return Color.yellow;
            case "white": return Color.white;
            case "black": return Color.black;
            case "cyan": return Color.cyan;
            case "magenta": return Color.magenta;
            case "gray": case "grey": return Color.gray;
            case "purple": return new Color(0.5f, 0f, 0.5f);
            case "orange": return new Color(1.0f, 0.65f, 0f);
            case "pink": return new Color(1.0f, 0.75f, 0.8f);
            case "warm white": case "warm": return new Color(1.0f, 0.9f, 0.8f);
            case "cool white": case "cool": return new Color(0.8f, 0.9f, 1.0f);
            default: Debug.LogWarning($"ParseColor: Unrecognized color name '{colorName}'. Returning white."); return Color.white;
        }
    }

    private void SuggestAlternativeCommands()
    {
        if (sessionController == null || feedbackManager == null) return;
        SessionState currentState = sessionController.GetCurrentState();
        switch (currentState) {
            case SessionState.Idle: feedbackManager.ShowSuggestion("Try saying: \"Start therapy\""); break;
            case SessionState.Active: feedbackManager.ShowSuggestion("Try saying: \"Next step\" or \"Continue\""); break;
            case SessionState.Complete: feedbackManager.ShowSuggestion("Session complete. Say \"Start therapy\" to begin again."); break;
        }
    }

    private float CalculateSimilarity(string s, string t) {
        int distance = LevenshteinDistance(s, t);
        float maxLength = Math.Max(s.Length, t.Length);
        if (maxLength == 0) return 1.0f;
        return 1.0f - (distance / maxLength);
    }

    private int LevenshteinDistance(string s, string t) {
        if (string.IsNullOrEmpty(s)) return string.IsNullOrEmpty(t) ? 0 : t.Length;
        if (string.IsNullOrEmpty(t)) return s.Length;
        int n = s.Length; int m = t.Length;
        int[,] d = new int[n + 1, m + 1];
        for (int i = 0; i <= n; d[i, 0] = i++) ;
        for (int j = 0; j <= m; d[0, j] = j++) ;
        for (int i = 1; i <= n; i++) {
            for (int j = 1; j <= m; j++) {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        } return d[n, m];
    }


    // --- Editor-Only Keyboard Fallbacks ---
    private void Update()
    {
        #if UNITY_EDITOR
        HandleEditorKeyboardInput();
        #endif
    }

    #if UNITY_EDITOR
    private void HandleEditorKeyboardInput() {
        if (Input.GetKeyDown(KeyCode.S)) { // Start Session
            Debug.Log("EDITOR: S key pressed - Starting session");
            sessionController?.StartSession();
            feedbackManager?.PlaySuccessFeedback("Session started (keyboard)");
            StartListening();
        } else if (Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown(KeyCode.C)) { // Next/Continue
             Debug.Log("EDITOR: N/C key pressed - Next step");
             sessionController?.AdvanceToNextStep();
             feedbackManager?.PlaySuccessFeedback("Next step (keyboard)");
             StartListening();
        } else if (Input.GetKeyDown(KeyCode.E)) { // End Session
            Debug.Log("EDITOR: E key pressed - Ending session");
            sessionController?.EndSession();
            feedbackManager?.PlaySuccessFeedback("Session ended (keyboard)");
            // StopListening(); // Optional
        } else if (Input.GetKeyDown(KeyCode.D)) { // Debug Status / Restart Listener
            Debug.Log($"====== EDITOR: VOICE SYSTEM STATUS (D Key) ======");
            Debug.Log($"Session State: {sessionController?.GetCurrentState()}");
            Debug.Log($"Is Listening Flag: {isListening}");
            Debug.Log($"AppVoiceExperience Active: {appVoiceExperience?.Active}");
            Debug.Log($"AppVoiceExperience IsRequestActive: {appVoiceExperience?.IsRequestActive}");
            Debug.Log("Forcing voice listener restart...");
            StopListening();
            StartCoroutine(RestartListeningAfterDelay(0.2f));
        }
    }
    #endif // UNITY_EDITOR

} // End of class VoiceCommandManager