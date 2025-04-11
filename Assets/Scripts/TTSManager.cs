using UnityEngine;
using Meta.WitAi.TTS.Utilities; // Use the namespace where TTSSpeaker is located
using Meta.WitAi.TTS.Data; // Use the namespace for TTSVoiceSettings if needed for voice selection later

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
    [SerializeField] private TTSSpeaker speaker; // *** IMPORTANT: Assign this in the Inspector ***

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

    // --- Optional Voice Selection Methods (Example) ---

    // /// <summary>
    // /// Sets the active voice preset for the TTSSpeaker.
    // /// </summary>
    // /// <param name="voicePresetID">The PresetID of the desired TTSVoiceSettings asset.</param>
    // public void SetVoice(string voicePresetID)
    // {
    //     if (speaker == null) return;
    //     if (string.IsNullOrEmpty(voicePresetID)) return;

    //     // Find the voice settings asset (implementation depends on how you store/manage them)
    //     TTSVoiceSettings selectedSetting = FindVoiceSettingByID(voicePresetID); // You'd need to implement this lookup

    //     if (selectedSetting != null)
    //     {
    //         speaker.VoiceSettings = selectedSetting; // Assign the found settings
    //         selectedVoiceID = voicePresetID;
    //         Debug.Log($"TTSManager: Voice set to {voicePresetID}");
    //     }
    //     else
    //     {
    //         Debug.LogWarning($"TTSManager: Voice preset with ID '{voicePresetID}' not found.");
    //     }
    // }

    // /// <summary>
    // /// Placeholder for finding voice settings by ID.
    // /// </summary>
    // private TTSVoiceSettings FindVoiceSettingByID(string id)
    // {
    //     if (availableVoices == null) return null;
    //     foreach (var voice in availableVoices)
    //     {
    //         if (voice != null && voice.PresetId == id)
    //         {
    //             return voice;
    //         }
    //     }
    //     return null; // Not found
    // }

    // --- Optional Event Handlers ---

    // private void HandlePlaybackComplete()
    // {
    //     Debug.Log("TTSManager: Playback Complete.");
    //     // Unsubscribe or handle completion logic
    //     // speaker.Events.OnPlaybackComplete.RemoveListener(HandlePlaybackComplete);
    // }

    // private void HandleSpeechError(string code, string message)
    // {
    //      Debug.LogError($"TTSManager: Speech Error - Code: {code}, Message: {message}");
    //      // Unsubscribe or handle error logic
    //      // speaker.Events.OnError.RemoveListener(HandleSpeechError);
    // }
}
