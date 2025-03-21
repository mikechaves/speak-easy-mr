using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UI.Enhanced
{
    /// <summary>
    /// Creates a pulsing effect for the microphone icon.
    /// Attach to any UI Image component that needs to pulse.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class MicPulseEffect : MonoBehaviour
    {
        [Tooltip("Speed of the pulse effect")]
        [SerializeField] private float pulseSpeed = 1.5f;
        
        [Tooltip("Minimum alpha value during pulsing")]
        [SerializeField] private float minAlpha = 0.6f;
        
        [Tooltip("Maximum alpha value during pulsing")]
        [SerializeField] private float maxAlpha = 1.0f;
        
        private Image icon;
        private Coroutine pulseCoroutine;
        private bool isPulsing = false;
        
        private void Awake()
        {
            icon = GetComponent<Image>();
        }
        
        /// <summary>
        /// Start or stop the pulse animation
        /// </summary>
        /// <param name="pulse">Whether to pulse or not</param>
        public void SetPulsing(bool pulse)
        {
            if (pulse && !isPulsing)
            {
                StartPulse();
            }
            else if (!pulse && isPulsing)
            {
                StopPulse();
            }
        }
        
        /// <summary>
        /// Starts the pulse animation
        /// </summary>
        public void StartPulse()
        {
            if (icon == null) return;
            
            // Stop any existing coroutine
            StopPulse();
            
            // Start new pulse effect
            pulseCoroutine = StartCoroutine(PulseRoutine());
            isPulsing = true;
        }
        
        /// <summary>
        /// Stops the pulse animation
        /// </summary>
        public void StopPulse()
        {
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
                pulseCoroutine = null;
            }
            
            // Reset alpha
            if (icon != null)
            {
                Color color = icon.color;
                icon.color = new Color(color.r, color.g, color.b, 1.0f);
            }
            
            isPulsing = false;
        }
        
        private IEnumerator PulseRoutine()
        {
            float t = 0;
            
            while (true)
            {
                // Calculate alpha based on sine wave
                float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(t * pulseSpeed) + 1) / 2);
                
                // Apply alpha to icon
                if (icon != null)
                {
                    Color color = icon.color;
                    icon.color = new Color(color.r, color.g, color.b, alpha);
                }
                
                t += Time.deltaTime;
                yield return null;
            }
        }
        
        /// <summary>
        /// Adjust the pulse parameters
        /// </summary>
        /// <param name="speed">New pulse speed</param>
        /// <param name="min">New minimum alpha</param>
        /// <param name="max">New maximum alpha</param>
        public void SetPulseParameters(float speed, float min, float max)
        {
            pulseSpeed = speed;
            minAlpha = min;
            maxAlpha = max;
        }
        
        private void OnDisable()
        {
            // Make sure to stop the coroutine when disabled
            StopPulse();
        }
    }
}