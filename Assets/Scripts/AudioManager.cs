using UnityEngine;
using UnityEngine.Audio; // Optional: if using Audio Mixers

/// <summary>
/// Manages background music playback and one-shot UI sounds like command confirmations.
/// Uses Singleton pattern for easy access.
/// </summary>
[RequireComponent(typeof(AudioSource), typeof(AudioSource))] // Ensure two AudioSources exist
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField]
    [Tooltip("AudioSource for looping background music.")]
    private AudioSource backgroundMusicSource;

    [SerializeField]
    [Tooltip("AudioSource for one-shot UI sounds (e.g., confirmation chime).")]
    private AudioSource uiSoundSource;

    [Header("Audio Clips")]
    [SerializeField]
    [Tooltip("The background music track to loop.")]
    private AudioClip backgroundMusicClip;

    [SerializeField]
    [Tooltip("The short sound to play upon successful voice command execution.")]
    private AudioClip confirmationSoundClip;

    // Optional: If using Audio Mixers
    // [Header("Audio Mixers")]
    // [SerializeField] private AudioMixerGroup musicMixerGroup;
    // [SerializeField] private AudioMixerGroup sfxMixerGroup;

    void Awake()
    {
        // Singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("AudioManager: Another instance found, destroying this one.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Optional: Persist across scenes if needed
        // DontDestroyOnLoad(gameObject);

        // Basic validation and setup
        if (backgroundMusicSource == null || uiSoundSource == null)
        {
            AudioSource[] sources = GetComponents<AudioSource>();
            if (sources.Length >= 2)
            {
                if (backgroundMusicSource == null) backgroundMusicSource = sources[0];
                if (uiSoundSource == null) uiSoundSource = sources[1];
                Debug.LogWarning("AudioManager: AudioSources not assigned, attempting to find them on the GameObject.");
            }
            else
            {
                Debug.LogError("AudioManager: Requires two AudioSource components. Please assign them in the Inspector or ensure they exist on the GameObject.");
                return; // Stop further setup if sources are missing
            }
        }

        // Configure sources
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.playOnAwake = false;
            backgroundMusicSource.loop = true;
            // Optional: Assign mixer group
            // if (musicMixerGroup != null) backgroundMusicSource.outputAudioMixerGroup = musicMixerGroup;
        }

        if (uiSoundSource != null)
        {
            uiSoundSource.playOnAwake = false;
            uiSoundSource.loop = false;
            // Optional: Assign mixer group
            // if (sfxMixerGroup != null) uiSoundSource.outputAudioMixerGroup = sfxMixerGroup;
        }
    }

    /// <summary>
    /// Starts playing the assigned background music clip on loop.
    /// Should be called by SessionController when a session starts.
    /// </summary>
    public void StartBackgroundMusic()
    {
        if (backgroundMusicSource != null && backgroundMusicClip != null)
        {
            if (!backgroundMusicSource.isPlaying)
            {
                backgroundMusicSource.clip = backgroundMusicClip;
                backgroundMusicSource.Play();
                Debug.Log("AudioManager: Starting background music.");
            }
        }
        else
        {
            Debug.LogWarning("AudioManager: Cannot start background music. Source or Clip not assigned.");
        }
    }

    /// <summary>
    /// Stops the background music playback.
    /// Should be called by SessionController when a session ends.
    /// </summary>
    public void StopBackgroundMusic()
    {
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Stop();
            Debug.Log("AudioManager: Stopping background music.");
        }
    }

    /// <summary>
    /// Plays the assigned confirmation sound once.
    /// Should be called by VoiceCommandManager upon successful command execution.
    /// </summary>
    public void PlayConfirmationSound()
    {
        if (uiSoundSource != null && confirmationSoundClip != null)
        {
            uiSoundSource.PlayOneShot(confirmationSoundClip);
            // Debug.Log("AudioManager: Playing confirmation sound."); // Optional: Can be noisy
        }
        else
        {
            Debug.LogWarning("AudioManager: Cannot play confirmation sound. UI Source or Confirmation Clip not assigned.");
        }
    }

    // Optional: Add methods for volume control, fading, etc. if needed
}
