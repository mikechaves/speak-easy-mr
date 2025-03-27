using UnityEngine;
using UnityEngine.Events;

namespace SpeakEasy.VoiceControl
{
    public class VoiceCommandManager : MonoBehaviour
    {
        [Header("Command Recognition")]
        [SerializeField] private float confidenceThreshold = 0.7f;
        
        [Header("Feedback")]
        [SerializeField] private AudioSource feedbackAudioSource;
        [SerializeField] private AudioClip commandRecognizedClip;
        [SerializeField] private AudioClip commandNotRecognizedClip;
        
        [Header("Events")]
        public UnityEvent OnStartTherapyCommand;
        public UnityEvent OnNextStepCommand;
        public UnityEvent OnRepeatCommand;
        public UnityEvent OnEndSessionCommand;
        public UnityEvent OnCalibrationComplete;
        public UnityEvent<string> OnCommandRecognized;
        public UnityEvent<string> OnCommandNotRecognized;
        
        private Meta.WitAi.Wit wit;
        
        void Awake()
        {
            // Ensure we have access to Wit.ai
            wit = GetComponent<Meta.WitAi.Wit>();
            if (wit == null)
            {
                Debug.LogError("No Wit component found on VoiceCommandManager! Please add a Wit component.");
                enabled = false;
                return;
            }
        }
        
        void Start()
        {
            Debug.Log("VoiceCommandManager initialized");
        }
        
        public void BeginCalibration()
        {
            Debug.Log("Starting calibration...");
            // In a real implementation, this would handle voice calibration
            // For MVP, we'll just simulate completion after a delay
            Invoke("CompleteCalibration", 2f);
        }
        
        private void CompleteCalibration()
        {
            Debug.Log("Calibration complete!");
            OnCalibrationComplete?.Invoke();
        }
        
        // Simulation methods for keyboard testing
        public void SimulateStartCommand()
        {
            Debug.Log("Simulating 'Start Therapy' command");
            OnStartTherapyCommand?.Invoke();
            OnCommandRecognized?.Invoke("start");
        }
        
        public void SimulateNextCommand()
        {
            Debug.Log("Simulating 'Next Step' command");
            OnNextStepCommand?.Invoke();
            OnCommandRecognized?.Invoke("next");
        }
        
        public void SimulateRepeatCommand()
        {
            Debug.Log("Simulating 'Repeat' command");
            OnRepeatCommand?.Invoke();
            OnCommandRecognized?.Invoke("repeat");
        }
        
        public void SimulateEndCommand()
        {
            Debug.Log("Simulating 'End Session' command");
            OnEndSessionCommand?.Invoke();
            OnCommandRecognized?.Invoke("end");
        }
    }
}