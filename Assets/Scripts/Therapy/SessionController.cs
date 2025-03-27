using UnityEngine;
using System.Collections;
using System;
using SpeakEasy.VoiceControl;
using SpeakEasy.UI;

namespace SpeakEasy.Therapy
{
    [Serializable]
    public class TherapyStep
    {
        public string stepName;
        public string instruction;
        [TextArea(3, 10)]
        public string detailedInstructions;
        public float minimumStepDuration = 5f; 
    }

    public class SessionController : MonoBehaviour
    {
        [Header("Session Configuration")]
        [SerializeField] private string sessionName = "Voice Therapy Session";
        [SerializeField] private TherapyStep[] therapySteps;
        [SerializeField] private bool requireCalibration = true;
        
        [Header("Dependencies")]
        [SerializeField] private VoiceCommandManager voiceManager;
        [SerializeField] private UIController uiController;
        [SerializeField] private AudioSource instructionAudioSource;
        
        // Session state
        private bool isSessionActive = false;
        private bool isCalibrated = false;
        private int currentStepIndex = -1;
        private float currentStepStartTime;
        private bool canAdvanceStep = false;
        
        void Awake()
        {
            if (voiceManager == null)
            {
                voiceManager = FindObjectOfType<VoiceCommandManager>();
                if (voiceManager == null)
                {
                    Debug.LogError("No VoiceCommandManager found in scene. Voice commands will not work.");
                }
            }
            
            if (uiController == null)
            {
                uiController = FindObjectOfType<UIController>();
                if (uiController == null)
                {
                    Debug.LogError("No UIController found in scene. UI feedback will not work.");
                }
            }
        }
        
        void Start()
        {
            if (voiceManager != null)
            {
                voiceManager.OnStartTherapyCommand.AddListener(StartSession);
                voiceManager.OnNextStepCommand.AddListener(AdvanceToNextStep);
                voiceManager.OnRepeatCommand.AddListener(RepeatCurrentStep);
                voiceManager.OnEndSessionCommand.AddListener(EndSession);
                voiceManager.OnCalibrationComplete.AddListener(OnCalibrationCompleted);
            }
            
            if (uiController != null)
            {
                uiController.UpdateSessionName(sessionName);
            }
            
            if (requireCalibration && voiceManager != null)
            {
                StartCalibration();
            }
            else
            {
                isCalibrated = true;
                if (uiController != null)
                {
                    uiController.ShowReadyToStartMessage();
                }
            }
        }
        
        // Other methods remain the same
        public void StartCalibration()
        {
            if (voiceManager != null)
            {
                if (uiController != null)
                {
                    uiController.ShowCalibrationScreen();
                }
                
                voiceManager.BeginCalibration();
            }
        }
        
        private void OnCalibrationCompleted()
        {
            isCalibrated = true;
            if (uiController != null)
            {
                uiController.ShowCalibrationComplete();
                uiController.ShowReadyToStartMessage();
            }
        }
        
        public void StartSession()
        {
            if (!isCalibrated && requireCalibration)
            {
                if (uiController != null)
                {
                    uiController.ShowMessage("Please complete calibration first.");
                }
                return;
            }
            
            if (isSessionActive)
            {
                return;
            }
            
            isSessionActive = true;
            currentStepIndex = -1;
            
            if (uiController != null)
            {
                uiController.ShowSessionStarted();
            }
            
            AdvanceToNextStep();
        }
        
        public void AdvanceToNextStep()
        {
            // Implementation unchanged
        }
        
        public void RepeatCurrentStep()
        {
            // Implementation unchanged
        }
        
        public void EndSession()
        {
            // Implementation unchanged
        }
    }
}