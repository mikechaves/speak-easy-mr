using UnityEngine;
using UnityEngine.Events; // Keep for potential UnityEvent usage if needed elsewhere
using Meta.WitAi; // Keep for WitResponseNode if OnResponse still uses it
using Meta.Voice; // Likely needed for AppVoiceExperience
using Oculus.Voice;
using Meta.WitAi.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Ensure AppVoiceExperience type is accessible. You might need to add:
// using Oculus.Voice; // If using older Oculus SDK integration namespace

public class VoiceCommandManager : MonoBehaviour
{
    [Header("Voice Service")]
    // Ensure this is assigned an AppVoiceExperience component in the Inspector
    [SerializeField] private VoiceService voiceService;

    [Tooltip("Check this to print additional diagnostic info")]
    [SerializeField] private bool verboseDiagnostics = true;
    [Tooltip("Set to lower value (0.5-0.7) if having trouble with command recognition")]
    [SerializeField] private float matchThreshold = 0.65f;

    [Header("References")]
    [SerializeField] private SessionController sessionController;
    [SerializeField] private FeedbackManager feedbackManager;

    [Header("Command Settings")]
    [SerializeField] private string[] startSessionCommands = { "start therapy", "begin session", "start", "begin", "therapy" };
    [SerializeField] private string[] nextStepCommands = { "next step", "continue", "next", "go on", "proceed", "forward", "advance", "cont", "move on", "go ahead", "keep going" };
    [SerializeField] private string[] endSessionCommands = { "end session", "stop therapy", "end", "stop", "finish", "exit", "quit" };

    // Common mistaken transcriptions and their correct mappings
    private readonly Dictionary<string, string> commonMistranscriptions = new Dictionary<string, string>
    {
        // "Continue" often gets transcribed as...
        { "continue you", "continue" }, { "continued", "continue" }, { "can to you", "continue" },
        { "container", "continue" }, { "continue on", "continue" }, { "continue new", "continue" },
        { "continuing", "continue" }, { "continu", "continue" }, { "contenyu", "continue" },
        { "can tea new", "continue" }, { "content", "continue" }, { "continual", "continue" },
        { "continue to", "continue" }, { "continue the", "continue" }, { "continue a", "continue" },
        { "continue please", "continue" }, { "continue now", "continue" }, { "continuous", "continue" },
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
        Debug.Log($"Voice service confirmed as AppVoiceExperience: {appVoiceExperience.name}");

        // --- Check Wit Configuration (Simplified) ---
        // Basic check if the config asset is assigned on the AppVoiceExperience
        // More detailed checks (like token) could be added if necessary, accessing appVoiceExperience.RuntimeConfiguration
        if (appVoiceExperience.RuntimeConfiguration == null || string.IsNullOrEmpty(appVoiceExperience.RuntimeConfiguration.witConfiguration?.GetClientAccessToken())) // Use function call ()
        {
             Debug.LogError("Wit configuration asset or client access token might be missing on AppVoiceExperience!");
             feedbackManager.UpdateStatusIndicator(false, "Wit config/token error");
             // Consider returning here if this is critical
        }
        else {
             Debug.Log("Found Wit configuration with token on AppVoiceExperience.");
        }

        // --- Setup Event Listeners ---
        SetupTranscriptionEvents();

        // --- Initial State ---
        feedbackManager.UpdateStatusIndicator(false, "Voice recognition ready");
        Debug.Log("VoiceCommandManager initialized.");

        // --- Automatic Activation ---
        Debug.Log("Attempting to automatically activate voice recognition on start.");
        // Consider using Voice Activity Detection (VAD) configured on the AppVoiceExperience component
        // in the Inspector for a more robust hands-free experience, which might negate the need for this call.
        StartListening();
    }

    private void SetupTranscriptionEvents()
    {
        if (appVoiceExperience == null) return; // Should have been caught in Start, but safety check

        try
        {
            Debug.Log("Adding event listeners to AppVoiceExperience.VoiceEvents");

            // Subscribe to the events provided by AppVoiceExperience
            // **IMPORTANT**: Ensure these method signatures match YOUR SDK version!
            appVoiceExperience.VoiceEvents.OnSend?.AddListener(OnSend); // Use OnSend
            appVoiceExperience.VoiceEvents.OnPartialTranscription?.AddListener(OnPartialTranscriptionReceived);
            appVoiceExperience.VoiceEvents.OnFullTranscription?.AddListener(OnFullTranscriptionReceived);
            appVoiceExperience.VoiceEvents.OnStartListening?.AddListener(OnStartListening);
            appVoiceExperience.VoiceEvents.OnStoppedListening?.AddListener(OnStoppedListening);
            appVoiceExperience.VoiceEvents.OnError?.AddListener(OnError); // Signature might be different, e.g., (string code, string message)
            appVoiceExperience.VoiceEvents.OnResponse?.AddListener(OnResponse); // Signature might be different, e.g., (WitResponseNode response)
            appVoiceExperience.VoiceEvents.OnAborted?.AddListener(OnAborted); // Signature likely has no parameters

            Debug.Log("Successfully added listeners (check handler signatures if issues arise).");
        }
        catch (Exception e)
        {
             Debug.LogError($"Error setting up voice events: {e.Message}\n{e.StackTrace}");
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from events to prevent errors when the object is destroyed
        if (appVoiceExperience != null && appVoiceExperience.VoiceEvents != null)
        {
            try
            {
                 Debug.Log("Removing event listeners from AppVoiceExperience.VoiceEvents");
                 appVoiceExperience.VoiceEvents.OnSend?.RemoveListener(OnSend); // Use OnSend
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
    // **IMPORTANT**: Verify the parameters match AppVoiceExperience.VoiceEvents in your SDK version!

    // Delete this entire method
    // private void OnRequestCreated(Meta.WitAi.Requests.VoiceServiceRequest request) {
    //     // Optional: Log or handle when a new request begins processing
    //     if(verboseDiagnostics) Debug.Log($"Voice Request Created: {request.Options?.RequestId}");
    // }

    // Handler for the OnSend event (replaces obsolete OnRequestCreated)
 // Verify signature if needed for your SDK version
    private void OnSend(Meta.WitAi.Requests.VoiceServiceRequest request)
    {
        // Optional: Log or handle when a request is sent to Wit.ai
        if(verboseDiagnostics) Debug.Log($"Voice Request Sent: {request?.Options?.RequestId}");
    }

    private void OnStartListening()
    {
        isListening = true;
        feedbackManager.UpdateStatusIndicator(true, "Listening...");
        if(verboseDiagnostics) Debug.Log("Event: Started listening for voice commands");
    }

    private void OnStoppedListening()
    {
        isListening = false;
        feedbackManager.UpdateStatusIndicator(false, "Processing...");
         if(verboseDiagnostics) Debug.Log("Event: Stopped listening for voice commands");
    }

     // Verify signature: Might be (string code, string message) or just (string error) etc.
    private void OnError(string code, string message)
    {
        Debug.LogError($"Event: Voice recognition error: {code} - {message}");
        isListening = false; // Ensure state is correct
        feedbackManager.UpdateStatusIndicator(false, "Error: " + message);
        feedbackManager.PlayErrorFeedback("Voice recognition error. Please try again.");

        // Optional: Auto-restart listening after an error if in an active session
        if (sessionController != null && sessionController.GetCurrentState() == SessionState.Active)
        {
            Debug.Log("In active session - attempting auto-restart after error");
            // Add a small delay before restarting
            StartCoroutine(RestartListeningAfterDelay(2.0f));
        }
    }

     // Verify signature: Does it still provide WitResponseNode or something else?
    private void OnResponse(WitResponseNode response)
    {
         if(verboseDiagnostics) Debug.Log("Event: Response received from voice service");
         if (response == null) {
             Debug.LogWarning("OnResponse received a null response node.");
             HandleLowConfidence();
             return;
         }

        try
        {
            if(verboseDiagnostics) Debug.Log($"FULL RESPONSE NODE: {response.ToString()}");

            // Fallback: If OnFullTranscription isn't reliable, try extracting from OnResponse
            string transcript = response["text"]?.Value; // Common location
            if (string.IsNullOrEmpty(transcript)) {
                transcript = response["transcript"]?.Value; // Alternative
            }
             if (string.IsNullOrEmpty(transcript)) {
                 // Check for intents if transcript is missing (like in original code)
                 if (response["intents"] != null && response["intents"].Count > 0)
                 {
                     string intentName = response["intents"][0]["name"]?.Value;
                     float intentConfidence = response["intents"][0]["confidence"]?.AsFloat ?? 0f;
                     Debug.Log($"Detected intent via OnResponse: {intentName} (confidence: {intentConfidence})");
                     // You might add intent-based handling here if needed
                 }
             }

            // If we got a transcript here AND OnFullTranscription isn't firing, process it.
            // Avoid double-processing if OnFullTranscription works.
            // For now, let's assume OnFullTranscription is the primary handler.
            // If OnFullTranscriptionReceived doesn't get called, uncomment the next line:
            // ProcessTranscript(transcript);

        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing response node: {e.Message}");
            HandleLowConfidence();
        }

        // Optional: Restart listening after processing if needed, handled by OnStoppedListening + auto-restart logic for now
    }


    // Updated OnFullTranscriptionReceived with explicit StopListening call before restart
    private void OnFullTranscriptionReceived(string transcript)
    {
        Debug.Log($"****** OnFullTranscriptionReceived FIRED with transcript: '{transcript}' ******");
        if(verboseDiagnostics) Debug.Log($"Event: Full transcription received: '{transcript}'");

        // Process the transcript and check if a command was actually handled
        bool commandProcessedSuccessfully = ProcessTranscript(transcript);

        SessionState stateAfterProcessing = sessionController != null ? sessionController.GetCurrentState() : SessionState.Idle;
        // Log state *before* potential StopListening call might change isListening flag
        Debug.Log($"<color=#FFBF00>OnFullTranscriptionReceived: State after processing='{stateAfterProcessing}', isListening flag (before check)='{isListening}'</color>");

        // --- Revised Restart Logic ---
        // If a command WAS successfully processed AND the session should continue (is Active),
        // ensure we stop first, then schedule a restart.
        if (commandProcessedSuccessfully && stateAfterProcessing == SessionState.Active)
        {
            Debug.Log("<color=magenta>Command processed & session active. Explicitly calling StopListening() before scheduling restart.</color>");
            StopListening(); // Ensure listener is stopped and isListening flag becomes false

            // Now that we are sure listening is stopped, schedule the restart
            Debug.Log("<color=green>Scheduling listener restart via coroutine...</color>");
            StartCoroutine(RestartListeningAfterDelay(0.5f)); // Short delay
        }
        else
        {
             // Log why we are not restarting
             if (!commandProcessedSuccessfully) Debug.LogWarning("<color=orange>OnFullTranscriptionReceived: Not restarting (command not processed successfully).</color>");
             else if (stateAfterProcessing != SessionState.Active) Debug.LogWarning($"<color=orange>OnFullTranscriptionReceived: Not restarting (session not Active, state={stateAfterProcessing}).</color>");
             // If command was processed but session ended, we don't restart.
        }
    }

    private void OnPartialTranscriptionReceived(string transcript)
    {
        if(verboseDiagnostics) Debug.Log($"Event: Partial transcription: '{transcript}'");
        // Inside VoiceCommandManager.cs, within OnPartialTranscriptReceived method
        // feedbackManager?.UpdatePartialTranscript(transcript); // Temporarily disabled
    }

    // Verify signature: Likely has no parameters
    private void OnAborted()
    {
         if(verboseDiagnostics) Debug.Log("Event: Voice recognition aborted");
         isListening = false; // Ensure state is correct
         feedbackManager.UpdateStatusIndicator(false, "Voice recognition ready");

         // Optional: Auto-restart listening after abort if in an active session
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
        if (string.IsNullOrEmpty(transcript))
        {
            if(verboseDiagnostics) Debug.LogWarning("ProcessTranscript called with empty transcript.");
            HandleLowConfidence();
            return false; // No command processed
        }

        string lowerTranscript = transcript.ToLower();
        SessionState currentState = sessionController.GetCurrentState();
        string correctedTranscript = ApplyTranscriptionCorrections(lowerTranscript);
        bool commandMatched = false; // Flag to track if we processed a command

        if (verboseDiagnostics && lowerTranscript != correctedTranscript) {
             Debug.Log($"Corrected transcript: '{lowerTranscript}' -> '{correctedTranscript}'");
        }

        // Log analysis details
        if (verboseDiagnostics) {
            Debug.Log("---COMMAND DETECTION ANALYSIS---");
            Debug.Log($"Original: '{transcript}'");
            Debug.Log($"Corrected: '{correctedTranscript}'");
            Debug.Log($"Current Session State: {currentState}");
        }

        // Prioritize based on state
        if (currentState == SessionState.Active && IsPotentialContinueCommand(correctedTranscript)) {
             if (verboseDiagnostics) Debug.Log($"PRIORITY CONTINUE COMMAND DETECTED: '{correctedTranscript}'");
             sessionController.AdvanceToNextStep();
             feedbackManager.PlaySuccessFeedback("Moving to next step");
             commandMatched = true; // Command processed
        }
        else if (currentState == SessionState.Idle && IsPotentialStartCommand(correctedTranscript)) {
             if (verboseDiagnostics) Debug.Log($"PRIORITY START COMMAND DETECTED: '{correctedTranscript}'");
             sessionController.StartSession();
             feedbackManager.PlaySuccessFeedback("Session started");
             commandMatched = true; // Command processed
        }
        else {
             // General matching if priority didn't hit
             if (ContainsAny(correctedTranscript, startSessionCommands)) {
                 Debug.Log($"COMMAND DETECTED: START SESSION - '{correctedTranscript}'");
                 sessionController.StartSession();
                 feedbackManager.PlaySuccessFeedback("Session started");
                 commandMatched = true; // Command processed
             }
             else if (ContainsAny(correctedTranscript, nextStepCommands)) {
                 Debug.Log($"COMMAND DETECTED: NEXT STEP - '{correctedTranscript}'");
                 sessionController.AdvanceToNextStep();
                 feedbackManager.PlaySuccessFeedback("Moving to next step");
                 commandMatched = true; // Command processed
             }
             else if (ContainsAny(correctedTranscript, endSessionCommands)) {
                 Debug.Log($"COMMAND DETECTED: END SESSION - '{correctedTranscript}'");
                 sessionController.EndSession();
                 feedbackManager.PlaySuccessFeedback("Session completed");
                 commandMatched = true; // Command processed
             }
             // Last resort matching for common words during active session
             else if (currentState == SessionState.Active && IsLastResortContinueWord(correctedTranscript))
             {
                 Debug.Log($"LAST RESORT CONTINUE MATCH: '{correctedTranscript}'");
                 sessionController.AdvanceToNextStep();
                 feedbackManager.PlaySuccessFeedback("Moving to next step");
                 commandMatched = true; // Command processed
             }
        }

        // Reset retry count only if a command was matched
        if (commandMatched) {
            retryCount = 0;
        }

        if (!commandMatched)
        {
            Debug.Log($"NO COMMAND MATCH: '{correctedTranscript}'");
            feedbackManager.PlayErrorFeedback("I didn't understand that command");
            SuggestAlternativeCommands();
            // HandleLowConfidence(); // Decide if no match should trigger retry logic
        }

        return commandMatched; // Return true if we matched and processed a command
    }

    // Helper for priority check
    private bool IsPotentialContinueCommand(string correctedTranscript) {
        return correctedTranscript.Contains("cont") || correctedTranscript.Contains("next") ||
               correctedTranscript.Contains("go") || correctedTranscript.Contains("step") ||
               correctedTranscript.Contains("move") || correctedTranscript.Contains("forward") ||
               correctedTranscript.Contains("proceed");
    }

    // Helper for priority check
    private bool IsPotentialStartCommand(string correctedTranscript) {
        return correctedTranscript.Contains("start") || correctedTranscript.Contains("begin") ||
               correctedTranscript.Contains("therapy");
    }

     // Helper for last resort check
    private bool IsLastResortContinueWord(string correctedTranscript) {
        string[] words = correctedTranscript.Split(new char[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string word in words)
        {
            if (word.Equals("yes") || word.Equals("yeah") || word.Equals("ok") ||
                word.Equals("okay") || word.Equals("fine") || word.Equals("sure") ||
                word.Equals("alright") || word.Equals("continue") || word.Equals("next") ||
                word.StartsWith("cont") || word.StartsWith("next") || word.StartsWith("mov"))
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
        bool correctionApplied = false;

        // Check exact mistranscription dictionary
        foreach (var kvp in commonMistranscriptions) {
            if (lowerTranscript.Contains(kvp.Key)) {
                lowerTranscript = lowerTranscript.Replace(kvp.Key, kvp.Value);
                correctionApplied = true;
                if (verboseDiagnostics) Debug.Log($"Applied correction: '{kvp.Key}' -> '{kvp.Value}'");
            }
        }

        // Aggressive fuzzy matching for "continue" / "next" if no dictionary match found
        if (!correctionApplied && (lowerTranscript.Contains("con") || lowerTranscript.Contains("can") || lowerTranscript.Contains("tin") || lowerTranscript.Contains("nex")))
        {
            float continueScore = CalculateSimilarity(lowerTranscript, "continue");
            float nextScore = CalculateSimilarity(lowerTranscript, "next");

            if (continueScore > 0.4f) {
                 if (verboseDiagnostics) Debug.Log($"Special continue correction applied (score {continueScore:F2}) to: '{transcript}' -> 'continue'");
                 return "continue"; // Return corrected directly
            } else if (nextScore > 0.4f) {
                 if (verboseDiagnostics) Debug.Log($"Special next correction applied (score {nextScore:F2}) to: '{transcript}' -> 'next'");
                 return "next"; // Return corrected directly
            }
        }

        return lowerTranscript; // Return potentially corrected transcript
    }

    private bool ContainsAny(string source, string[] keywords)
    {
        if (string.IsNullOrEmpty(source) || keywords == null || keywords.Length == 0) {
            return false;
        }

        // Prepare source: lowercase, no punctuation, padded with spaces for word boundary checks
        source = " " + source.ToLower().Replace(".", "").Replace(",", "").Replace("!", "").Replace("?", "") + " ";

        foreach (string keyword in keywords)
        {
            if (string.IsNullOrEmpty(keyword)) continue;

            string lowerKeyword = keyword.ToLower();
            string spacedKeyword = " " + lowerKeyword + " ";

            // Check for whole word match using padding
            if (source.Contains(spacedKeyword)) {
                return true;
            }

            // Simple contains check (less precise, might match substrings)
            // if (source.Contains(lowerKeyword)) {
            //     return true;
            // }

             // Optional: Fuzzy matching using Levenshtein (can be slow if used heavily)
            /*
             string[] words = source.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
             foreach (string word in words)
             {
                 if (word.Length > 3 && lowerKeyword.Length > 3) {
                     float similarity = CalculateSimilarity(word, lowerKeyword);
                     if (similarity >= matchThreshold) {
                         if (verboseDiagnostics) Debug.Log($"Fuzzy match: '{word}' similar to '{lowerKeyword}' (sim: {similarity:F2})");
                         return true;
                     }
                 }
             }
            */
        }
        return false;
    }


    // --- Utility & Helper Methods ---

    public void StartListening()
    {
        if (appVoiceExperience == null) {
            Debug.LogError("Cannot start listening, AppVoiceExperience is not set.");
            return;
        }

        // ADDED LOGS: Check state *before* the main condition
        // Use Rich Text tags for emphasis
        Debug.Log($"<color=yellow>StartListening Check: Current isListening flag = {isListening}</color>");
        bool isActive = false;
        try {
            isActive = appVoiceExperience.Active; // Get the state safely
        } catch (Exception e) {
            Debug.LogError($"<color=red>StartListening Check: Error accessing appVoiceExperience.Active: {e.Message}</color>");
        }
        Debug.Log($"<color=yellow>StartListening Check: Current appVoiceExperience.Active = {isActive}</color>");

        // Don't try to activate if already listening or if service reports active
        if (!isListening && !isActive) // Use the safely obtained 'isActive' value
        {
            Debug.Log("<color=green>Condition Met: Attempting to Activate voice service...</color>");
            try
            {
                // ADDED LOGS: Check token access result more carefully
                var runtimeConfig = appVoiceExperience.RuntimeConfiguration;
                var witConfig = runtimeConfig?.witConfiguration;
                string token = null; // Default to null
                if (witConfig != null) {
                    try {
                        token = witConfig.GetClientAccessToken();
                    } catch (Exception e) {
                        Debug.LogError($"<color=red>Token Check: Error calling GetClientAccessToken(): {e.Message}</color>");
                    }
                }
                Debug.Log($"<color=cyan>Token Check: runtimeConfig null? {runtimeConfig == null}</color>");
                Debug.Log($"<color=cyan>Token Check: witConfig null? {witConfig == null}</color>");
                bool isTokenNullOrEmpty = string.IsNullOrEmpty(token);
                Debug.Log($"<color=cyan>Token Check: token is null or empty? {isTokenNullOrEmpty}</color>");

                // Ensure configuration is still valid (basic check)
                if (runtimeConfig == null || isTokenNullOrEmpty) {
                    Debug.LogError("<color=red>Cannot Activate: Wit configuration or token missing/invalid.</color>");
                    feedbackManager?.UpdateStatusIndicator(false, "Config/Token Error");
                    return; // Exit if token is invalid
                }

                Debug.Log("<color=lime>Calling appVoiceExperience.Activate()...</color>");
                appVoiceExperience.Activate();
                // OnStartListening event should update isListening and UI
            }
            catch (Exception e)
            {
                Debug.LogError($"<color=red>Error during appVoiceExperience.Activate(): {e.Message}\n{e.StackTrace}</color>"); // Added StackTrace
                isListening = false; // Ensure state is false on error
                feedbackManager?.UpdateStatusIndicator(false, "Error activating");
            }
        } else {
            // Log why the condition failed
            string reason = "";
            if (isListening) reason += "isListening=true; ";
            if (isActive) reason += "appVoiceExperience.Active=true; ";
            Debug.LogWarning($"<color=orange>Condition NOT Met for Activation: {reason}Skipping Activate call.</color>");
            // If already active, maybe just ensure our flag matches?
            if (isActive && !isListening) {
                Debug.LogWarning("<color=orange>Service is active but flag is false. Updating flag via OnStartListening event hopefully.</color>");
                // Optionally force flag sync, but event *should* handle this if it fires
                // isListening = true;
                // feedbackManager?.UpdateStatusIndicator(true, "Listening (already active)");
            }
        }
    }
    public void StopListening()
    {
         if (appVoiceExperience == null) return;

        // Only deactivate if we think we are listening or if service reports active
        if (isListening || appVoiceExperience.Active)
        {
             if(verboseDiagnostics) Debug.Log("Deactivating voice service...");
             try {
                appVoiceExperience.Deactivate();
                // OnStoppedListening event should update isListening and UI
             } catch (Exception e) {
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
            feedbackManager.PlayErrorFeedback("I'm still having trouble understanding. Please check commands or try again.");
            SuggestAlternativeCommands();
            retryCount = 0; // Reset after max retries
        } else {
            feedbackManager.PlayErrorFeedback("I'm not sure I understood. Could you repeat that?");
            // Don't automatically StartListening here, let the error/abort handlers manage restart if needed
        }
    }

    private IEnumerator RestartListeningAfterDelay(float delay)
    {
        Debug.Log($"<color=#00FFFF>RestartListeningAfterDelay: Coroutine started. Waiting for {delay}s...</color>"); // Changed Color
        yield return new WaitForSeconds(delay);
        Debug.Log("<color=#00FFFF>RestartListeningAfterDelay: Wait finished.</color>"); // Changed Color

        // Check if we should restart (e.g., still in active session)
        SessionState currentState = sessionController != null ? sessionController.GetCurrentState() : SessionState.Idle; // Get state *now*
        Debug.Log($"<color=#00FFFF>RestartListeningAfterDelay: Checking session state. Current State = {currentState}</color>"); // ADDED

        if (currentState == SessionState.Active) {
            Debug.Log($"<color=green>RestartListeningAfterDelay: Session is Active. Calling StartListening()...</color>"); // Changed Color
            StartListening();
        } else {
            Debug.LogWarning($"<color=orange>RestartListeningAfterDelay: Session not active (State={currentState}). Not restarting listener.</color>"); // Changed Color
        }
    }


    private void SuggestAlternativeCommands()
    {
        SessionState currentState = sessionController.GetCurrentState();
        switch (currentState) {
            case SessionState.Idle:
                feedbackManager.ShowSuggestion("Try saying: \"Start therapy\"");
                break;
            case SessionState.Active:
                feedbackManager.ShowSuggestion("Try saying: \"Next step\" or \"Continue\"");
                break;
            case SessionState.Complete:
                feedbackManager.ShowSuggestion("Session complete. Say \"Start therapy\" to begin again.");
                break;
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

        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        for (int i = 0; i <= n; d[i, 0] = i++) ;
        for (int j = 0; j <= m; d[0, j] = j++) ;

        for (int i = 1; i <= n; i++) {
            for (int j = 1; j <= m; j++) {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m];
    }


    // --- Editor-Only Keyboard Fallbacks ---
    private void Update()
    {
        #if UNITY_EDITOR
        HandleEditorKeyboardInput();
        #endif

        // Removed auto-reactivate timer logic - rely on Activate on Start / VAD / specific event restarts
    }

    #if UNITY_EDITOR
    private void HandleEditorKeyboardInput() {
        // S key for Start Session
        if (Input.GetKeyDown(KeyCode.S)) {
            Debug.Log("EDITOR: S key pressed - Starting session");
            sessionController?.StartSession();
            feedbackManager?.PlaySuccessFeedback("Session started (keyboard)");
            StartListening(); // Restart listening after command
        }
        // N key for Next Step
        else if (Input.GetKeyDown(KeyCode.N)) {
            Debug.Log("EDITOR: N key pressed - Next step");
            sessionController?.AdvanceToNextStep();
            feedbackManager?.PlaySuccessFeedback("Next step (keyboard)");
            StartListening(); // Restart listening after command
        }
        // C key also for Continue/Next
        else if (Input.GetKeyDown(KeyCode.C)) {
             Debug.Log("EDITOR: C key pressed - Continue/Next step");
             sessionController?.AdvanceToNextStep();
             feedbackManager?.PlaySuccessFeedback("Next step (keyboard)");
             StartListening(); // Restart listening after command
        }
        // E key for End Session
        else if (Input.GetKeyDown(KeyCode.E)) {
            Debug.Log("EDITOR: E key pressed - Ending session");
            sessionController?.EndSession();
            feedbackManager?.PlaySuccessFeedback("Session ended (keyboard)");
            // Optionally stop listening after ending
            // StopListening();
        }
         // D key for Debug Status / Force Restart Listener
        else if (Input.GetKeyDown(KeyCode.D)) {
            Debug.Log($"====== EDITOR: VOICE SYSTEM STATUS (D Key) ======");
            Debug.Log($"Session State: {sessionController?.GetCurrentState()}");
            Debug.Log($"Is Listening Flag: {isListening}");
            Debug.Log($"AppVoiceExperience Active: {appVoiceExperience?.Active}");
            Debug.Log($"AppVoiceExperience IsRequestActive: {appVoiceExperience?.IsRequestActive}");
            Debug.Log("Forcing voice listener restart...");
            StopListening();
             // Add a small delay before starting again if needed
            StartCoroutine(RestartListeningAfterDelay(0.2f));
        }
    }
    #endif // UNITY_EDITOR

} // End of class VoiceCommandManager