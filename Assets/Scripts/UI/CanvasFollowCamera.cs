using UnityEngine;

namespace SpeakEasy.UI
{
    public class CanvasFollowCamera : MonoBehaviour
    {
        [SerializeField] private float smoothing = 5f;
        [SerializeField] private float distanceFromCamera = 1f;
        [SerializeField] private bool lookAtCamera = true;
        [SerializeField] private Vector3 offset = new Vector3(0, 0, 0);
        
        private Camera mainCamera;
        
        private void Start()
        {
            // Find the main camera
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("No Main Camera found. Canvas follow will not work.");
                enabled = false;
            }
        }
        
        private void LateUpdate()
        {
            if (mainCamera == null) return;
            
            // Calculate target position
            Vector3 targetPosition = mainCamera.transform.position + 
                                     mainCamera.transform.forward * distanceFromCamera +
                                     offset;
            
            // Smoothly move to that position
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothing);
            
            // Look at camera if specified
            if (lookAtCamera)
            {
                transform.LookAt(mainCamera.transform);
            }
        }
    }
}