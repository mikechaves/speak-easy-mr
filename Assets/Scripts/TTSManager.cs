using UnityEngine;
using UnityEngine.Events;
using Meta.WitAi.TTS.Utilities;
using Meta.WitAi.TTS.Data;

// Simple Singleton TTS Manager
public class TTSManager : MonoBehaviour
{
    // --- Singleton Pattern ---
    private static TTSManager _instance;
    public static TTSManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find an existing instance in the scene
                _instance = FindObjectOfType<TTSManager>();

                // If not found, create a new GameObject and add the component
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("TTSManager_Runtime");
                    _instance = singletonObject.AddComponent<TTSManager>();
                    Debug.Log("TTSManager instance created dynamically.");
                }
            }
            return _instance;
        }
    }
    // --- End Singleton Pattern ---

    [Header("TTS References")]
    [Tooltip("Assign the TTSSpeaker component from your scene here.")]
    [SerializeField] private TTSSpeaker speaker;

    [Header("Events (For Other Scripts)")]
    [Tooltip("Invoked when TTS playback begins.")]
     public UnityEvent OnSpeakStart; // This event is for OrbAnimator etc.
    [Tooltip("Invoked when TTS playback ends (completes, errors, or is cancelled).")]
    public UnityEvent OnSpeakEnd; // This event is for OrbAnimator etc.

    private bool isCurrentlySpeaking = false;

    // Optional: Reference to voice presets if you want to switch voices later
    // [SerializeField] private TTSVoiceSettings[] availableVoices;
    // private string selectedVoiceID = ""; // Store selected voice preset ID

    private void Awake()
    {
        // --- Singleton Enforcement ---
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Duplicate TTSManager instance found. Destroying this one.");
            Destroy(gameObject);
            return;
        }
        _instance = this;
        // Optional: Keep the manager object persistent across scene loads
        // DontDestroyOnLoad(gameObject);
        // --- End Singleton Enforcement ---

        // --- Initialization & Checks ---
        if (speaker == null)
        {
            // Attempt to find the speaker if not assigned
            speaker = FindObjectOfType<TTSSpeaker>();
            if (speaker == null)
            {
                Debug.LogError("TTSManager: TTSSpeaker component not found in the scene and not assigned in the Inspector! TTS will not function.");
            }
            else
            {
                Debug.Log("TTSManager: Found TTSSpeaker component in the scene.");
            }
        }
        else
        {
             Debug.Log("TTSManager: TTSSpeaker assigned via Inspector.");
        }

        // Optional: Initialize default voice or load user preference here
        // if (availableVoices != null && availableVoices.Length > 0) {
        //     SetVoice(availableVoices[0].PresetId); // Example: Set default voice
        // }
    }

    private void OnDestroy() {
             // <<< --- REMOVED: Event unsubscription code from OnDestroy --- >>>
             // Listeners connected via Inspector are handled automatically by Unity
             // <<< --- END REMOVED --- >>>

             if (_instance == this) {
                 _instance = null;
             }
    }

    /// <summary>
    /// Speaks the provided text using the assigned TTSSpeaker.
    /// </summary>
    /// <param name="textToSpeak">The text string to synthesize and speak.</param>
    public void Speak(string textToSpeak)
    {
        if (speaker == null)
        {
            Debug.LogError("TTSManager.Speak: Cannot speak, TTSSpeaker reference is missing.");
            return;
        }

        if (string.IsNullOrEmpty(textToSpeak))
        {
             Debug.LogWarning("TTSManager.Speak: Received empty text to speak. Skipping.");
             return;
        }

        // Basic check: If the speaker is already speaking, you might want to stop it first
        // or queue the request. For simplicity, we'll just call Speak directly.
        // The TTSSpeaker component might handle interruptions automatically or have options.
        if (speaker.IsSpeaking)
        {
             Debug.Log("TTSManager.Speak: Speaker is already speaking. Interrupting with new request.");
             // Optionally call speaker.Stop() first if needed, depending on TTSSpeaker behavior
             // speaker.Stop();
        }

        Debug.Log($"TTSManager: Requesting speech for: \"{textToSpeak}\"");
        try
        {
            // Use the Speak method of the TTSSpeaker component
            speaker.Speak(textToSpeak);

            // Optional: Subscribe to events if needed (e.g., to know when speech finishes)
            // speaker.Events.OnPlaybackComplete.AddListener(HandlePlaybackComplete);
            // speaker.Events.OnError.AddListener(HandleSpeechError);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TTSManager.Speak: Error calling speaker.Speak(): {e.Message}\n{e.StackTrace}");
        }
    }

    // --- Event Handlers (MUST BE PUBLIC TO BE VISIBLE IN INSPECTOR) ---

        /// <summary>
        /// PUBLIC handler called by TTSSpeaker's "On Playback Started" Inspector event.
        /// </summary>
        public void HandlePlaybackStarted() { // <<< MADE PUBLIC >>>
            Debug.Log("TTSManager: Handler - Playback Started");
            if (!isCurrentlySpeaking) {
                 isCurrentlySpeaking = true;
                 OnSpeakStart?.Invoke(); // Invoke our own event for OrbAnimator etc.
                 Debug.Log("TTSManager: OnSpeakStart Invoked.");
            } else { Debug.Log("TTSManager: Playback Started event received, but already speaking. Ignoring duplicate start."); }
        }

        /// <summary>
        /// PUBLIC handler called by TTSSpeaker's "On Playback Complete" and "On Playback Cancelled" Inspector events.
        /// </summary>
        public void HandlePlaybackEnded() { // <<< MADE PUBLIC >>>
             Debug.Log("TTSManager: Handler - Playback Ended (Completed or Cancelled)");
             if (isCurrentlySpeaking) {
                isCurrentlySpeaking = false;
                OnSpeakEnd?.Invoke(); // Invoke our own event for OrbAnimator etc.
                Debug.Log("TTSManager: OnSpeakEnd Invoked.");
             } else { Debug.Log("TTSManager: Playback Ended event received, but already idle. Ignoring duplicate end."); }
        }

        /// <summary>
        /// PUBLIC handler called by TTSSpeaker's "On Error" Inspector event.
        /// </summary>
        /*
        public void HandlePlaybackError(string code, string message) { // <<< MADE PUBLIC - Note: Inspector might not support string params directly
             Debug.LogError($"TTSManager: Handler - Playback Error - Code: {code}, Message: {message}");
              if (isCurrentlySpeaking) {
                isCurrentlySpeaking = false;
                OnSpeakEnd?.Invoke(); // Treat error as end of speaking
                Debug.Log("TTSManager: OnSpeakEnd Invoked due to error.");
             } else { Debug.Log("TTSManager: Playback Error event received, but already idle. Ignoring duplicate end."); }
             // If the Inspector event for Error doesn't take string params, you might need a separate public void HandlePlaybackErrorSimple() method.
        }
        */

        // Optional simple handler if the Inspector event for Error doesn't take parameters
        /*
        public void HandlePlaybackErrorSimple() {
             Debug.LogError($"TTSManager: Handler - Playback Error (Simple)");
             if (isCurrentlySpeaking) {
                isCurrentlySpeaking = false;
                OnSpeakEnd?.Invoke(); // Treat error as end of speaking
                Debug.Log("TTSManager: OnSpeakEnd Invoked due to error.");
             } else { Debug.Log("TTSManager: Playback Error event received, but already idle. Ignoring duplicate end."); }
        }
        */

        // --- Optional Voice Selection Methods ---
        // public void SetVoice(string voicePresetID) { /* ... */ }
        // private TTSVoiceSettings FindVoiceSettingByID(string id) { /* ... */ }
}
