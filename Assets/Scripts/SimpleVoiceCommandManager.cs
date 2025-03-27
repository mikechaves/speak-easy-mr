using UnityEngine;
using UnityEngine.Events;

public class SimpleVoiceCommandManager : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent OnStartTherapyCommand;
    public UnityEvent OnNextStepCommand;
    public UnityEvent OnRepeatCommand;
    public UnityEvent OnEndSessionCommand;
    public UnityEvent OnCalibrationComplete;
    public UnityEvent<string> OnCommandRecognized;
    public UnityEvent<string> OnCommandNotRecognized;
    
    private void Awake()
    {
        // Initialize Unity Events if they don't exist
        if (OnStartTherapyCommand == null) OnStartTherapyCommand = new UnityEvent();
        if (OnNextStepCommand == null) OnNextStepCommand = new UnityEvent();
        if (OnRepeatCommand == null) OnRepeatCommand = new UnityEvent();
        if (OnEndSessionCommand == null) OnEndSessionCommand = new UnityEvent();
        if (OnCalibrationComplete == null) OnCalibrationComplete = new UnityEvent();
        if (OnCommandRecognized == null) OnCommandRecognized = new UnityEvent<string>();
        if (OnCommandNotRecognized == null) OnCommandNotRecognized = new UnityEvent<string>();
    }
    
    void Start()
    {
        Debug.Log("SimpleVoiceCommandManager initialized");
    }
    
    public void BeginCalibration()
    {
        Debug.Log("Starting calibration simulation...");
        Invoke("CompleteCalibration", 2f);
    }
    
    private void CompleteCalibration()
    {
        Debug.Log("Calibration complete!");
        OnCalibrationComplete?.Invoke();
    }
    
    // Simulation methods for keyboard testing
    public void SimulateStartCommand()
    {
        Debug.Log("Simulating 'Start Therapy' command");
        OnStartTherapyCommand?.Invoke();
        OnCommandRecognized?.Invoke("start");
    }
    
    public void SimulateNextCommand()
    {
        Debug.Log("Simulating 'Next Step' command");
        OnNextStepCommand?.Invoke();
        OnCommandRecognized?.Invoke("next");
    }
    
    public void SimulateRepeatCommand()
    {
        Debug.Log("Simulating 'Repeat' command");
        OnRepeatCommand?.Invoke();
        OnCommandRecognized?.Invoke("repeat");
    }
    
    public void SimulateEndCommand()
    {
        Debug.Log("Simulating 'End Session' command");
        OnEndSessionCommand?.Invoke();
        OnCommandRecognized?.Invoke("end");
    }
}