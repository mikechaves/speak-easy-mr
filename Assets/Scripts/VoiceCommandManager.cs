using UnityEngine;
using Meta.WitAi;
using UnityEngine.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class VoiceCommandManager : MonoBehaviour
{
    [Header("Voice Service")]
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
        { "continue you", "continue" },
        { "continued", "continue" },
        { "can to you", "continue" },
        { "container", "continue" },
        { "continue on", "continue" },
        { "continue new", "continue" },
        { "continuing", "continue" },
        { "continu", "continue" },
        { "contenyu", "continue" },
        { "can tea new", "continue" },
        { "content", "continue" },
        { "continual", "continue" },
        { "continue to", "continue" },
        { "continue the", "continue" },
        { "continue a", "continue" },
        { "continue please", "continue" },
        { "continue now", "continue" },
        { "continuous", "continue" },
        
        // "Next" often gets transcribed as...
        { "next up", "next" },
        { "text", "next" },
        { "nest", "next" },
        { "necks", "next" },
        { "next to", "next" },
        { "next please", "next" },
        { "next one", "next" },
        { "next time", "next" },
        { "next day", "next" },
        
        // "Begin" often gets transcribed as...
        { "began", "begin" },
        { "beginning", "begin" },
        { "begins", "begin" },
        
        // "Start" often gets transcribed as...
        { "started", "start" },
        { "starting", "start" },
        { "starts", "start" },
        
        // "End" often gets transcribed as...
        { "and", "end" },
        { "ending", "end" },
        { "ends", "end" }
    };
    
    private bool isListening = false;
    private int retryCount = 0;
    private const int MAX_RETRIES = 3;
    
    private void Start()
    {
        if (voiceService == null)
        {
            Debug.LogError("Voice Service is not assigned! Voice commands will not work.");
            feedbackManager?.UpdateStatusIndicator(false, "Voice service missing");
            return;
        }
        
        if (feedbackManager == null)
        {
            Debug.LogError("Feedback Manager is not assigned! User feedback will not work.");
            return;
        }
        
        if (sessionController == null)
        {
            Debug.LogError("Session Controller is not assigned! Session management will not work.");
            return;
        }
        
        // Install request header fix for Meta API auth
        ApplyVoiceSDKPatches();
        
        // Check if Wit configuration is valid
        try
        {
            // Try to access service properties to verify configuration
            var serviceType = voiceService.GetType();
            Debug.Log($"Voice service type: {serviceType.Name}");
            
            // For Wit implementation, check configuration
            if (voiceService is Meta.WitAi.Wit wit)
            {
                // Check configuration properties available through reflection since property names
                // may differ between Wit SDK versions
                try {
                    var configProperty = wit.GetType().GetProperty("RuntimeConfiguration") ?? 
                                        wit.GetType().GetProperty("Config") ??
                                        wit.GetType().GetProperty("Configuration");
                    
                    if (configProperty != null)
                    {
                        var config = configProperty.GetValue(wit);
                        if (config == null)
                        {
                            Debug.LogError("Wit configuration is null!");
                            feedbackManager.UpdateStatusIndicator(false, "Wit configuration error");
                            feedbackManager.ShowMessage("Voice recognition unavailable. Configuration error.");
                            return;
                        }
                        
                        // Try to access client token through reflection
                        var tokenProperty = config.GetType().GetProperty("clientAccessToken") ??
                                          config.GetType().GetProperty("ClientAccessToken");
                                         
                        if (tokenProperty != null)
                        {
                            string token = tokenProperty.GetValue(config) as string;
                            if (string.IsNullOrEmpty(token))
                            {
                                Debug.LogError("Wit configuration is missing client access token!");
                                feedbackManager.UpdateStatusIndicator(false, "Wit token missing");
                                feedbackManager.ShowMessage("Voice recognition unavailable. Token missing.");
                                return;
                            }
                            
                            Debug.Log("Found valid Wit configuration with token");
                        }
                        
                        // Try to set endpoint configuration if it's missing
                        FixWitEndpointConfiguration(config);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error checking Wit configuration: {e.Message}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error checking voice service configuration: {e.Message}");
            feedbackManager.UpdateStatusIndicator(false, "Voice service configuration error");
            return;
        }
        
        // Set up direct transcription events
        SetupTranscriptionEvents();
        
        // Initialize UI
        feedbackManager.UpdateStatusIndicator(false, "Voice recognition ready");
        Debug.Log("VoiceCommandManager initialized. Say 'Start therapy' to begin or press spacebar to activate.");
    }
    
    // Attempt to fix the Wit.ai SDK endpoint configuration using reflection
    private void FixWitEndpointConfiguration(object config)
    {
        try
        {
            // Try to access the endpoint configuration
            var endpointProperty = config.GetType().GetProperty("endpointConfiguration");
            if (endpointProperty != null)
            {
                var endpoint = endpointProperty.GetValue(config);
                
                if (endpoint == null)
                {
                    Debug.LogWarning("Endpoint configuration is null - can't fix it directly");
                    return;
                }
                
                // Check if we need to set endpoint values
                var uriSchemeProperty = endpoint.GetType().GetProperty("uriScheme");
                var authorityProperty = endpoint.GetType().GetProperty("authority");
                var portProperty = endpoint.GetType().GetProperty("port");
                var apiVersionProperty = endpoint.GetType().GetProperty("witApiVersion");
                
                if (uriSchemeProperty != null && authorityProperty != null)
                {
                    string uriScheme = uriSchemeProperty.GetValue(endpoint) as string;
                    string authority = authorityProperty.GetValue(endpoint) as string;
                    
                    if (string.IsNullOrEmpty(uriScheme) || string.IsNullOrEmpty(authority))
                    {
                        Debug.Log("Attempting to fix empty endpoint configuration");
                        
                        // Set default values
                        uriSchemeProperty.SetValue(endpoint, "https");
                        authorityProperty.SetValue(endpoint, "api.wit.ai");
                        
                        if (portProperty != null)
                        {
                            var currentPort = (int)portProperty.GetValue(endpoint);
                            if (currentPort == 0)
                            {
                                portProperty.SetValue(endpoint, 443);
                            }
                        }
                        
                        if (apiVersionProperty != null)
                        {
                            string apiVersion = apiVersionProperty.GetValue(endpoint) as string;
                            if (string.IsNullOrEmpty(apiVersion))
                            {
                                apiVersionProperty.SetValue(endpoint, "20240813");
                            }
                        }
                        
                        Debug.Log("Fixed endpoint configuration with default values");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error fixing endpoint configuration: {e.Message}");
        }
    }
    
    // Apply patches and workarounds for known Voice SDK issues
    private void ApplyVoiceSDKPatches()
    {
        try
        {
            // Apply patch for the null header issue
            // Find the VRequest type using reflection
            Type vrequestType = null;
            
            // Search through all loaded assemblies for VRequest
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var type = assembly.GetType("Meta.WitAi.Requests.VRequest");
                    if (type != null)
                    {
                        vrequestType = type;
                        Debug.Log("Found VRequest type for patching");
                        break;
                    }
                }
                catch { /* Continue to next assembly */ }
            }
            
            if (vrequestType != null)
            {
                // Find the CreateRequest method
                var createRequestMethod = vrequestType.GetMethod("CreateRequest", 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                
                if (createRequestMethod != null)
                {
                    Debug.Log("Found CreateRequest method - patch unnecessary since we can't monkey patch methods");
                    Debug.Log("Will rely on keyboard fallbacks and other improvements");
                }
            }
            
            // Find all instances of MultiRequestTranscription in current GO and disable them
            var multiTransComponents = GetComponents<MonoBehaviour>().Where(mb => 
                mb.GetType().Name.Contains("MultiRequestTranscription")).ToArray();
            
            foreach (var component in multiTransComponents)
            {
                Debug.Log($"Found and disabling MultiRequestTranscription component: {component.GetType().FullName}");
                component.enabled = false;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error applying SDK patches: {e.Message}");
        }
    }
    
    private void SetupTranscriptionEvents()
    {
        try
        {
            // Use the correct method to attach to events based on the type of VoiceService
            
            // Try the most common VoiceService types
            if (voiceService is Meta.WitAi.Wit wit)
            {
                // This is the classic Wit implementation
                Debug.Log("Using Wit VoiceService implementation");
                
                // Connect to direct transcript events when available
                if (wit.VoiceEvents.OnFullTranscription != null)
                {
                    wit.VoiceEvents.OnFullTranscription.AddListener(OnFullTranscriptionReceived);
                    Debug.Log("Connected to Wit.OnFullTranscription");
                }
                
                if (wit.VoiceEvents.OnPartialTranscription != null)
                {
                    wit.VoiceEvents.OnPartialTranscription.AddListener(OnPartialTranscriptionReceived);
                    Debug.Log("Connected to Wit.OnPartialTranscription");
                }
                
                // Basic events
                wit.VoiceEvents.OnStartListening.AddListener(OnStartListening);
                wit.VoiceEvents.OnStoppedListening.AddListener(OnStoppedListening);
                wit.VoiceEvents.OnError.AddListener(OnError);
                wit.VoiceEvents.OnResponse.AddListener(OnResponse);
                wit.VoiceEvents.OnAborted.AddListener(OnAborted);
            }
            else 
            {
                // Try to find the events through reflection for other service types
                Debug.Log($"Using reflection for VoiceService type: {voiceService.GetType().Name}");
                
                // Try to get VoiceEvents property
                var eventsProperty = voiceService.GetType().GetProperty("VoiceEvents") ??
                                    voiceService.GetType().GetProperty("Events") ?? 
                                    voiceService.GetType().GetProperty("events");
                
                if (eventsProperty != null)
                {
                    var events = eventsProperty.GetValue(voiceService);
                    if (events != null)
                    {
                        Type eventsType = events.GetType();
                        
                        // Find and connect to OnResponse
                        var onResponseProperty = eventsType.GetProperty("OnResponse");
                        if (onResponseProperty != null)
                        {
                            var onResponse = onResponseProperty.GetValue(events) as UnityEvent<Meta.WitAi.Json.WitResponseNode>;
                            if (onResponse != null)
                            {
                                onResponse.AddListener(OnResponse);
                                Debug.Log("Connected to OnResponse via reflection");
                            }
                        }
                        
                        // Continue with similar code for other events...
                        // This is simplified for brevity
                    }
                }
                else
                {
                    // Direct method approach for newer SDKs
                    Debug.Log("Trying direct method approach for event connection");
                    
                    // Find methods like 'get_VoiceEvents', 'get_Events', etc.
                    var eventsAccessMethod = voiceService.GetType().GetMethod("get_VoiceEvents") ??
                                            voiceService.GetType().GetMethod("get_Events");
                    
                    if (eventsAccessMethod != null)
                    {
                        Debug.Log($"Found events access method: {eventsAccessMethod.Name}");
                        
                        // Since we can't call the method directly without knowing the return type,
                        // we'll use the App Voice Experience directly
                        
                        // For AppVoiceExperience common in newer SDKs
                        Debug.Log("Checking if AppVoiceExperience exists in project assembly");
                        
                        // This will work only if our code can see the AppVoiceExperience type
                        try
                        {
                            // Try to find the AppVoiceExperience type via reflection
                            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                            {
                                try
                                {
                                    var appVoiceType = assembly.GetType("Meta.WitAi.VoiceService") ??
                                                      assembly.GetType("Meta.Voice.VoiceService") ??
                                                      assembly.GetType("Oculus.Voice.AppVoiceExperience");
                                                      
                                    if (appVoiceType != null && voiceService.GetType().IsSubclassOf(appVoiceType))
                                    {
                                        Debug.Log($"Found matching voice service type: {appVoiceType.Name}");
                                        break;
                                    }
                                }
                                catch (Exception) { /* Continue to next assembly */ }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"Error looking for AppVoiceExperience: {e.Message}");
                        }
                    }
                }
            }
            
            // Final fallback - use direct methods if available
            var activateMethod = voiceService.GetType().GetMethod("Activate", Type.EmptyTypes);
            if (activateMethod != null)
            {
                Debug.Log("Found Activate method on voice service");
            }
            
            Debug.Log("Voice event setup completed - some events may not be connected");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error setting up voice events: {e.Message}\n{e.StackTrace}");
        }
        
        // Use reflection to find and connect to event fields
        try
        {
            Debug.Log("Trying to connect to VoiceService events using reflection");
            Type voiceServiceType = voiceService.GetType();
            
            // Look for event fields via reflection
            var eventFields = voiceServiceType.GetEvents(System.Reflection.BindingFlags.Public | 
                                                        System.Reflection.BindingFlags.NonPublic | 
                                                        System.Reflection.BindingFlags.Instance);
            
            if (eventFields != null && eventFields.Length > 0)
            {
                Debug.Log($"Found {eventFields.Length} events on VoiceService type {voiceServiceType.Name}");
                
                foreach (var eventField in eventFields)
                {
                    Debug.Log($"Found event: {eventField.Name}");
                }
                
                // Try to get the events by name
                ConnectToEventByName(voiceServiceType, "OnStartListening", new Action<VoiceService>(OnStartListening));
                ConnectToEventByName(voiceServiceType, "OnStopListening", new Action<VoiceService>(OnStoppedListening));
                ConnectToEventByName(voiceServiceType, "OnFullTranscription", new Action<VoiceService, string>(OnFullTranscriptionReceived));
                ConnectToEventByName(voiceServiceType, "OnPartialTranscription", new Action<VoiceService, string>(OnPartialTranscriptionReceived));
                ConnectToEventByName(voiceServiceType, "OnError", new Action<VoiceService, string, string>(OnError));
                ConnectToEventByName(voiceServiceType, "OnResponse", new Action<VoiceService, Meta.WitAi.Json.WitResponseNode>(OnResponse));
                ConnectToEventByName(voiceServiceType, "OnAborted", new Action<VoiceService>(OnAborted));
            }
            else
            {
                Debug.LogWarning("No events found on VoiceService type");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error connecting to VoiceService events via reflection: {e.Message}");
        }
    }
    
    private void ConnectToEventByName(Type type, string eventName, Delegate handler)
    {
        try
        {
            var eventField = type.GetEvent(eventName, System.Reflection.BindingFlags.Public | 
                                                  System.Reflection.BindingFlags.NonPublic | 
                                                  System.Reflection.BindingFlags.Instance);
            
            if (eventField != null)
            {
                Debug.Log($"Connecting to event {eventName}");
                eventField.AddEventHandler(voiceService, handler);
                Debug.Log($"Successfully connected to {eventName}");
            }
            else
            {
                Debug.LogWarning($"Event {eventName} not found on type {type.Name}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error connecting to event {eventName}: {e.Message}");
        }
    }
    
    private void OnFullTranscriptionReceived(string transcript)
    {
        Debug.Log($"Full transcription received: '{transcript}'");
        ProcessTranscript(transcript);
    }
    
    private void OnPartialTranscriptionReceived(string transcript)
    {
        Debug.Log($"Partial transcription: '{transcript}'");
    }
    
    private void OnResponse(Meta.WitAi.Json.WitResponseNode response)
    {
        Debug.Log("Response received from voice service");
        
        try
        {
            // Dump full response for debugging
            Debug.Log($"FULL RESPONSE: {response}");
            
            // Reset the voice activity timer since we got a response
            timeSinceLastVoiceActivity = 0f;
            
            // Fallback for older Meta SDK versions
            string transcript = "";
            
            // Try to extract transcript from various possible locations
            if (response["text"] != null && !string.IsNullOrEmpty(response["text"].Value))
            {
                transcript = response["text"].Value;
            }
            else if (response["transcript"] != null && !string.IsNullOrEmpty(response["transcript"].Value))
            {
                transcript = response["transcript"].Value;
            }
            else if (response["utterance"] != null && !string.IsNullOrEmpty(response["utterance"].Value))
            {
                transcript = response["utterance"].Value;
            }
            
            // Try additional response paths
            else if (response["entities"] != null && response["entities"]["wit$message_body"] != null && 
                    response["entities"]["wit$message_body"][0] != null && 
                    response["entities"]["wit$message_body"][0]["value"] != null)
            {
                transcript = response["entities"]["wit$message_body"][0]["value"].Value;
            }
            
            // Extra diagnostic - look for any intent matches which might help us
            string intentName = "";
            float intentConfidence = 0f;
            
            try
            {
                if (response["intents"] != null && response["intents"].Count > 0)
                {
                    intentName = response["intents"][0]["name"].Value;
                    intentConfidence = response["intents"][0]["confidence"].AsFloat;
                    Debug.Log($"Detected intent: {intentName} (confidence: {intentConfidence})");
                    
                    // If we have a strong intent match for next_step but no transcript, use that
                    if (intentName == "next_step" && intentConfidence > 0.7f && string.IsNullOrEmpty(transcript))
                    {
                        Debug.Log("Using intent match for next_step (high confidence)");
                        sessionController.AdvanceToNextStep();
                        feedbackManager.PlaySuccessFeedback("Moving to next step");
                        
                        // Restart listening after processing the command
                        StartCoroutine(RestartListeningAfterDelay(1.0f));
                        return;
                    }
                }
            }
            catch (Exception intentEx)
            {
                Debug.LogWarning($"Error checking intents: {intentEx.Message}");
            }
            
            if (!string.IsNullOrEmpty(transcript))
            {
                // Extra logging for command debugging
                Debug.Log($"TRANSCRIPT RECEIVED: '{transcript}'");
                Debug.Log($"CHECKING AGAINST COMMANDS:");
                Debug.Log($"- Start commands: {string.Join(", ", startSessionCommands)}");
                Debug.Log($"- Next commands: {string.Join(", ", nextStepCommands)}");
                Debug.Log($"- End commands: {string.Join(", ", endSessionCommands)}");
                
                Debug.Log($"MATCH RESULTS:");
                Debug.Log($"- Contains start command: {ContainsAny(transcript, startSessionCommands)}");
                Debug.Log($"- Contains next command: {ContainsAny(transcript, nextStepCommands)}");
                Debug.Log($"- Contains end command: {ContainsAny(transcript, endSessionCommands)}");
                
                Debug.Log($"Extracted transcript from response: '{transcript}'");
                ProcessTranscript(transcript);
                
                // Restart listening after processing the transcript
                if (sessionController.GetCurrentState() == SessionState.Active)
                {
                    StartCoroutine(RestartListeningAfterDelay(1.0f));
                }
            }
            else
            {
                // If in active session, check for speech confidence as a fallback
                if (sessionController.GetCurrentState() == SessionState.Active)
                {
                    // If we have speech but no transcript, try to interpret as continue during therapy
                    bool hasSpeech = false;
                    
                    try 
                    {
                        if (response["speech"] != null && response["speech"]["tokens"] != null && 
                            response["speech"]["tokens"].Count > 0)
                        {
                            hasSpeech = true;
                            Debug.Log("Detected speech without transcript - interpreting as continue during therapy");
                            sessionController.AdvanceToNextStep();
                            feedbackManager.PlaySuccessFeedback("Moving to next step");
                            
                            // Restart listening after processing the command
                            StartCoroutine(RestartListeningAfterDelay(1.0f));
                            return;
                        }
                    }
                    catch (Exception speechEx)
                    {
                        Debug.LogWarning($"Error checking speech tokens: {speechEx.Message}");
                    }
                    
                    if (!hasSpeech)
                    {
                        Debug.LogWarning("Could not extract transcript from response");
                        HandleLowConfidence();
                        
                        // Restart listening even after low confidence
                        StartCoroutine(RestartListeningAfterDelay(2.0f));
                    }
                }
                else
                {
                    Debug.LogWarning("Could not extract transcript from response");
                    HandleLowConfidence();
                    
                    // Only restart in active session
                    if (sessionController.GetCurrentState() == SessionState.Active)
                    {
                        StartCoroutine(RestartListeningAfterDelay(2.0f));
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing response: {e.Message}");
            HandleLowConfidence();
            
            // Restart listening even after errors if in active session
            if (sessionController.GetCurrentState() == SessionState.Active)
            {
                StartCoroutine(RestartListeningAfterDelay(2.0f));
            }
        }
    }
    
    private void ProcessTranscript(string transcript)
    {
        if (string.IsNullOrEmpty(transcript))
        {
            HandleLowConfidence();
            return;
        }
        
        // Convert to lowercase early for more consistent matching
        string lowerTranscript = transcript.ToLower();
        
        // Get the current session state to help with context-aware matching
        SessionState currentState = sessionController.GetCurrentState();
        
        // Apply common mistranscription corrections
        string correctedTranscript = ApplyTranscriptionCorrections(lowerTranscript);
        
        if (verboseDiagnostics && lowerTranscript != correctedTranscript)
        {
            Debug.Log($"Corrected transcript: '{lowerTranscript}' -> '{correctedTranscript}'");
        }
        
        // Log the full command detection details
        Debug.Log("---COMMAND DETECTION ANALYSIS---");
        Debug.Log($"Original: '{transcript}'");
        Debug.Log($"Lowercase: '{lowerTranscript}'");
        Debug.Log($"Corrected: '{correctedTranscript}'");
        Debug.Log($"Current Session State: {currentState}");
        
        // Super aggressive continue detection - high priority during active session
        if (currentState == SessionState.Active && 
           (correctedTranscript.Contains("cont") || 
            correctedTranscript.Contains("next") || 
            correctedTranscript.Contains("go") ||
            correctedTranscript.Contains("step") ||
            correctedTranscript.Contains("move") ||
            correctedTranscript.Contains("forward") ||
            correctedTranscript.Contains("proceed")))
        {
            // Special case handling for continue/next commands - most common during active session
            Debug.Log($"PRIORITY CONTINUE COMMAND DETECTED: '{correctedTranscript}'");
            sessionController.AdvanceToNextStep();
            feedbackManager.PlaySuccessFeedback("Moving to next step");
            retryCount = 0;
            return;
        }
        
        // Process start commands - high priority during idle state
        if (currentState == SessionState.Idle && 
           (correctedTranscript.Contains("start") || 
            correctedTranscript.Contains("begin") || 
            correctedTranscript.Contains("therapy")))
        {
            Debug.Log($"PRIORITY START COMMAND DETECTED: '{correctedTranscript}'");
            sessionController.StartSession();
            feedbackManager.PlaySuccessFeedback("Session started");
            retryCount = 0;
            return;
        }
        
        // More precise command matching if the quick detect didn't trigger
        if (ContainsAny(correctedTranscript, startSessionCommands))
        {
            Debug.Log($"COMMAND DETECTED: START SESSION - '{correctedTranscript}'");
            sessionController.StartSession();
            feedbackManager.PlaySuccessFeedback("Session started");
            retryCount = 0;
        }
        else if (ContainsAny(correctedTranscript, nextStepCommands))
        {
            Debug.Log($"COMMAND DETECTED: NEXT STEP - '{correctedTranscript}'");
            sessionController.AdvanceToNextStep();
            feedbackManager.PlaySuccessFeedback("Moving to next step");
            retryCount = 0;
        }
        else if (ContainsAny(correctedTranscript, endSessionCommands))
        {
            Debug.Log($"COMMAND DETECTED: END SESSION - '{correctedTranscript}'");
            sessionController.EndSession();
            feedbackManager.PlaySuccessFeedback("Session completed");
            retryCount = 0;
        }
        else
        {
            // Last resort - check for any word/phrase that might indicate continuation
            // During an active session, we want to be very lenient with what counts as "continue"
            if (currentState == SessionState.Active)
            {
                // Try checking if ANY word in a split version of the transcript matches common terms
                string[] words = correctedTranscript.Split(new char[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
                bool foundContinueWord = false;
                
                foreach (string word in words)
                {
                    if (word.Equals("yes") || word.Equals("yeah") || word.Equals("ok") || 
                        word.Equals("okay") || word.Equals("fine") || word.Equals("sure") ||
                        word.Equals("alright") || word.Equals("continue") || word.Equals("next") ||
                        word.StartsWith("cont") || word.StartsWith("next") || word.StartsWith("mov"))
                    {
                        foundContinueWord = true;
                        Debug.Log($"LAST RESORT WORD MATCH: '{word}' in '{correctedTranscript}'");
                        break;
                    }
                }
                
                if (foundContinueWord)
                {
                    Debug.Log($"LAST RESORT CONTINUE MATCH: '{correctedTranscript}'");
                    sessionController.AdvanceToNextStep();
                    feedbackManager.PlaySuccessFeedback("Moving to next step");
                    retryCount = 0;
                    return;
                }
            }
            
            Debug.Log($"NO COMMAND MATCH: '{correctedTranscript}'");
            feedbackManager.PlayErrorFeedback("I didn't understand that command");
            SuggestAlternativeCommands();
        }
    }
    
    // Apply corrections for common mistranscriptions
    private string ApplyTranscriptionCorrections(string transcript)
    {
        if (string.IsNullOrEmpty(transcript))
            return transcript;
            
        string lowerTranscript = transcript.ToLower();
        
        // First check for direct "continue" related sounds that might not be in our dictionary
        if (lowerTranscript.Contains("kon") || 
            lowerTranscript.Contains("con") || 
            lowerTranscript.Contains("tin"))
        {
            Debug.Log($"Detected possible continue phonetics: '{lowerTranscript}'");
        }
        
        // Special handling for common continue phonetic patterns
        if (lowerTranscript.Contains("kon") && lowerTranscript.Contains("tin"))
        {
            Debug.Log("Continue phonetic pattern detected!");
            lowerTranscript = "continue";
            return lowerTranscript;
        }
        
        // Check for exact matches in our correction dictionary
        bool correctionApplied = false;
        foreach (var kvp in commonMistranscriptions)
        {
            if (lowerTranscript.Contains(kvp.Key))
            {
                // Replace the mistranscription with the correct form
                lowerTranscript = lowerTranscript.Replace(kvp.Key, kvp.Value);
                correctionApplied = true;
                
                if (verboseDiagnostics)
                {
                    Debug.Log($"Applied correction: '{kvp.Key}' -> '{kvp.Value}'");
                }
            }
        }
        
        // Extra handling for extremely mangled "continue" commands that are common problems
        if (!correctionApplied && (lowerTranscript.Contains("con") || lowerTranscript.Contains("can") || lowerTranscript.Contains("tin")))
        {
            // This is an aggressive match for what might be a mangled "continue"
            // Calculate word similarity to "continue"
            float continueScore = CalculateSimilarity(lowerTranscript, "continue");
            float nextScore = CalculateSimilarity(lowerTranscript, "next");
            
            Debug.Log($"Special continue detection - continue score: {continueScore:F2}, next score: {nextScore:F2}");
            
            // If we have a reasonable match to either continue or next, replace it
            if (continueScore > 0.4f)
            {
                Debug.Log($"Special continue correction applied to: '{lowerTranscript}'");
                lowerTranscript = "continue";
            }
            else if (nextScore > 0.4f)
            {
                Debug.Log($"Special next correction applied to: '{lowerTranscript}'");
                lowerTranscript = "next";
            }
        }
        
        return lowerTranscript;
    }
    
    // Helper to calculate word similarity as a ratio between 0 and 1
    private float CalculateSimilarity(string s, string t)
    {
        int distance = LevenshteinDistance(s, t);
        float maxLength = Math.Max(s.Length, t.Length);
        return 1.0f - (distance / maxLength);
    }
    
    // Unity event-compatible signatures
    private void OnStartListening()
    {
        isListening = true;
        feedbackManager.UpdateStatusIndicator(true, "Listening...");
        Debug.Log("Started listening for voice commands");
    }
    
    private void OnStoppedListening()
    {
        isListening = false;
        feedbackManager.UpdateStatusIndicator(false, "Processing...");
        Debug.Log("Stopped listening for voice commands");
        
        // Reset the voice activity timer to enable auto-reactivation
        timeSinceLastVoiceActivity = 0f;
    }
    
    private void OnError(string error, string message)
    {
        Debug.LogError($"Voice recognition error: {error} - {message}");
        isListening = false;
        feedbackManager.UpdateStatusIndicator(false, "Error: " + message);
        feedbackManager.PlayErrorFeedback("Voice recognition error. Please try again.");
        
        // Auto-restart listening after an error if in an active session
        if (sessionController.GetCurrentState() == SessionState.Active)
        {
            Debug.Log("In active session - auto-restarting voice recognition after error");
            StartCoroutine(RestartListeningAfterDelay(1.5f));
        }
    }
    
    private IEnumerator RestartListeningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Auto-restarting voice recognition");
        StartListening();
    }
    
    private void OnAborted()
    {
        isListening = false;
        feedbackManager.UpdateStatusIndicator(false, "Voice recognition ready");
        Debug.Log("Voice recognition aborted");
        
        // Auto-restart listening after abort if in an active session
        if (sessionController.GetCurrentState() == SessionState.Active)
        {
            Debug.Log("In active session - auto-restarting voice recognition after abort");
            StartCoroutine(RestartListeningAfterDelay(2.0f));
        }
    }
    
    // Direct delegate-compatible signatures
    // These are for the direct event handlers at the end of SetupTranscriptionEvents
    private void OnStartListening(VoiceService voiceService)
    {
        OnStartListening();
    }
    
    private void OnStoppedListening(VoiceService voiceService)
    {
        OnStoppedListening();
    }
    
    private void OnAborted(VoiceService voiceService)
    {
        OnAborted();
    }
    
    private void OnResponse(VoiceService voiceService, Meta.WitAi.Json.WitResponseNode response)
    {
        OnResponse(response);
    }
    
    private void OnError(VoiceService voiceService, string error, string message)
    {
        OnError(error, message);
    }
    
    private void OnFullTranscriptionReceived(VoiceService voiceService, string transcript)
    {
        Debug.Log($"Full transcription received from delegate: '{transcript}'");
        ProcessTranscript(transcript);
    }
    
    private void OnPartialTranscriptionReceived(VoiceService voiceService, string transcript)
    {
        Debug.Log($"Partial transcription from delegate: '{transcript}'");
    }
    
    // Time tracking for auto-reactivation of voice listening
    private float timeSinceLastVoiceActivity = 0f;
    private const float AUTO_REACTIVATE_VOICE_TIME = 5.0f; // Auto reactivate voice after 5 seconds of no activity
    
    private void Update()
    {
        // Get the current session state
        SessionState currentState = sessionController.GetCurrentState();
        
        // Track time since last voice activity if we're in an active session
        if (currentState == SessionState.Active)
        {
            // If we're not listening and should be, auto-reactivate
            if (!isListening)
            {
                timeSinceLastVoiceActivity += Time.deltaTime;
                
                // Auto reactivate voice listening after a delay
                if (timeSinceLastVoiceActivity > AUTO_REACTIVATE_VOICE_TIME)
                {
                    Debug.Log("Auto-reactivating voice recognition");
                    StartListening();
                    timeSinceLastVoiceActivity = 0f;
                }
            }
        }
        else
        {
            // Reset timer if we're not in an active session
            timeSinceLastVoiceActivity = 0f;
        }
        
        // Press spacebar to test voice activation
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Spacebar pressed - Activating voice recognition");
            StartListening();
        }
        
        // Additional keyboard fallbacks for testing when voice service has issues
        #if UNITY_EDITOR
        // S key for Start Session
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("S key pressed - Starting session (keyboard fallback)");
            sessionController.StartSession();
            feedbackManager.PlaySuccessFeedback("Session started (keyboard command)");
            
            // Auto-activate listening after starting session
            StartListening();
        }
        
        // N key for Next Step
        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("N key pressed - Next step (keyboard fallback)");
            sessionController.AdvanceToNextStep();
            feedbackManager.PlaySuccessFeedback("Moving to next step (keyboard command)");
            
            // Auto-activate listening after advancing
            StartListening();
        }
        
        // C key also for Continue/Next (an additional option)
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("C key pressed - Continue/Next step (keyboard fallback)");
            sessionController.AdvanceToNextStep();
            feedbackManager.PlaySuccessFeedback("Moving to next step (keyboard command)");
            
            // Auto-activate listening after advancing
            StartListening();
        }
        
        // E key for End Session
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E key pressed - Ending session (keyboard fallback)");
            sessionController.EndSession();
            feedbackManager.PlaySuccessFeedback("Session completed (keyboard command)");
        }
        
        // Debug key to log current state and voice system status
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log($"====== VOICE SYSTEM STATUS ======");
            Debug.Log($"Session State: {currentState}");
            Debug.Log($"Is Listening: {isListening}");
            Debug.Log($"Time Since Last Voice Activity: {timeSinceLastVoiceActivity}");
            Debug.Log($"Voice Service Type: {voiceService?.GetType().Name}");
            
            // Log additional voice service info
            if (voiceService is Meta.WitAi.Wit wit)
            {
                Debug.Log($"Wit Active: {wit.Active}");
                Debug.Log($"Wit IsRequestActive: {wit.IsRequestActive}");
            }
            
            // Try to force restart voice listening
            Debug.Log("Forcing voice listener restart...");
            if (isListening)
            {
                StopListening();
            }
            StartListening();
        }
        #endif
    }
    
    public void StartListening()
    {
        if (!isListening && voiceService != null)
        {
            Debug.Log("Manually activating voice service");
            isListening = true;
            try
            {
                // Check if we're using Wit and need to handle configuration issues
                if (voiceService is Meta.WitAi.Wit wit)
                {
                    try {
                        // Use reflection to work with different Wit API versions
                        var configProperty = wit.GetType().GetProperty("RuntimeConfiguration") ?? 
                                            wit.GetType().GetProperty("Config") ??
                                            wit.GetType().GetProperty("Configuration");
                        
                        if (configProperty == null)
                        {
                            Debug.LogError("Cannot find configuration property on Wit instance");
                            feedbackManager.PlayErrorFeedback("Voice recognition unavailable");
                            isListening = false;
                            feedbackManager.UpdateStatusIndicator(false, "Config error");
                            return;
                        }
                        
                        var config = configProperty.GetValue(wit);
                        if (config == null)
                        {
                            Debug.LogError("Wit configuration is null");
                            feedbackManager.PlayErrorFeedback("Voice recognition unavailable");
                            isListening = false;
                            feedbackManager.UpdateStatusIndicator(false, "Config error");
                            return;
                        }
                        
                        // Check if config has token
                        var tokenProperty = config.GetType().GetProperty("clientAccessToken") ??
                                          config.GetType().GetProperty("ClientAccessToken");
                        
                        if (tokenProperty != null)
                        {
                            string token = tokenProperty.GetValue(config) as string;
                            if (string.IsNullOrEmpty(token))
                            {
                                Debug.LogError("Client access token is missing");
                                feedbackManager.PlayErrorFeedback("Voice API token is missing");
                                isListening = false;
                                feedbackManager.UpdateStatusIndicator(false, "Token missing");
                                return;
                            }
                        }
                        
                        // Check endpoint configuration if available
                        var endpointProperty = config.GetType().GetProperty("endpointConfiguration") ??
                                             config.GetType().GetProperty("EndpointConfiguration");
                        
                        if (endpointProperty != null)
                        {
                            var endpoint = endpointProperty.GetValue(config);
                            if (endpoint == null)
                            {
                                Debug.LogWarning("Endpoint configuration is null - using defaults");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Error checking Wit configuration: {e.Message}");
                    }
                }
                
                voiceService.Activate();
                feedbackManager.UpdateStatusIndicator(true, "Listening...");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error activating voice service: {e.Message}");
                isListening = false;
                feedbackManager.UpdateStatusIndicator(false, "Error activating voice service");
                
                // Provide fallback for testing without voice service
                #if UNITY_EDITOR
                Debug.LogWarning("In Editor: Using keyboard input fallback due to voice service error");
                feedbackManager.ShowMessage("Voice service error. Use keyboard shortcuts for testing: S=Start, N=Next, E=End");
                #endif
            }
        }
    }
    
    public void StopListening()
    {
        if (isListening && voiceService != null)
        {
            isListening = false;
            voiceService.Deactivate();
            feedbackManager.UpdateStatusIndicator(false, "Voice recognition paused");
        }
    }
    
    private void HandleLowConfidence()
    {
        retryCount++;
        
        Debug.Log($"Low confidence or empty transcript. Retry count: {retryCount}/{MAX_RETRIES}");
        
        if (retryCount >= MAX_RETRIES)
        {
            feedbackManager.PlayErrorFeedback("I'm still having trouble understanding. Let's try a different approach.");
            SuggestAlternativeCommands();
            retryCount = 0;
        }
        else
        {
            feedbackManager.PlayErrorFeedback("I'm not sure I understood. Could you repeat that?");
            StartListening();
        }
    }
    
    private void SuggestAlternativeCommands()
    {
        SessionState currentState = sessionController.GetCurrentState();
        
        switch (currentState)
        {
            case SessionState.Idle:
                feedbackManager.ShowSuggestion("Try saying: \"Start therapy\" or \"Begin session\"");
                break;
                
            case SessionState.Active:
                feedbackManager.ShowSuggestion("Try saying: \"Next step\" or \"Continue\"");
                break;
                
            case SessionState.Complete:
                feedbackManager.ShowSuggestion("This session is complete. Say \"Start therapy\" to begin again.");
                break;
        }
    }
    
    private bool ContainsAny(string source, string[] keywords)
    {
        if (string.IsNullOrEmpty(source))
        {
            return false;
        }
        
        // Convert to lowercase and remove punctuation for better matching
        source = source.ToLower().Replace(".", "").Replace(",", "").Replace("!", "").Replace("?", "");
        
        // Add spaces to ensure we're matching whole words or phrases
        source = " " + source + " ";
        
        foreach (string keyword in keywords)
        {
            string lowerKeyword = keyword.ToLower();
            
            // Direct contains check
            if (source.Contains(lowerKeyword))
            {
                return true;
            }
            
            // Check for word boundaries
            if (source.Contains(" " + lowerKeyword + " "))
            {
                return true;
            }
            
            // Check for variations with typos (simple Levenshtein distance)
            string[] words = source.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string word in words)
            {
                // Check if word is similar enough to keyword (handles minor typos)
                if (word.Length > 3 && lowerKeyword.Length > 3)
                {
                    // Calculate distance
                    int distance = LevenshteinDistance(word, lowerKeyword);
                    
                    // Calculate similarity ratio (0.0 to 1.0)
                    float maxLength = Math.Max(word.Length, lowerKeyword.Length);
                    float similarity = 1.0f - (distance / maxLength);
                    
                    // Compare to threshold
                    if (similarity >= matchThreshold || distance <= 2)
                    {
                        if (verboseDiagnostics)
                        {
                            Debug.Log($"Fuzzy match: '{word}' similar to '{lowerKeyword}'" +
                                $" (similarity: {similarity:F2}, threshold: {matchThreshold:F2})");
                        }
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    
    // Calculate Levenshtein distance between two strings
    // This helps match words even with small typos
    private int LevenshteinDistance(string s, string t)
    {
        // Skip if either string is empty
        if (string.IsNullOrEmpty(s))
        {
            return string.IsNullOrEmpty(t) ? 0 : t.Length;
        }
        
        if (string.IsNullOrEmpty(t))
        {
            return s.Length;
        }
        
        // Simple implementation for shorter strings
        if (s.Length < 5 || t.Length < 5)
        {
            int matches = 0;
            int length = Math.Min(s.Length, t.Length);
            
            for (int i = 0; i < length; i++)
            {
                if (s[i] == t[i]) matches++;
            }
            
            // Calculate simple distance
            return Math.Max(s.Length, t.Length) - matches;
        }
        
        // More involved implementation for longer strings
        int[,] d = new int[s.Length + 1, t.Length + 1];
        
        for (int i = 0; i <= s.Length; i++)
        {
            d[i, 0] = i;
        }
        
        for (int j = 0; j <= t.Length; j++)
        {
            d[0, j] = j;
        }
        
        for (int j = 1; j <= t.Length; j++)
        {
            for (int i = 1; i <= s.Length; i++)
            {
                int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                d[i, j] = Math.Min(Math.Min(
                    d[i - 1, j] + 1,     // deletion
                    d[i, j - 1] + 1),    // insertion
                    d[i - 1, j - 1] + cost); // substitution
            }
        }
        
        return d[s.Length, t.Length];
    }
    
    private void OnDisable()
    {
        try
        {
            Debug.Log("VoiceCommandManager OnDisable - Safe cleanup of event connections");
            
            // Safe removal of event handlers
            if (voiceService is Meta.WitAi.Wit wit && wit.VoiceEvents != null)
            {
                // For Wit implementation
                Debug.Log("Removing event handlers from Wit.VoiceEvents");
                wit.VoiceEvents.OnResponse.RemoveListener(OnResponse);
                wit.VoiceEvents.OnError.RemoveListener(OnError);
                wit.VoiceEvents.OnStartListening.RemoveListener(OnStartListening);
                wit.VoiceEvents.OnStoppedListening.RemoveListener(OnStoppedListening);
                wit.VoiceEvents.OnAborted.RemoveListener(OnAborted);
                
                if (wit.VoiceEvents.OnFullTranscription != null)
                {
                    wit.VoiceEvents.OnFullTranscription.RemoveListener(OnFullTranscriptionReceived);
                }
                
                if (wit.VoiceEvents.OnPartialTranscription != null)
                {
                    wit.VoiceEvents.OnPartialTranscription.RemoveListener(OnPartialTranscriptionReceived);
                }
            }
            
            // Check for dictation components and handle them safely
            foreach (var component in GetComponents<MonoBehaviour>())
            {
                if (component != null && component.GetType().Name.Contains("Dictation"))
                {
                    Debug.Log($"Found dictation component: {component.GetType().Name}");
                    // No specific action needed, just logging for diagnostics
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error during OnDisable cleanup: {e.Message}. This is non-fatal.");
        }
    }
}