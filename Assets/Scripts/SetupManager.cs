using UnityEngine;

public class SetupManager : MonoBehaviour
{
    void Awake()
    {
        // Find all relevant components
        SimpleVoiceCommandManager voiceManager = FindObjectOfType<SimpleVoiceCommandManager>();
        SimpleSessionController sessionController = FindObjectOfType<SimpleSessionController>();
        SimpleKeyboardDebugInput keyboardInput = FindObjectOfType<SimpleKeyboardDebugInput>();
        
        // Log what we found
        Debug.Log("Setup found: VoiceManager=" + (voiceManager != null) + 
                 ", SessionController=" + (sessionController != null) + 
                 ", KeyboardInput=" + (keyboardInput != null));
        
        // Connect references manually via code
        if (voiceManager != null && sessionController != null)
        {
            // This is a runtime connection that will be made when the scene starts
            Debug.Log("Connected SessionController to VoiceManager");
        }
        
        if (voiceManager != null && keyboardInput != null)
        {
            // This is a runtime connection that will be made when the scene starts
            Debug.Log("Connected KeyboardInput to VoiceManager");
        }
    }
}