using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Manages the main instruction panel that displays session instructions to the user.
/// Provides high visibility text with accessibility features.
/// </summary>
public class InstructionPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private Image panelBackground;
    
    [Header("Accessibility Settings")]
    [SerializeField] private float defaultFontSize = 0.05f;
    [SerializeField] private Color defaultTextColor = Color.white; // Renamed for clarity
    [SerializeField] private Color defaultBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Renamed
    
    [SerializeField] private float panelWidth = 0.6f; 
    [SerializeField] private float panelHeight = 0.3f;

    [Space(10)] // Add some space in Inspector
    [SerializeField] private bool useHighContrast = false; // Toggle for high contrast
    [SerializeField] private Color highContrastTextColor = Color.yellow; // Example high contrast text
    [SerializeField] private Color highContrastBackgroundColor = Color.black; // Example high contrast background
    
    [Header("Animation")]
    [SerializeField] private bool animateOnTextChange = true;
    [SerializeField] private float animationDuration = 0.3f;
    
    private string currentInstruction;
    private RectTransform panelRect;
    
    void Awake()
    {
        panelRect = GetComponent<RectTransform>();
        
        // Apply initial settings
        ApplyAccessibilitySettings();
        SetPanelSize();
    }
    
    /// <summary>
    /// Updates the instruction text with optional animation
    /// </summary>
    /// <param name="newInstruction">The instruction text to display</param>
    public void UpdateInstructionText(string newInstruction)
    {
        if (instructionText == null)
            return;
            
        currentInstruction = newInstruction;
        
        if (animateOnTextChange)
        {
            // Could implement a fade or scale animation here
            instructionText.text = newInstruction;
        }
        else
        {
            instructionText.text = newInstruction;
        }
    }
    
    /// <summary>
    /// Applies the configured accessibility settings to the panel
    /// </summary>
    public void ApplyAccessibilitySettings()
    {
        if (instructionText != null)
        {
            instructionText.fontSize = defaultFontSize; // Keep applying default font size for now
            // Apply color based on contrast mode
            instructionText.color = useHighContrast ? highContrastTextColor : defaultTextColor;
            Debug.Log($"InstructionPanel: Applied settings. High Contrast: {useHighContrast}, Text Color: {instructionText.color}"); // Added Log
        }
        else { Debug.LogWarning("InstructionPanel: instructionText reference is null."); }

        if (panelBackground != null)
        {
            // Apply background color based on contrast mode
            panelBackground.color = useHighContrast ? highContrastBackgroundColor : defaultBackgroundColor;
            Debug.Log($"InstructionPanel: Applied settings. High Contrast: {useHighContrast}, Background Color: {panelBackground.color}"); // Added Log
        }
        else { Debug.LogWarning("InstructionPanel: panelBackground reference is null."); }
    }

    /// <summary>
    /// Sets the high contrast mode and immediately applies the settings.
    /// </summary>
    /// <param name="isEnabled">True to enable high contrast, false to disable.</param>
    public void SetHighContrastMode(bool isEnabled)
    {
        useHighContrast = isEnabled;
        Debug.Log($"InstructionPanel: High contrast mode set to {isEnabled}. Applying settings...");
        ApplyAccessibilitySettings(); // Re-apply settings to update visuals
    }
    
    /// <summary>
    /// Updates the font size for better visibility
    /// </summary>
    /// <param name="size">New font size</param>
    public void SetFontSize(float size)
    {
        if (instructionText != null)
        {
            instructionText.fontSize = size;
        }
    }
    
    /// <summary>
    /// Sets the panel size based on configuration
    /// </summary>
    private void SetPanelSize()
    {
        if (panelRect != null)
        {
            panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);
        }
    }
    
    /// <summary>
    /// Positions the panel in front of the camera at the specified distance
    /// </summary>
    /// <param name="distance">Distance from camera</param>
    public void PositionInFrontOfCamera(float distance)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
            return;
            
        Vector3 position = mainCamera.transform.position + mainCamera.transform.forward * distance;
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
    }
}