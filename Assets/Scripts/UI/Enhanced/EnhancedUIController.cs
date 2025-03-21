using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

namespace UI.Enhanced
{
    /// <summary>
    /// Enhanced UI controller for voice-driven therapy application.
    /// Manages all UI panels, states, and positioning in VR.
    /// </summary>
    public class EnhancedUIController : MonoBehaviour
    {
        [Header("Panel References")]
        [SerializeField] private GameObject welcomePanel;
        [SerializeField] private GameObject instructionPanel;
        [SerializeField] private GameObject statusPanel;
        [SerializeField] private GameObject commandPanel;
        [SerializeField] private GameObject progressPanel;
        [SerializeField] private GameObject manualControls;
        
        [Header("Welcome Panel Elements")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text instructionsText;
        [SerializeField] private Button startButton;
        [SerializeField] private Image welcomePanelBorder;
        
        [Header("Instruction Panel Elements")]
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private Image instructionPanelBorder;
        
        [Header("Status Indicator Elements")]
        [SerializeField] private Image statusBackground;
        [SerializeField] private Image micIcon;
        [SerializeField] private TMP_Text statusText;
        
        [Header("Command Panel Elements")]
        [SerializeField] private Transform commandsList;
        [SerializeField] private GameObject commandItemPrefab;
        
        [Header("Progress Panel Elements")]
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private Image progressBar;
        [SerializeField] private Image progressBarFill;
        
        [Header("Manual Controls")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button restartVoiceButton;
        [SerializeField] private Button endSessionButton;
        
        [Header("UI Colors")]
        [SerializeField] private Color activeColor = new Color(0.298f, 0.686f, 0.313f); // #4CAF50
        [SerializeField] private Color processingColor = new Color(1.0f, 0.922f, 0.231f); // #FFEB3B
        [SerializeField] private Color errorColor = new Color(0.957f, 0.263f, 0.212f); // #F44336
        [SerializeField] private Color idleColor = new Color(0.619f, 0.619f, 0.619f); // #9E9E9E
        [SerializeField] private Color accentColor = new Color(0.129f, 0.588f, 0.953f); // #2196F3
        
        [Header("Position Settings")]
        [SerializeField] private float defaultDistance = 2f;
        [SerializeField] private float eyeOffset = -0.1f; // Slightly below eye level
        [SerializeField] private float panelTiltAngle = 5f; // Slight upward tilt
        [SerializeField] private float sidePanelDistance = 0.35f; // 350mm from center
        [SerializeField] private float sidePanelAngle = 15f; // Angle inward
        [SerializeField] private float statusOffset = 0.1f; // 100mm below main panel
        [SerializeField] private float manualControlsOffset = 0.15f; // 150mm from bottom
        [SerializeField] private bool followUserGaze = true;
        [SerializeField] private float followSmoothness = 0.1f; // Lower = smoother follow
        
        [Header("Panel Dimensions")]
        [SerializeField] private Vector2 welcomePanelSize = new Vector2(0.8f, 0.6f); // 800x600mm
        [SerializeField] private Vector2 instructionPanelSize = new Vector2(0.8f, 0.6f); // 800x600mm
        [SerializeField] private Vector2 statusPanelSize = new Vector2(0.6f, 0.08f); // 600x80mm
        [SerializeField] private Vector2 commandPanelSize = new Vector2(0.35f, 0.5f); // 350x500mm
        [SerializeField] private Vector2 progressPanelSize = new Vector2(0.35f, 0.3f); // 350x300mm
        [SerializeField] private Vector2 buttonSize = new Vector2(0.18f, 0.06f); // 180x60mm
        [SerializeField] private Vector2 micIconSize = new Vector2(0.03f, 0.03f); // 30x30mm
        
        [Header("Typography Settings")]
        [SerializeField] private float titleFontSize = 60f;
        [SerializeField] private float instructionsFontSize = 40f;
        [SerializeField] private float promptFontSize = 60f;
        [SerializeField] private float statusFontSize = 30f;
        [SerializeField] private float commandFontSize = 28f;
        [SerializeField] private float buttonFontSize = 24f;
        
        [Header("Accessibility Settings")]
        [SerializeField] private bool highContrastMode = true;
        
        private Camera mainCamera;
        private SessionState currentState = SessionState.Idle;
        private VoiceRecognitionStatus currentVoiceStatus = VoiceRecognitionStatus.Idle;
        private Coroutine pulseCoroutine;
        private Vector3 targetPosition;
        private Quaternion targetRotation;
        
        private void Start()
        {
            mainCamera = Camera.main;
            
            // Initialize target position/rotation
            if (mainCamera != null)
            {
                targetPosition = mainCamera.transform.position + mainCamera.transform.forward * defaultDistance;
                targetRotation = Quaternion.LookRotation(targetPosition - mainCamera.transform.position);
            }
            
            // Apply panel dimensions
            ApplyPanelDimensions();
            
            // Position the UI in front of the camera
            PositionUIElements();
            
            // Apply accessibility settings
            ApplyAccessibilitySettings();
            
            // Setup UI state
            ShowWelcomeState();
            
            // Setup command items
            PopulateCommandsList();
            
            // Setup button listeners
            SetupButtonListeners();
        }
        
        private void Update()
        {
            if (followUserGaze && mainCamera != null)
            {
                // Calculate target position
                Vector3 forward = mainCamera.transform.forward;
                Vector3 up = mainCamera.transform.up;
                Vector3 newTargetPos = mainCamera.transform.position + 
                                      forward * defaultDistance + 
                                      up * eyeOffset;
                
                // Smooth movement with lerp
                targetPosition = Vector3.Lerp(targetPosition, newTargetPos, followSmoothness);
                
                // Calculate target rotation with tilt
                Quaternion baseLookRotation = Quaternion.LookRotation(targetPosition - mainCamera.transform.position);
                Quaternion tiltRotation = Quaternion.Euler(-panelTiltAngle, 0, 0);
                Quaternion newTargetRot = baseLookRotation * tiltRotation;
                
                // Smooth rotation with slerp
                targetRotation = Quaternion.Slerp(targetRotation, newTargetRot, followSmoothness);
                
                // Update position of UI elements
                PositionUIElements();
            }
        }
        
        #region UI Setup Methods
        
        private void ApplyPanelDimensions()
        {
            // Apply welcome panel dimensions
            ApplyRectTransformSize(welcomePanel, welcomePanelSize);
            
            // Apply instruction panel dimensions
            ApplyRectTransformSize(instructionPanel, instructionPanelSize);
            
            // Apply status panel dimensions
            ApplyRectTransformSize(statusPanel, statusPanelSize);
            
            // Apply command panel dimensions
            ApplyRectTransformSize(commandPanel, commandPanelSize);
            
            // Apply progress panel dimensions
            ApplyRectTransformSize(progressPanel, progressPanelSize);
            
            // Apply text dimensions and sizes
            if (titleText != null)
            {
                titleText.fontSize = titleFontSize;
                ApplyRectTransformSize(titleText.gameObject, new Vector2(welcomePanelSize.x * 0.975f, 0.08f)); // 780x80mm
            }
            
            if (instructionsText != null)
            {
                instructionsText.fontSize = instructionsFontSize;
                ApplyRectTransformSize(instructionsText.gameObject, new Vector2(welcomePanelSize.x * 0.95f, 0.12f)); // 760x120mm
            }
            
            if (promptText != null)
            {
                promptText.fontSize = promptFontSize;
                ApplyRectTransformSize(promptText.gameObject, new Vector2(instructionPanelSize.x * 0.95f, 0.2f)); // 760x200mm
            }
            
            if (statusText != null)
            {
                statusText.fontSize = statusFontSize;
            }
            
            if (progressBar != null)
            {
                ApplyRectTransformSize(progressBar.gameObject, new Vector2(0.3f, 0.015f)); // 300x15mm
            }
            
            // Apply button dimensions
            if (startButton != null)
            {
                ApplyRectTransformSize(startButton.gameObject, new Vector2(0.3f, 0.08f)); // 300x80mm
                
                // Set font size for button text
                TMP_Text buttonText = startButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.fontSize = buttonFontSize;
                }
            }
            
            // Apply manual control button dimensions
            Button[] controlButtons = { continueButton, restartVoiceButton, endSessionButton };
            foreach (Button button in controlButtons)
            {
                if (button != null)
                {
                    ApplyRectTransformSize(button.gameObject, buttonSize);
                    
                    TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
                    if (buttonText != null)
                    {
                        buttonText.fontSize = buttonFontSize;
                    }
                }
            }
            
            // Apply mic icon dimensions
            if (micIcon != null)
            {
                ApplyRectTransformSize(micIcon.gameObject, micIconSize);
            }
        }
        
        private void ApplyRectTransformSize(GameObject obj, Vector2 size)
        {
            if (obj == null) return;
            
            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.sizeDelta = size * 1000f; // Convert to mm
                rt.localScale = new Vector3(0.001f, 0.001f, 0.001f); // Convert to meters
            }
        }
        
        private void SetupButtonListeners()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartButtonClicked);
            }
            
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueButtonClicked);
            }
            
            if (restartVoiceButton != null)
            {
                restartVoiceButton.onClick.AddListener(OnRestartVoiceButtonClicked);
            }
            
            if (endSessionButton != null)
            {
                endSessionButton.onClick.AddListener(OnEndSessionButtonClicked);
            }
        }
        
        private void PopulateCommandsList()
        {
            if (commandsList == null || commandItemPrefab == null) return;
            
            // Clear existing items
            foreach (Transform child in commandsList)
            {
                Destroy(child.gameObject);
            }
            
            // Add command items
            CreateCommandItem("Begin", "Start the session");
            CreateCommandItem("Continue", "Next prompt");
            CreateCommandItem("Repeat", "Repeat current prompt");
            CreateCommandItem("End", "End the session");
        }
        
        private void CreateCommandItem(string command, string description)
        {
            if (commandsList == null || commandItemPrefab == null) return;
            
            GameObject item = Instantiate(commandItemPrefab, commandsList);
            RectTransform rt = item.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.sizeDelta = new Vector2(320f, 70f); // 320x70mm
            }
            
            // Set data if the prefab has the CommandItemPrefab script
            CommandItemPrefab itemScript = item.GetComponent<CommandItemPrefab>();
            if (itemScript != null)
            {
                itemScript.SetData(command, description, accentColor);
            }
            else
            {
                // Find and set text components manually if no script exists
                TMP_Text[] texts = item.GetComponentsInChildren<TMP_Text>();
                if (texts.Length >= 2)
                {
                    texts[0].text = command;
                    texts[0].fontSize = commandFontSize;
                    texts[0].fontStyle = FontStyles.Bold;
                    
                    texts[1].text = description;
                    texts[1].fontSize = commandFontSize * 0.8f;
                }
                
                // Set border color
                Image border = item.GetComponentInChildren<Image>();
                if (border != null)
                {
                    border.color = accentColor;
                }
            }
        }
        
        #endregion
        
        #region Positioning Methods
        
        public void PositionUIElements()
        {
            if (mainCamera == null) return;
            
            // Position and rotate the main panels based on current state
            if (currentState == SessionState.Idle)
            {
                // Position welcome panel
                if (welcomePanel != null && welcomePanel.activeSelf)
                {
                    welcomePanel.transform.position = targetPosition;
                    welcomePanel.transform.rotation = targetRotation;
                }
            }
            else
            {
                // Position instruction panel for active session
                if (instructionPanel != null && instructionPanel.activeSelf)
                {
                    instructionPanel.transform.position = targetPosition;
                    instructionPanel.transform.rotation = targetRotation;
                }
            }
            
            // Calculate vectors based on the rotation
            Vector3 right = targetRotation * Vector3.right;
            Vector3 up = targetRotation * Vector3.up;
            
            // Position status panel below main panel
            if (statusPanel != null)
            {
                Vector3 statusPos = targetPosition + (up * -statusOffset);
                statusPanel.transform.position = statusPos;
                statusPanel.transform.rotation = targetRotation;
            }
            
            // Position command panel on the right
            if (commandPanel != null)
            {
                Vector3 commandPos = targetPosition + (right * sidePanelDistance);
                commandPanel.transform.position = commandPos;
                
                // Angle the side panel slightly inward
                Quaternion sideRotation = Quaternion.LookRotation(commandPos - mainCamera.transform.position);
                sideRotation *= Quaternion.Euler(0, -sidePanelAngle, 0);
                commandPanel.transform.rotation = sideRotation;
            }
            
            // Position progress panel on the left
            if (progressPanel != null && currentState == SessionState.Active)
            {
                Vector3 progressPos = targetPosition + (right * -sidePanelDistance);
                progressPanel.transform.position = progressPos;
                
                // Angle the side panel slightly inward
                Quaternion sideRotation = Quaternion.LookRotation(progressPos - mainCamera.transform.position);
                sideRotation *= Quaternion.Euler(0, sidePanelAngle, 0);
                progressPanel.transform.rotation = sideRotation;
            }
            
            // Position manual controls at the bottom
            if (manualControls != null)
            {
                Vector3 controlsPos = targetPosition + (up * -manualControlsOffset);
                manualControls.transform.position = controlsPos;
                manualControls.transform.rotation = targetRotation;
            }
        }
        
        #endregion
        
        #region Visual State Methods
        
        public void ApplyAccessibilitySettings()
        {
            if (highContrastMode)
            {
                // Apply dark backgrounds with high contrast text
                Color darkBackground = new Color(0f, 0f, 0f, 0.8f);
                
                ApplyBackgroundColor(welcomePanel, darkBackground);
                ApplyBackgroundColor(instructionPanel, darkBackground);
                ApplyBackgroundColor(statusPanel, new Color(0f, 0f, 0f, 0.6f));
                ApplyBackgroundColor(commandPanel, new Color(0f, 0f, 0f, 0.7f));
                ApplyBackgroundColor(progressPanel, new Color(0f, 0f, 0f, 0.7f));
                
                // Apply white text for all text components
                ApplyTextColor(titleText, Color.white);
                ApplyTextColor(instructionsText, Color.white);
                ApplyTextColor(promptText, Color.white);
                ApplyTextColor(statusText, Color.white);
                ApplyTextColor(progressText, Color.white);
            }
        }
        
        private void ApplyBackgroundColor(GameObject panel, Color color)
        {
            if (panel == null) return;
            
            Image background = panel.GetComponent<Image>();
            if (background != null)
            {
                background.color = color;
            }
        }
        
        private void ApplyTextColor(TMP_Text text, Color color)
        {
            if (text != null)
            {
                text.color = color;
            }
        }
        
        public void UpdateVoiceStatus(VoiceRecognitionStatus status, string statusMessage)
        {
            currentVoiceStatus = status;
            
            // Update status message
            if (statusText != null)
            {
                statusText.text = statusMessage;
            }
            
            Color statusColor;
            bool enablePulse = false;
            
            // Determine color and animation based on status
            switch (status)
            {
                case VoiceRecognitionStatus.Listening:
                    statusColor = activeColor;
                    enablePulse = true;
                    break;
                case VoiceRecognitionStatus.Processing:
                    statusColor = processingColor;
                    enablePulse = true;
                    break;
                case VoiceRecognitionStatus.Error:
                    statusColor = errorColor;
                    enablePulse = false;
                    break;
                case VoiceRecognitionStatus.Idle:
                default:
                    statusColor = idleColor;
                    enablePulse = false;
                    break;
            }
            
            // Apply colors to status elements
            if (statusBackground != null)
            {
                statusBackground.color = new Color(0, 0, 0, 0.6f);
                
                // Apply border color - assuming we're using a border image
                Image border = statusBackground.transform.GetChild(0)?.GetComponent<Image>();
                if (border != null)
                {
                    border.color = statusColor;
                }
            }
            
            if (micIcon != null)
            {
                micIcon.color = statusColor;
                
                // Toggle pulse animation via MicPulseEffect if available
                MicPulseEffect pulseEffect = micIcon.GetComponent<MicPulseEffect>();
                if (pulseEffect != null)
                {
                    pulseEffect.SetPulsing(enablePulse);
                }
                else if (enablePulse)
                {
                    // Use coroutine as fallback
                    StartPulseEffect(micIcon);
                }
                else
                {
                    StopPulseEffect();
                }
            }
            
            // Update border colors on main panel based on status
            UpdatePanelBorders(status);
            
            // Update command item appearance based on state
            UpdateCommandHighlights(status);
        }
        
        private void UpdatePanelBorders(VoiceRecognitionStatus status)
        {
            Color borderColor;
            
            switch (status)
            {
                case VoiceRecognitionStatus.Listening:
                    borderColor = activeColor;
                    break;
                case VoiceRecognitionStatus.Processing:
                    borderColor = processingColor;
                    break;
                case VoiceRecognitionStatus.Error:
                    borderColor = errorColor;
                    break;
                case VoiceRecognitionStatus.Idle:
                default:
                    borderColor = idleColor;
                    break;
            }
            
            // Update border of the active panel
            if (currentState == SessionState.Idle && welcomePanelBorder != null)
            {
                welcomePanelBorder.color = borderColor;
            }
            else if (currentState == SessionState.Active && instructionPanelBorder != null)
            {
                instructionPanelBorder.color = borderColor;
            }
        }
        
        private void UpdateCommandHighlights(VoiceRecognitionStatus status)
        {
            if (commandsList == null) return;
            
            // Get all command items
            CommandItemPrefab[] items = commandsList.GetComponentsInChildren<CommandItemPrefab>();
            if (items == null || items.Length == 0) return;
            
            // Dim all command items when processing
            bool dimItems = (status == VoiceRecognitionStatus.Processing);
            
            foreach (CommandItemPrefab item in items)
            {
                if (item == null) continue;
                
                // Dim or un-dim based on processing state
                item.SetDimmed(dimItems);
                
                // Highlight the relevant command based on the current session state
                string commandText = item.GetCommandText();
                bool highlight = false;
                
                if (currentState == SessionState.Idle && 
                    (commandText.Contains("Begin") || commandText.Contains("Start")))
                {
                    highlight = true;
                }
                else if (currentState == SessionState.Active && commandText.Contains("Continue"))
                {
                    highlight = true;
                }
                
                item.SetHighlighted(highlight);
            }
        }
        
        private void StartPulseEffect(Image targetImage)
        {
            if (targetImage == null) return;
            
            StopPulseEffect();
            pulseCoroutine = StartCoroutine(PulseRoutine(targetImage));
        }
        
        private void StopPulseEffect()
        {
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }
            
            // Reset alpha on mic icon
            if (micIcon != null)
            {
                Color color = micIcon.color;
                micIcon.color = new Color(color.r, color.g, color.b, 1.0f);
            }
        }
        
        private IEnumerator PulseRoutine(Image targetImage)
        {
            float t = 0;
            float pulseSpeed = 1.5f;
            float minAlpha = 0.6f;
            float maxAlpha = 1.0f;
            
            while (true)
            {
                float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(t * pulseSpeed) + 1) / 2);
                
                Color color = targetImage.color;
                targetImage.color = new Color(color.r, color.g, color.b, alpha);
                
                t += Time.deltaTime;
                yield return null;
            }
        }
        
        #endregion
        
        #region UI State Management
        
        public void ShowWelcomeState()
        {
            currentState = SessionState.Idle;
            
            // Setup panel visibility
            ShowElement(welcomePanel);
            HideElement(instructionPanel);
            ShowElement(statusPanel);
            ShowElement(commandPanel);
            HideElement(progressPanel);
            
            // Only show manual controls if in active session
            HideElement(manualControls);
            
            // Set welcome text
            if (titleText != null)
            {
                titleText.text = "Welcome to Voice-Driven Therapy";
            }
            
            if (instructionsText != null)
            {
                instructionsText.text = "Say \"Start therapy\" or \"Begin session\" to start.";
            }
            
            // Update status
            UpdateVoiceStatus(VoiceRecognitionStatus.Idle, "Voice Recognition Waiting - Say \"Begin\" to start");
            
            // Update position
            PositionUIElements();
        }
        
        public void ShowActiveState(string promptMessage)
        {
            currentState = SessionState.Active;
            
            // Setup panel visibility
            HideElement(welcomePanel);
            ShowElement(instructionPanel);
            ShowElement(statusPanel);
            ShowElement(commandPanel);
            ShowElement(progressPanel);
            ShowElement(manualControls);
            
            // Set prompt text
            if (promptText != null)
            {
                promptText.text = promptMessage;
            }
            
            // Update status
            UpdateVoiceStatus(VoiceRecognitionStatus.Listening, "Voice Recognition Active - Say \"Continue\" when ready");
            
            // Re-enable buttons
            if (continueButton != null) continueButton.interactable = true;
            if (restartVoiceButton != null) restartVoiceButton.interactable = true;
            if (endSessionButton != null) endSessionButton.interactable = true;
            
            // Update position
            PositionUIElements();
        }
        
        public void ShowProcessingState(string transcription)
        {
            // Don't change the current state, just the visual appearance
            
            // Update status and colors
            UpdateVoiceStatus(VoiceRecognitionStatus.Processing, "Processing Voice Command...");
            
            // Disable buttons except emergency end button
            if (continueButton != null) continueButton.interactable = false;
            if (restartVoiceButton != null) restartVoiceButton.interactable = false;
            if (endSessionButton != null) endSessionButton.interactable = true; // Keep emergency button active
            
            // Show transcription if we have a dedicated area for it
            // You could add a transcription text object to the instruction panel
        }
        
        public void ShowErrorState(string errorMessage)
        {
            // Update status and colors
            UpdateVoiceStatus(VoiceRecognitionStatus.Error, "Voice Recognition Error - Please try manual controls");
            
            // Re-enable buttons
            if (continueButton != null) continueButton.interactable = true;
            if (restartVoiceButton != null) restartVoiceButton.interactable = true;
            if (endSessionButton != null) endSessionButton.interactable = true;
            
            // Show error message
            // You could display the error in a dedicated area or use a popup
        }
        
        public void UpdateProgressBar(int currentStep, int totalSteps)
        {
            if (progressText != null)
            {
                progressText.text = $"Prompt: {currentStep} of {totalSteps}";
            }
            
            if (progressBarFill != null)
            {
                float progress = (float)currentStep / totalSteps;
                progressBarFill.fillAmount = progress;
            }
        }
        
        #endregion
        
        #region Button Handlers
        
        private void OnStartButtonClicked()
        {
            // Find and call SessionController.StartSession()
            SessionController sessionController = FindObjectOfType<SessionController>();
            if (sessionController != null)
            {
                sessionController.StartSession();
            }
        }
        
        private void OnContinueButtonClicked()
        {
            // Find and call SessionController.AdvanceToNextStep()
            SessionController sessionController = FindObjectOfType<SessionController>();
            if (sessionController != null)
            {
                sessionController.AdvanceToNextStep();
            }
        }
        
        private void OnRestartVoiceButtonClicked()
        {
            // Find and call VoiceCommandManager to restart voice recognition
            VoiceCommandManager voiceManager = FindObjectOfType<VoiceCommandManager>();
            if (voiceManager != null)
            {
                // Try to call RestartVoiceRecognition if it exists
                voiceManager.SendMessage("RestartVoiceRecognition", null, SendMessageOptions.DontRequireReceiver);
            }
            
            // Update status
            UpdateVoiceStatus(VoiceRecognitionStatus.Listening, "Voice Recognition Restarted - Please try again");
        }
        
        private void OnEndSessionButtonClicked()
        {
            // Find and call SessionController.EndSession()
            SessionController sessionController = FindObjectOfType<SessionController>();
            if (sessionController != null)
            {
                sessionController.EndSession();
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        public void ShowElement(GameObject element)
        {
            if (element != null)
            {
                element.SetActive(true);
            }
        }
        
        public void HideElement(GameObject element)
        {
            if (element != null)
            {
                element.SetActive(false);
            }
        }
        
        public void UpdatePromptText(string text)
        {
            if (promptText != null)
            {
                promptText.text = text;
            }
        }
        
        public void UpdateStatusText(string text)
        {
            if (statusText != null)
            {
                statusText.text = text;
            }
        }
        
        #endregion
    }
}