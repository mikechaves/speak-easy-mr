using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

/// <summary>
/// Manages the privacy settings panel that allows users to control
/// voice processing options and data sharing preferences.
/// </summary>
public class PrivacyPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Toggle localProcessingToggle;
    [SerializeField] private Toggle dataSharingToggle;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button closeButton;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    
    [Header("Settings")]
    [SerializeField] private bool defaultLocalProcessing = true;
    [SerializeField] private bool defaultDataSharing = false;
    [SerializeField] private bool isLocalProcessingAvailable = true;
    
    // Events
    public event Action<bool> OnLocalProcessingChanged;
    public event Action<bool> OnDataSharingChanged;
    
    private void Awake()
    {
        // Set up toggle listeners
        if (localProcessingToggle != null)
        {
            localProcessingToggle.onValueChanged.AddListener(OnLocalProcessingToggled);
            localProcessingToggle.isOn = defaultLocalProcessing;
            
            // Disable if not available on this platform
            localProcessingToggle.interactable = isLocalProcessingAvailable;
        }
        
        if (dataSharingToggle != null)
        {
            dataSharingToggle.onValueChanged.AddListener(OnDataSharingToggled);
            dataSharingToggle.isOn = defaultDataSharing;
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HidePanel);
        }
        
        // Initially hide the panel
        HidePanel();
        
        // Update initial status text
        UpdateStatusText();
    }
    
    /// <summary>
    /// Shows the privacy settings panel with a fade animation
    /// </summary>
    public void ShowPanel()
    {
        gameObject.SetActive(true);
        
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 1f;
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.blocksRaycasts = true;
        }
    }
    
    /// <summary>
    /// Hides the privacy settings panel
    /// </summary>
    public void HidePanel()
    {
        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
        }
        
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Toggles the panel visibility
    /// </summary>
    public void TogglePanel()
    {
        if (gameObject.activeSelf)
        {
            HidePanel();
        }
        else
        {
            ShowPanel();
        }
    }
    
    /// <summary>
    /// Called when the local processing toggle is changed
    /// </summary>
    /// <param name="isLocalProcessing">New toggle state</param>
    private void OnLocalProcessingToggled(bool isLocalProcessing)
    {
        // Update UI
        UpdateStatusText();
        
        // Notify listeners
        OnLocalProcessingChanged?.Invoke(isLocalProcessing);
    }
    
    /// <summary>
    /// Called when the data sharing toggle is changed
    /// </summary>
    /// <param name="isDataSharing">New toggle state</param>
    private void OnDataSharingToggled(bool isDataSharing)
    {
        // Update UI
        UpdateStatusText();
        
        // Notify listeners
        OnDataSharingChanged?.Invoke(isDataSharing);
    }
    
    /// <summary>
    /// Updates the status text based on current settings
    /// </summary>
    private void UpdateStatusText()
    {
        if (statusText == null)
            return;
            
        bool isLocalProcessing = localProcessingToggle != null && localProcessingToggle.isOn;
        bool isDataSharing = dataSharingToggle != null && dataSharingToggle.isOn;
        
        string statusMessage = "Privacy: ";
        
        if (!isLocalProcessingAvailable)
        {
            statusMessage += "Cloud processing only (local not available on this device)";
        }
        else if (isLocalProcessing)
        {
            statusMessage += "Local processing enabled";
        }
        else
        {
            statusMessage += "Cloud processing";
        }
        
        statusMessage += "\nData sharing: " + (isDataSharing ? "Enabled" : "Disabled");
        
        statusText.text = statusMessage;
    }
    
    /// <summary>
    /// Sets the availability of local processing based on platform capabilities
    /// </summary>
    /// <param name="available">Whether local processing is available</param>
    public void SetLocalProcessingAvailability(bool available)
    {
        isLocalProcessingAvailable = available;
        
        if (localProcessingToggle != null)
        {
            localProcessingToggle.interactable = available;
            
            // If not available, force to false
            if (!available && localProcessingToggle.isOn)
            {
                localProcessingToggle.isOn = false;
            }
        }
        
        UpdateStatusText();
    }
}