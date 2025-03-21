using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PrivacySettings : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VoiceCommandManager voiceCommandManager;
    [SerializeField] private Toggle localProcessingToggle;
    [SerializeField] private Toggle dataSharingToggle;
    [SerializeField] private TMP_Text privacyStatusText;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject privacyPanel;
    
    private void Start()
    {
        // Set up toggle listeners
        if (localProcessingToggle != null)
        {
            localProcessingToggle.onValueChanged.AddListener(OnLocalProcessingToggled);
        }
        
        if (dataSharingToggle != null)
        {
            dataSharingToggle.onValueChanged.AddListener(OnDataSharingToggled);
        }
        
        UpdatePrivacyStatusText();
    }
    
    public void TogglePrivacyPanel()
    {
        privacyPanel.SetActive(!privacyPanel.activeSelf);
    }
    
    private void OnLocalProcessingToggled(bool isLocal)
    {
        // Apply local processing setting
        // Note: This requires Wit.ai SDK support for local processing
        // You may need to modify this based on the actual implementation
        
        UpdatePrivacyStatusText();
    }
    
    private void OnDataSharingToggled(bool isSharing)
    {
        // Apply data sharing settings
        
        UpdatePrivacyStatusText();
    }
    
    private void UpdatePrivacyStatusText()
    {
        bool isLocalProcessing = localProcessingToggle != null && localProcessingToggle.isOn;
        bool isDataSharing = dataSharingToggle != null && dataSharingToggle.isOn;
        
        string statusText = "Privacy: ";
        
        if (isLocalProcessing)
        {
            statusText += "Local processing enabled";
        }
        else
        {
            statusText += "Cloud processing";
        }
        
        statusText += ", Data sharing: " + (isDataSharing ? "Enabled" : "Disabled");
        
        privacyStatusText.text = statusText;
    }
}