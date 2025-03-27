using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace SpeakEasy.UI
{
    public class UIController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private CanvasGroup welcomeGroup;
        [SerializeField] private CanvasGroup calibrationGroup;
        [SerializeField] private CanvasGroup sessionGroup;
        [SerializeField] private CanvasGroup completionGroup;
        [SerializeField] private CanvasGroup feedbackGroup;
        [SerializeField] private CanvasGroup dataRetentionGroup;
        
        [Header("Text Elements")]
        [SerializeField] private TextMeshProUGUI sessionNameText;
        [SerializeField] private TextMeshProUGUI stepNameText;
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private TextMeshProUGUI detailedInstructionsText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private TextMeshProUGUI calibrationInstructionText;
        
        [Header("UI Components")]
        [SerializeField] private Slider progressSlider;
        
        [Header("Audio")]
        [SerializeField] private AudioSource uiAudioSource;
        [SerializeField] private AudioClip positiveSound;
        [SerializeField] private AudioClip negativeSound;
        [SerializeField] private AudioClip notificationSound;
        
        [Header("Configuration")]
        [SerializeField] private float messageFadeDuration = 3f;
        
        private Coroutine currentFeedbackCoroutine;
        
        void Start()
        {
            Debug.Log("UIController initialized");
            ShowWelcomeScreen();
        }
        
        private void SetCanvasGroupActive(CanvasGroup group, bool active)
        {
            if (group == null) return;
            
            group.alpha = active ? 1f : 0f;
            group.interactable = active;
            group.blocksRaycasts = active;
        }
        
        private void SetActiveGroup(string groupName)
        {
            if (welcomeGroup != null) SetCanvasGroupActive(welcomeGroup, groupName == "welcome");
            if (calibrationGroup != null) SetCanvasGroupActive(calibrationGroup, groupName == "calibration");
            if (sessionGroup != null) SetCanvasGroupActive(sessionGroup, groupName == "session");
            if (completionGroup != null) SetCanvasGroupActive(completionGroup, groupName == "completion");
            if (dataRetentionGroup != null) SetCanvasGroupActive(dataRetentionGroup, groupName == "dataRetention");
        }
        
        public void ShowWelcomeScreen()
        {
            SetActiveGroup("welcome");
            ShowFeedback("Welcome to SpeakEasy MR. Say 'Begin Therapy' to start.");
        }
        
        public void ShowCalibrationScreen()
        {
            SetActiveGroup("calibration");
            if (calibrationInstructionText != null)
            {
                calibrationInstructionText.text = "Please read the following phrases clearly when prompted:";
            }
            ShowFeedback("Calibration mode. Please follow the on-screen instructions.");
        }
        
        public void ShowCalibrationComplete()
        {
            PlaySound(positiveSound);
            ShowFeedback("Calibration complete! Your voice settings have been saved.");
        }
        
        public void ShowReadyToStartMessage()
        {
            ShowFeedback("Ready to begin. Say 'Start Therapy' when you're ready.");
        }
        
        public void UpdateSessionName(string name)
        {
            if (sessionNameText != null)
            {
                sessionNameText.text = name;
            }
        }
        
        public void ShowSessionStarted()
        {
            SetActiveGroup("session");
            PlaySound(positiveSound);
            ShowFeedback("Therapy session started. Follow the instructions for each step.");
        }
        
        public void UpdateStepInformation(string stepName, string instruction, string detailedInstructions, int currentStep, int totalSteps)
        {
            if (stepNameText != null) stepNameText.text = stepName;
            if (instructionText != null) instructionText.text = instruction;
            if (detailedInstructionsText != null) detailedInstructionsText.text = detailedInstructions;
            if (progressText != null) progressText.text = $"Step {currentStep} of {totalSteps}";
            if (progressSlider != null) progressSlider.value = (float)currentStep / totalSteps;
            
            PlaySound(notificationSound);
            ShowFeedback($"Step {currentStep}: {stepName}");
        }
        
        public void ShowRepeatingStep()
        {
            PlaySound(notificationSound);
            ShowFeedback("Repeating current step instructions.");
        }
        
        public void ShowReadyForNextPrompt()
        {
            ShowFeedback("You can now proceed to the next step. Say 'Next' when ready.");
        }
        
        public void ShowSessionComplete()
        {
            SetActiveGroup("completion");
            
            if (progressSlider != null)
            {
                progressSlider.value = 1f;
            }
            
            PlaySound(positiveSound);
            ShowFeedback("Therapy session complete! Great job!");
        }
        
        public void ShowDataRetentionPrompt()
        {
            SetActiveGroup("dataRetention");
            ShowFeedback("Would you like to save your voice data for improving future sessions?");
        }
        
        public void ShowUnrecognizedCommand(string transcription)
        {
            PlaySound(negativeSound);
            ShowFeedback($"I didn't understand '{transcription}'. Please try again with a valid command.");
        }
        
        public void ShowUnrecognizedCommandWithSuggestion(string transcription, string suggestion)
        {
            PlaySound(negativeSound);
            ShowFeedback($"I heard '{transcription}'. {suggestion}");
        }
        
        public void ShowMessage(string message)
        {
            ShowFeedback(message);
        }
        
        private void ShowFeedback(string message)
        {
            if (feedbackText != null) feedbackText.text = message;
            
            // Show feedback panel
            if (feedbackGroup != null)
            {
                SetCanvasGroupActive(feedbackGroup, true);
                
                // Cancel existing fade coroutine if any
                if (currentFeedbackCoroutine != null)
                {
                    StopCoroutine(currentFeedbackCoroutine);
                }
                
                // Start new fade coroutine
                currentFeedbackCoroutine = StartCoroutine(HideFeedbackAfterDelay(messageFadeDuration));
            }
        }
        
        private IEnumerator HideFeedbackAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (feedbackGroup != null)
            {
                SetCanvasGroupActive(feedbackGroup, false);
            }
        }
        
        private void PlaySound(AudioClip clip)
        {
            if (uiAudioSource != null && clip != null)
            {
                uiAudioSource.PlayOneShot(clip);
            }
        }
    }
}