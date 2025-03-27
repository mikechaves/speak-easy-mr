using UnityEngine;

public class SimpleKeyboardDebugInput : MonoBehaviour
{
    [SerializeField] private SimpleVoiceCommandManager voiceManager;
    
    void Start()
    {
        if (voiceManager == null)
        {
            voiceManager = FindObjectOfType<SimpleVoiceCommandManager>();
            if (voiceManager == null)
            {
                Debug.LogError("No SimpleVoiceCommandManager found. Keyboard debug input will not work.");
                enabled = false;
            }
            else
            {
                Debug.Log("SimpleVoiceCommandManager found!");
            }
        }
    }
    
    void Update()
    {
        // For development testing with keyboard
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Debug: Simulating 'Start' command");
            voiceManager.SimulateStartCommand();
        }
        else if (Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Debug: Simulating 'Next' command");
            voiceManager.SimulateNextCommand();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Debug: Simulating 'Repeat' command");
            voiceManager.SimulateRepeatCommand();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Debug: Simulating 'End' command");
            voiceManager.SimulateEndCommand();
        }
    }
}