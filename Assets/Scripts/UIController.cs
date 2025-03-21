using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject instructionPanel;
    [SerializeField] private GameObject statusPanel;
    [SerializeField] private GameObject feedbackPanel;
    
    [Header("Text References")]
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private TMP_Text statusText;
    
    [Header("Visual Elements")]
    [SerializeField] private Image statusIndicator;
    [SerializeField] private GameObject breathingVisualizer;
    [SerializeField] private GameObject visualizationEnvironment;
    [SerializeField] private GameObject affirmationsDisplay;
    
    [Header("Position Settings")]
    [SerializeField] private float defaultDistance = 2f;
    [SerializeField] private float panelHeight = 1f;
    [SerializeField] private bool followUserGaze = true;
    
    [Header("Accessibility Settings")]
    [SerializeField] private float textSize = 0.05f;
    [SerializeField] private float contrastLevel = 1f;
    [SerializeField] private bool highContrastMode = false;
    
    private Camera mainCamera;
    private Vector3 defaultPosition;
    
    private void Start()
    {
        mainCamera = Camera.main;
        
        // Position the UI in front of the camera
        PositionUIElements();
        
        // Apply accessibility settings
        ApplyAccessibilitySettings();
        
        // Hide elements that should be inactive at start
        HideStepSpecificElements();
    }
    
    private void Update()
    {
        if (followUserGaze)
        {
            // Update position to follow user's gaze
            PositionUIElements();
        }
    }
    
    public void PositionUIElements()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // Position the UI panels in front of the user's view
        Vector3 forwardDirection = mainCamera.transform.forward;
        Vector3 position = mainCamera.transform.position + forwardDirection * defaultDistance;
        
        // Position main instruction panel
        if (instructionPanel != null)
        {
            instructionPanel.transform.position = position;
            instructionPanel.transform.rotation = Quaternion.LookRotation(instructionPanel.transform.position - mainCamera.transform.position);
        }
        
        // Position status panel slightly below
        if (statusPanel != null)
        {
            statusPanel.transform.position = position + Vector3.down * panelHeight;
            statusPanel.transform.rotation = Quaternion.LookRotation(statusPanel.transform.position - mainCamera.transform.position);
        }
        
        // Position feedback panel slightly above
        if (feedbackPanel != null)
        {
            feedbackPanel.transform.position = position + Vector3.up * panelHeight;
            feedbackPanel.transform.rotation = Quaternion.LookRotation(feedbackPanel.transform.position - mainCamera.transform.position);
        }
    }
    
    public void ApplyAccessibilitySettings()
    {
        // Apply text size to all text elements
        if (instructionText != null)
        {
            instructionText.fontSize = textSize;
        }
        
        if (statusText != null)
        {
            statusText.fontSize = textSize * 0.8f; // Slightly smaller for status
        }
        
        // Apply contrast settings
        if (highContrastMode)
        {
            // Apply high contrast settings to UI elements
            if (instructionPanel != null)
            {
                Image panelImage = instructionPanel.GetComponent<Image>();
                if (panelImage != null)
                {
                    panelImage.color = new Color(0f, 0f, 0f, 0.8f); // Dark background
                }
            }
            
            // Set text to white for high contrast
            if (instructionText != null)
            {
                instructionText.color = Color.white;
            }
            
            if (statusText != null)
            {
                statusText.color = Color.white;
            }
        }
    }
    
    public void HideStepSpecificElements()
    {
        // Hide elements that should only appear during specific steps
        if (breathingVisualizer != null)
        {
            breathingVisualizer.SetActive(false);
        }
        
        if (visualizationEnvironment != null)
        {
            visualizationEnvironment.SetActive(false);
        }
        
        if (affirmationsDisplay != null)
        {
            affirmationsDisplay.SetActive(false);
        }
    }
    
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
    
    public void UpdateInstructionText(string text)
    {
        if (instructionText != null)
        {
            instructionText.text = text;
        }
    }
    
    public void UpdateStatusText(string text)
    {
        if (statusText != null)
        {
            statusText.text = text;
        }
    }
}