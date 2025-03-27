using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFixupManager : MonoBehaviour
{
    // This is a utility script to clean up and properly set up the scene
    // It's meant to be added to an empty GameObject and run only once
    
    void Start()
    {
        Debug.Log("Starting scene fixup process...");
        
        // Step 1: Delete unnecessary objects
        CleanupRedundantObjects();
        
        // Step 2: Find or create necessary objects
        SetupRequiredObjects();
        
        // Step 3: Connect references properly
        ConnectReferences();
        
        Debug.Log("Scene fixup complete! Try running the scene now.");
        
        // Self-destruct when done
        Destroy(gameObject);
    }
    
    void CleanupRedundantObjects()
    {
        // Find and delete redundant objects
        GameObject[] redundantObjects = new GameObject[]
        {
            GameObject.Find("VoiceCommandSystem_New"),
            GameObject.Find("SimpleVoiceManager"),
            GameObject.Find("SimpleSessionController"),
            GameObject.Find("AudioSource"),
            GameObject.Find("TherapyStepsPreset"),
            GameObject.Find("SetupManager")
        };
        
        foreach (GameObject obj in redundantObjects)
        {
            if (obj != null)
            {
                Debug.Log("Removing redundant object: " + obj.name);
                DestroyImmediate(obj);
            }
        }
    }
    
    void SetupRequiredObjects()
    {
        // Fix VoiceCommandSystem
        GameObject voiceSystem = GameObject.Find("VoiceCommandSystem");
        if (voiceSystem != null)
        {
            // Ensure it has the necessary components
            if (voiceSystem.GetComponent<AudioSource>() == null)
            {
                voiceSystem.AddComponent<AudioSource>();
            }
            
            // Try to attach SimpleVoiceCommandManager
            if (voiceSystem.GetComponent<SimpleVoiceCommandManager>() == null)
            {
                voiceSystem.AddComponent<SimpleVoiceCommandManager>();
            }
        }
        else
        {
            Debug.LogError("VoiceCommandSystem not found! Please create it manually.");
        }
        
        // Fix SessionController
        GameObject sessionController = GameObject.Find("SessionController");
        if (sessionController != null)
        {
            // Ensure it has the necessary components
            if (sessionController.GetComponent<SimpleSessionController>() == null)
            {
                sessionController.AddComponent<SimpleSessionController>();
            }
        }
        else
        {
            Debug.LogError("SessionController not found! Please create it manually.");
        }
        
        // Fix KeyboardDebug
        GameObject keyboardDebug = GameObject.Find("KeyboardDebug");
        if (keyboardDebug != null)
        {
            // Remove old script
            var oldScript = keyboardDebug.GetComponent<KeyboardDebugInput>();
            if (oldScript != null)
            {
                DestroyImmediate(oldScript);
            }
            
            // Add new script
            if (keyboardDebug.GetComponent<SimpleKeyboardDebugInput>() == null)
            {
                keyboardDebug.AddComponent<SimpleKeyboardDebugInput>();
            }
        }
        else
        {
            Debug.LogError("KeyboardDebug not found! Please create it manually.");
        }
    }
    
    void ConnectReferences()
    {
        // Get all the components we need
        SimpleVoiceCommandManager voiceManager = FindObjectOfType<SimpleVoiceCommandManager>();
        SimpleSessionController sessionController = FindObjectOfType<SimpleSessionController>();
        SimpleKeyboardDebugInput keyboardInput = FindObjectOfType<SimpleKeyboardDebugInput>();
        
        // Connect session controller to voice manager
        if (sessionController != null && voiceManager != null)
        {
            sessionController.SendMessage("SetVoiceManager", voiceManager, SendMessageOptions.DontRequireReceiver);
            Debug.Log("Connected SessionController to VoiceManager");
        }
        
        // Connect keyboard input to voice manager
        if (keyboardInput != null && voiceManager != null)
        {
            keyboardInput.SendMessage("SetVoiceManager", voiceManager, SendMessageOptions.DontRequireReceiver);
            Debug.Log("Connected KeyboardInput to VoiceManager");
        }
    }
}