using UnityEngine;
using UnityEngine.Events;
using Meta.WitAi;
using Meta.WitAi.Json;
using Meta.Voice;
using Oculus.Voice;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

// <<< --- MOVED HELPER CLASS OUTSIDE --- >>>
/// <summary>
/// Helper class for serializable dictionary. Implements ISerializationCallbackReceiver
/// to allow Unity to serialize/deserialize the dictionary content via lists.
/// </summary>
[System.Serializable]
public class StringStringDictionary : Dictionary<string, string>, ISerializationCallbackReceiver
{
    [SerializeField] private List<string> _keys = new List<string>();
    [SerializeField] private List<string> _values = new List<string>();

    // Saves the dictionary to lists before serialization.
    public void OnBeforeSerialize()
    {
        _keys.Clear();
        _values.Clear();
        foreach (KeyValuePair<string, string> pair in this)
        {
            _keys.Add(pair.Key);
            _values.Add(pair.Value);
        }
    }

    // Loads the dictionary from lists after deserialization.
    public void OnAfterDeserialize()
    {
        this.Clear();
        if (_keys.Count != _values.Count)
        {
            Debug.LogError("Serialization error in StringStringDictionary: Keys and Values count mismatch. Dictionary will be empty.");
            return;
        }
        for (int i = 0; i < _keys.Count; i++)
        {
             if (_keys[i] != null) {
                this.Add(_keys[i], _values[i]);
             } else {
                 Debug.LogWarning($"StringStringDictionary: Skipped adding null key at index {i} during deserialization.");
             }
        }
    }
}
// <<< --- END MOVED HELPER CLASS --- >>>


/// <summary>
/// Manages voice command input. Uses a continuous listening approach (auto-restarts)
/// combined with a wake word ("Okay") detected by a Response Matcher component.
/// Only processes transcriptions as commands if the wake word was detected immediately prior.
/// </summary>
public class VoiceCommandManager : MonoBehaviour // <<< Main class starts here
{
    [Header("Voice Service")]
    [Tooltip("Assign the AppVoiceExperience component from your scene.")]
    [SerializeField] private VoiceService voiceService;

    [Tooltip("Check this to print detailed diagnostic information to the console.")]
    [SerializeField] private bool verboseDiagnostics = true;

    [Header("References")]
    [Tooltip("Reference to the SessionController for managing therapy flow.")]
    [SerializeField] private SessionController sessionController;
    [Tooltip("Reference to the FeedbackManager for providing visual/audio feedback.")]
    [SerializeField] private FeedbackManager feedbackManager;
    [Tooltip("Reference to the VisualizationEnvironment for handling light modification commands.")]
    [SerializeField] private VisualizationEnvironment visualizationEnvironment;

    [Header("Command Settings")]
    [SerializeField] private string[] startSessionCommands = { "start therapy", "begin session", "start", "begin", "therapy", "ready" };
    [SerializeField] private string[] nextStepCommands = { "next step", "continue", "next", "go on", "proceed", "forward", "advance", "cont", "move on", "go ahead", "keep going", "okay", "ok" };
    [SerializeField] private string[] endSessionCommands = { "end session", "stop therapy", "end", "stop", "finish", "exit", "quit" };

    [Header("Transcription Correction")]
    [Tooltip("Common Wit.ai transcription errors and their desired corrections.")]
    [SerializeField] private StringStringDictionary commonMistranscriptions = new StringStringDictionary {
        { "next up", "next" }, { "text", "next" }, { "nest", "next" }, { "necks", "next" },
        { "next to", "next" }, { "next please", "next" }, { "next one", "next" },
        { "next time", "next" }, { "next day", "next" },
        { "began", "begin" }, { "beginning", "begin" }, { "begins", "begin" },
        { "started", "start" }, { "starting", "start" }, { "starts", "start" },
        { "and", "end" }, { "ending", "end" }, { "ends", "end" }
    };

    // Internal State
    private bool isListening = false;
    private AppVoiceExperience appVoiceExperience;
    private string transcriptFromResponse = null;
    private bool isWaitingForCommand = false;


    private void Start()
    {
        if (!PerformChecks()) { enabled = false; return; }
        SetupTranscriptionEvents();
        feedbackManager.UpdateStatusIndicator(false, "Voice recognition ready");
        Debug.Log("VoiceCommandManager initialized.", this);
        Debug.Log("Activating voice recognition initially.", this);
        this.StartListening(); // Use 'this.' qualifier
    }

    private bool PerformChecks() {
        bool checksPassed = true;
        if (feedbackManager == null) { Debug.LogError("Feedback Manager is not assigned!", this); checksPassed = false; }
        if (sessionController == null) { Debug.LogError("Session Controller is not assigned!", this); feedbackManager?.UpdateStatusIndicator(false, "Session controller missing"); checksPassed = false; }
        if (voiceService == null) { Debug.LogError("Voice Service is not assigned!", this); feedbackManager?.UpdateStatusIndicator(false, "Voice service missing"); checksPassed = false; }
        if (visualizationEnvironment == null) { Debug.LogWarning("VisualizationEnvironment is not assigned. Light modification commands will not work.", this); }

        appVoiceExperience = voiceService as AppVoiceExperience;
        if (appVoiceExperience == null) {
            Debug.LogError($"Assigned VoiceService is NOT an AppVoiceExperience. Found type: {voiceService.GetType().Name}. Cannot proceed.", this);
            feedbackManager?.UpdateStatusIndicator(false, "Incorrect voice service type"); checksPassed = false;
         } else if(verboseDiagnostics) { Debug.Log($"Voice service confirmed as AppVoiceExperience: {appVoiceExperience.name}", this); }

        bool configValid = appVoiceExperience?.RuntimeConfiguration?.witConfiguration != null &&
                           !string.IsNullOrEmpty(appVoiceExperience.RuntimeConfiguration.witConfiguration.GetClientAccessToken());
        if (!configValid) {
             Debug.LogError("Wit configuration asset or client access token might be missing/invalid on AppVoiceExperience!", this);
             feedbackManager?.UpdateStatusIndicator(false, "Wit config/token error");
        } else { if(verboseDiagnostics) Debug.Log("Found Wit configuration with token on AppVoiceExperience.", this); }

        return checksPassed;
    }

    private void SetupTranscriptionEvents() {
        if (appVoiceExperience == null || appVoiceExperience.VoiceEvents == null) { Debug.LogError("Cannot setup events - AppVoiceExperience or its VoiceEvents are null!", this); return; }
        try {
            if(verboseDiagnostics) Debug.Log("Adding event listeners to AppVoiceExperience.VoiceEvents", this);
            appVoiceExperience.VoiceEvents.OnSend?.AddListener(OnSend);
            appVoiceExperience.VoiceEvents.OnPartialTranscription?.AddListener(OnPartialTranscriptionReceived);
            appVoiceExperience.VoiceEvents.OnFullTranscription?.AddListener(OnFullTranscriptionReceived);
            appVoiceExperience.VoiceEvents.OnStartListening?.AddListener(OnStartListening);
            appVoiceExperience.VoiceEvents.OnStoppedListening?.AddListener(OnStoppedListening);
            appVoiceExperience.VoiceEvents.OnError?.AddListener(OnError);
            appVoiceExperience.VoiceEvents.OnResponse?.AddListener(OnResponse);
            appVoiceExperience.VoiceEvents.OnAborted?.AddListener(OnAborted);
            if(verboseDiagnostics) Debug.Log("Successfully added listeners.", this);
        } catch (Exception e) { Debug.LogError($"Error setting up voice events: {e.Message}\n{e.StackTrace}", this); }
     }

    private void OnDisable() {
        if (appVoiceExperience != null && appVoiceExperience.VoiceEvents != null) {
            try {
                if(verboseDiagnostics) Debug.Log("Removing event listeners from AppVoiceExperience.VoiceEvents", this);
                appVoiceExperience.VoiceEvents.OnSend?.RemoveListener(OnSend);
                appVoiceExperience.VoiceEvents.OnPartialTranscription?.RemoveListener(OnPartialTranscriptionReceived);
                appVoiceExperience.VoiceEvents.OnFullTranscription?.RemoveListener(OnFullTranscriptionReceived);
                appVoiceExperience.VoiceEvents.OnStartListening?.RemoveListener(OnStartListening);
                appVoiceExperience.VoiceEvents.OnStoppedListening?.RemoveListener(OnStoppedListening);
                appVoiceExperience.VoiceEvents.OnError?.RemoveListener(OnError);
                appVoiceExperience.VoiceEvents.OnResponse?.RemoveListener(OnResponse);
                appVoiceExperience.VoiceEvents.OnAborted?.RemoveListener(OnAborted);
            } catch (Exception e) { Debug.LogWarning($"Error removing voice event listeners: {e.Message}", this); }
        }
        StopAllCoroutines(); // Stop coroutines on disable
    }

    // --- Event Handlers ---

    private void OnSend(Meta.WitAi.Requests.VoiceServiceRequest request) {
        if(verboseDiagnostics) Debug.Log($"Voice Request Sent: ID={request?.Options?.RequestId}", this);
    }

    private void OnStartListening() {
         isListening = true;
         feedbackManager?.UpdateStatusIndicator(true, "Listening...");
         if(verboseDiagnostics) Debug.Log("Event: Listener started", this);
         transcriptFromResponse = null;
    }

    private void OnStoppedListening() {
        isListening = false;
        if(verboseDiagnostics) Debug.Log("Event: Listener stopped", this);
        feedbackManager?.UpdateStatusIndicator(false, "Restarting listener...");

        // Auto-Restart Logic
        if (sessionController != null && sessionController.GetCurrentState() != SessionState.Complete && this.enabled && gameObject.activeInHierarchy) {
            StartCoroutine(this.ReactivateListenerAfterDelay(0.1f, "OnStoppedListening"));
        } else {
             if(verboseDiagnostics) Debug.Log("OnStoppedListening: Session complete or controller null or component disabled. Not restarting listener.", this);
        }
    }

    private void OnError(string code, string message) {
        Debug.LogError($"Event: Voice recognition error: Code='{code}', Message='{message}'", this);
        isListening = false;
        feedbackManager?.UpdateStatusIndicator(false, "Error: " + message);
        feedbackManager?.PlayErrorFeedback("Voice recognition error.");
        isWaitingForCommand = false;
        // Auto-restart handled by OnStoppedListening
    }

     private void OnAborted() {
        if(verboseDiagnostics) Debug.Log("Event: Voice recognition aborted", this);
        isListening = false;
        feedbackManager?.UpdateStatusIndicator(false, "Voice recognition aborted");
        isWaitingForCommand = false;
        // Auto-restart handled by OnStoppedListening
    }

    private void OnResponse(WitResponseNode response) {
        if (response == null) { Debug.LogWarning("OnResponse called with null response node.", this); return; }
        if(verboseDiagnostics) { Debug.Log($"<color=yellow>====== RAW WIT.AI OnResponse DATA ======\n{response.ToString()}\n====================================</color>", this); }

        bool processedIntentInResponse = false;
        transcriptFromResponse = null;

        // Try to Extract Transcript
        string transcript = response["text"]?.Value ?? response["_text"]?.Value;
        if (string.IsNullOrEmpty(transcript)) {
            try {
                string entityKey = "therapy_command:therapy_command";
                transcript = response?["entities"]?[entityKey]?[0]?["body"]?.Value;
                if (!string.IsNullOrEmpty(transcript) && verboseDiagnostics) { Debug.Log($"OnResponse: Transcript extracted from entity '{entityKey}': '{transcript}'", this); }
            } catch (Exception e) { Debug.LogWarning($"OnResponse: Error trying to extract transcript from entity: {e.Message}", this); }
        }
        if (!string.IsNullOrEmpty(transcript)) { transcriptFromResponse = transcript.Trim(); }
        else { if(verboseDiagnostics) Debug.Log("OnResponse: Transcript not found.", this); }

        // Intent Processing (e.g., modify_light)
        int visualizationStepIndex = 2;
        bool isVisualizationStepActive = sessionController?.GetCurrentState() == SessionState.Active && sessionController?.GetCurrentStepIndex() == visualizationStepIndex;
        if (isVisualizationStepActive && visualizationEnvironment != null) {
            string intentName = null; float intentConfidence = 0f;
            try {
                if (response["intents"] != null && response["intents"].Count > 0) {
                    intentName = response["intents"][0]["name"]?.Value;
                    intentConfidence = response["intents"][0]["confidence"]?.AsFloat ?? 0f;
                }
            } catch (Exception e) { Debug.LogWarning($"Error accessing intents: {e.Message}", this); }

            if (!string.IsNullOrEmpty(intentName) && intentName.Equals("modify_light", StringComparison.OrdinalIgnoreCase) && intentConfidence > 0.7f) {
                bool actionTaken = this.HandleModifyLightIntent(response); // Use 'this.'
                if (actionTaken) { AudioManager.Instance?.PlayConfirmationSound(); processedIntentInResponse = true; }
                 processedIntentInResponse = true;
                 isWaitingForCommand = false;
            }
        }

        if (!processedIntentInResponse && verboseDiagnostics) { Debug.Log("OnResponse: No specific intent processed. Relying on OnFullTranscriptionReceived.", this); }
    }

    private bool HandleModifyLightIntent(WitResponseNode response) {
        if (visualizationEnvironment == null || response == null) return false;
        bool actionTaken = false;
        string colorEntityKey = "light_color:light_color";
        string intensityEntityKey = "intensity_direction:intensity_direction";
        try {
            string colorName = response?["entities"]?[colorEntityKey]?[0]?["value"]?.Value;
            if (!string.IsNullOrEmpty(colorName)) {
                if(verboseDiagnostics) Debug.Log($"Extracted color entity value: {colorName}");
                Color targetColor = this.ParseColor(colorName); // Use 'this.'
                visualizationEnvironment.SetLightColor(targetColor);
                feedbackManager?.PlaySuccessFeedback($"Light color set to {colorName}");
                actionTaken = true;
            }
            string direction = response?["entities"]?[intensityEntityKey]?[0]?["value"]?.Value;
            if (!string.IsNullOrEmpty(direction)) {
                if(verboseDiagnostics) Debug.Log($"Extracted intensity direction entity value: {direction}");
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
        } catch (Exception e) { Debug.LogWarning($"Error processing modify_light entities/actions via JSON indexing: {e.Message}", this); }
        return actionTaken;
     }

    private void OnFullTranscriptionReceived(string transcript) {
        Debug.Log($"****** OnFullTranscriptionReceived FIRED with transcript: '{transcript}' ******", this);
        if(verboseDiagnostics) Debug.Log($"Event: Full transcription received: '{transcript}'", this);

        feedbackManager?.UpdateStatusIndicator(false, "Processing complete");

        string transcriptToProcess = transcript;
        if (string.IsNullOrEmpty(transcriptToProcess) && !string.IsNullOrEmpty(transcriptFromResponse)) {
            Debug.LogWarning("OnFullTranscriptionReceived: Received empty transcript, using stored transcript from OnResponse.", this);
            transcriptToProcess = transcriptFromResponse;
        }
        transcriptFromResponse = null;

        if (isWaitingForCommand) {
            Debug.Log("<color=purple>OnFullTranscriptionReceived: Wake word flag is TRUE. Processing transcript as command.</color>", this);
            isWaitingForCommand = false; // Consume the flag

            if (!string.IsNullOrEmpty(transcriptToProcess)) {
                bool commandProcessedSuccessfully = this.ProcessTranscript(transcriptToProcess); // Use 'this.'
                if (!commandProcessedSuccessfully) {
                    feedbackManager?.PlayErrorFeedback("I heard the wake word, but didn't understand the command.");
                    this.SuggestAlternativeCommands(); // Use 'this.'
                }
            } else {
                 Debug.LogWarning("OnFullTranscriptionReceived: Transcript is empty after wake word.", this);
                 feedbackManager?.PlayErrorFeedback("I heard the wake word, but didn't catch the command.");
                 this.SuggestAlternativeCommands(); // Use 'this.'
            }
        } else {
            if (!string.IsNullOrEmpty(transcriptToProcess) && verboseDiagnostics) {
                 Debug.Log($"<color=grey>OnFullTranscriptionReceived: Wake word flag is FALSE. Ignoring transcript: '{transcriptToProcess}'</color>", this);
            } else if (string.IsNullOrEmpty(transcriptToProcess) && verboseDiagnostics) {
                 Debug.Log("<color=grey>OnFullTranscriptionReceived: Empty transcript and wake word flag is FALSE.</color>", this);
            }
        }

        if (verboseDiagnostics) Debug.Log("OnFullTranscriptionReceived: Processing complete. Listener will deactivate and auto-restart.", this);
    }


    private void OnPartialTranscriptionReceived(string transcript) {
         if (isWaitingForCommand && feedbackManager != null) {
             if(verboseDiagnostics) Debug.Log($"Partial transcription (post-wake word): '{transcript}'");
         }
     }

    // --- Core Logic ---
    private bool ProcessTranscript(string transcript) {
        if (string.IsNullOrEmpty(transcript)) { return false; }
        string lowerTranscript = transcript.ToLower().Trim();
        string correctedTranscript = this.ApplyTranscriptionCorrections(lowerTranscript); // Use 'this.'
        if (string.IsNullOrEmpty(correctedTranscript)) { return false; }

        SessionState currentState = sessionController != null ? sessionController.GetCurrentState() : SessionState.Idle;
        bool commandMatched = false;

        if (verboseDiagnostics) { Debug.Log($"---COMMAND DETECTION: Corrected='{correctedTranscript}', State={currentState}---", this); }

        switch (currentState) {
            case SessionState.Idle:
                if (this.ContainsAny(correctedTranscript, startSessionCommands)) { // Use 'this.'
                    Debug.Log($"COMMAND DETECTED: START SESSION - '{correctedTranscript}'", this);
                    sessionController?.StartSession();
                    feedbackManager?.PlaySuccessFeedback("Session started");
                    AudioManager.Instance?.PlayConfirmationSound();
                    commandMatched = true;
                }
                break;
            case SessionState.Active:
                if (this.ContainsAny(correctedTranscript, endSessionCommands)) { // Use 'this.'
                    Debug.Log($"COMMAND DETECTED: END SESSION - '{correctedTranscript}'", this);
                    sessionController?.EndSession();
                    AudioManager.Instance?.PlayConfirmationSound();
                    commandMatched = true;
                }
                else if (this.ContainsAny(correctedTranscript, nextStepCommands)) { // Use 'this.'
                    Debug.Log($"COMMAND DETECTED: NEXT STEP - '{correctedTranscript}'", this);
                    sessionController?.AdvanceToNextStep();
                    AudioManager.Instance?.PlayConfirmationSound();
                    commandMatched = true;
                }
                break;
            case SessionState.Complete:
                 if (this.ContainsAny(correctedTranscript, startSessionCommands)) { // Use 'this.'
                    Debug.Log($"COMMAND DETECTED: START SESSION (from Complete) - '{correctedTranscript}'", this);
                    sessionController?.StartSession();
                    feedbackManager?.PlaySuccessFeedback("Starting new session");
                    AudioManager.Instance?.PlayConfirmationSound();
                    commandMatched = true;
                }
                break;
        }
        return commandMatched; // Error was here: Invalid token 'return'
     } // This brace likely closes ProcessTranscript

    // Methods previously thought to be outside the class scope by the compiler
    private string ApplyTranscriptionCorrections(string transcript) {
        if (string.IsNullOrEmpty(transcript)) return transcript;
        string corrected = transcript.ToLower();
        foreach (var kvp in commonMistranscriptions) {
            if (corrected.Contains(kvp.Key)) {
                 corrected = corrected.Replace(kvp.Key, kvp.Value);
                 if (verboseDiagnostics) Debug.Log($"Applied correction: '{kvp.Key}' -> '{kvp.Value}'", this);
            }
        }
        return corrected.Trim();
     }

    private bool ContainsAny(string source, string[] keywords) {
        if (string.IsNullOrEmpty(source) || keywords == null || keywords.Length == 0) return false;
        foreach (string keyword in keywords) {
             if (string.IsNullOrEmpty(keyword)) continue;
             if (source.Equals(keyword.ToLower())) return true;
        }
        string paddedSource = " " + source + " ";
        foreach (string keyword in keywords) {
            if (string.IsNullOrEmpty(keyword)) continue;
             if (keyword.Contains(" ") || !source.Equals(keyword.ToLower())) {
                string spacedKeyword = " " + keyword.ToLower() + " ";
                if (paddedSource.Contains(spacedKeyword)) return true;
             }
        }
        return false;
     }


    // --- Utility & Helper Methods ---

    public void HandleWakeWordDetected() {
         if(verboseDiagnostics) Debug.Log("<color=purple>HandleWakeWordDetected: Wake word ('Okay') detected! Setting flag to wait for command.</color>", this);
         isWaitingForCommand = true;
         // AudioManager.Instance?.PlaySoundEffect(SoundType.WakeWordDetected); // Example - Requires definition
         feedbackManager?.UpdateStatusIndicator(true, "Say command now...");
    }

    public void StartListening() {
        if (appVoiceExperience == null) { Debug.LogError("Cannot start listening, AppVoiceExperience is not set.", this); return; }
        if (verboseDiagnostics) Debug.Log($"<color=yellow>StartListening Check: isListening={isListening}, IsRequestActive={appVoiceExperience.IsRequestActive}</color>", this);

        if (!appVoiceExperience.IsRequestActive) {
            if(verboseDiagnostics) Debug.Log("<color=green>Attempting to Activate voice service...</color>", this);
            try {
                var runtimeConfig = appVoiceExperience.RuntimeConfiguration;
                if (runtimeConfig?.witConfiguration == null || string.IsNullOrEmpty(runtimeConfig.witConfiguration.GetClientAccessToken())) {
                     Debug.LogError("<color=red>Cannot Activate: Wit config missing/invalid.</color>", this);
                     feedbackManager?.UpdateStatusIndicator(false, "Config/Token Error"); return;
                }
                if(verboseDiagnostics) Debug.Log("<color=lime>Calling appVoiceExperience.Activate()...</color>", this);
                appVoiceExperience.Activate();
            } catch (Exception e) {
                Debug.LogError($"<color=red>Error during appVoiceExperience.Activate(): {e.Message}\n{e.StackTrace}</color>", this);
                isListening = false;
                feedbackManager?.UpdateStatusIndicator(false, "Error activating");
                StartCoroutine(this.ReactivateListenerAfterDelay(2.0f, "ActivateException")); // Use 'this.'
            }
        } else {
             string reason = $"IsRequestActive={appVoiceExperience.IsRequestActive}";
             if(verboseDiagnostics) Debug.LogWarning($"<color=orange>Skipping Activate call: {reason}</color>", this);
        }
    }

    public void StopListening() {
         if (appVoiceExperience == null) return;
         if (appVoiceExperience.IsRequestActive) {
             if(verboseDiagnostics) Debug.Log("<color=yellow>Calling appVoiceExperience.Deactivate()...</color>", this);
             try { appVoiceExperience.Deactivate(); }
             catch (Exception e) { Debug.LogError($"Error deactivating: {e.Message}", this); isListening = false; feedbackManager?.UpdateStatusIndicator(false, "Error stopping"); }
         } else {
              if(verboseDiagnostics) Debug.Log("StopListening called but service IsRequestActive was false.", this);
              isListening = false;
         }
    }

    private IEnumerator ReactivateListenerAfterDelay(float delay, string reason) {
        if(verboseDiagnostics) Debug.Log($"<color=#00FFFF>Reactivate Coroutine ({reason}): Waiting for {delay}s...</color>", this);
        yield return new WaitForSeconds(delay);

        if (this.enabled && gameObject.activeInHierarchy &&
            sessionController != null && sessionController.GetCurrentState() != SessionState.Complete)
        {
             if(verboseDiagnostics) Debug.Log($"<color=green>Reactivate Coroutine ({reason}): Session not Complete. Calling StartListening()...</color>", this);
             this.StartListening(); // Use 'this.'
        } else {
            if(verboseDiagnostics) Debug.LogWarning($"<color=orange>Reactivate Coroutine ({reason}): Conditions not met. Not starting listener.</color>", this);
            isListening = false;
        }
    }

    private Color ParseColor(string colorName) {
        if (string.IsNullOrEmpty(colorName)) return Color.white;
        switch (colorName.ToLower()) {
            case "red": return Color.red; case "blue": return Color.blue; case "green": return Color.green;
            case "yellow": return Color.yellow; case "white": return Color.white; case "black": return Color.black;
            case "cyan": return Color.cyan; case "magenta": return Color.magenta; case "gray": case "grey": return Color.gray;
            case "purple": return new Color(0.5f, 0f, 0.5f); case "orange": return new Color(1.0f, 0.65f, 0f);
            case "pink": return new Color(1.0f, 0.75f, 0.8f); case "warm white": case "warm": return new Color(1.0f, 0.9f, 0.8f);
            case "cool white": case "cool": return new Color(0.8f, 0.9f, 1.0f);
            default: Debug.LogWarning($"ParseColor: Unrecognized color name '{colorName}'. Returning white."); return Color.white;
        }
     }
    private void SuggestAlternativeCommands() {
        if (sessionController == null || feedbackManager == null) return;
        SessionState currentState = sessionController.GetCurrentState();
        switch (currentState) {
            case SessionState.Idle: feedbackManager.ShowSuggestion("Try saying: \"Okay\", then \"Start therapy\""); break;
            case SessionState.Active: feedbackManager.ShowSuggestion("Try saying: \"Okay\", then \"Next step\" or \"End session\""); break; // Updated suggestion
            case SessionState.Complete: feedbackManager.ShowSuggestion("Say \"Okay\", then \"Start therapy\" to begin again."); break;
        }
     }


    // --- Editor-Only Keyboard Fallbacks ---
    private void Update() {
        #if UNITY_EDITOR
        this.HandleEditorKeyboardInput(); // Use 'this.'
        #endif
    }

    #if UNITY_EDITOR
    private void HandleEditorKeyboardInput() {
         var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Simulate Keyword Activation
        if (keyboard.lKey.wasPressedThisFrame) { // Use 'L' for "Listen" (Simulates "Okay")
             Debug.Log("EDITOR: L key pressed - Simulating Wake Word Activation");
             this.HandleWakeWordDetected(); // Use 'this.'
        }

        // Simulate commands - these only work if wake word flag is true
        if (keyboard.sKey.wasPressedThisFrame) {
            Debug.Log("EDITOR: S key pressed - Simulating 'start therapy'");
            if (sessionController?.GetCurrentState() == SessionState.Idle || sessionController?.GetCurrentState() == SessionState.Complete) {
                 this.ProcessTranscript("start therapy"); // Use 'this.'
            } else { Debug.LogWarning("EDITOR: S pressed, but session already active."); }
        } else if (keyboard.nKey.wasPressedThisFrame || keyboard.cKey.wasPressedThisFrame) {
            Debug.Log("EDITOR: N/C key pressed - Simulating 'next step'");
             if(isWaitingForCommand) { isWaitingForCommand = false; this.ProcessTranscript("next step"); } else Debug.LogWarning("EDITOR: N/C pressed, but wake word flag not set (Press L?)."); // Use 'this.'
        } else if (keyboard.eKey.wasPressedThisFrame) {
            Debug.Log("EDITOR: E key pressed - Simulating 'end session'");
            if(isWaitingForCommand) { isWaitingForCommand = false; this.ProcessTranscript("end session"); } else Debug.LogWarning("EDITOR: E pressed, but wake word flag not set (Press L?)."); // Use 'this.'
        } else if (keyboard.dKey.wasPressedThisFrame) { // Debug Status
            Debug.Log($"====== EDITOR: VOICE SYSTEM STATUS (D Key) ======");
            Debug.Log($"Session State: {sessionController?.GetCurrentState()}");
            Debug.Log($"Is Listening Flag (Internal): {isListening}");
            Debug.Log($"Is Waiting For Command Flag: {isWaitingForCommand}");
            Debug.Log($"AppVoiceExperience Active: {appVoiceExperience?.Active}");
            Debug.Log($"AppVoiceExperience IsRequestActive: {appVoiceExperience?.IsRequestActive}");
        }
     }
    #endif // UNITY_EDITOR

} // End of class VoiceCommandManager
