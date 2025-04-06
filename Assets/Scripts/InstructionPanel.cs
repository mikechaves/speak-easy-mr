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
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    [SerializeField] private float panelWidth = 0.6f;
    [SerializeField] private float panelHeight = 0.3f;
    
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
            instructionText.fontSize = defaultFontSize;
            instructionText.color = textColor;
        }
        
        if (panelBackground != null)
        {
            panelBackground.color = backgroundColor;
        }
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