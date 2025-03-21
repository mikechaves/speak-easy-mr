using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace UI.Enhanced
{
    /// <summary>
    /// Component for command item prefab in the voice command list.
    /// Handles display and state of individual voice command entries.
    /// </summary>
    public class CommandItemPrefab : MonoBehaviour
    {
        [SerializeField] private TMP_Text commandText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image leftBorder;
        [SerializeField] private Image background;
        
        [SerializeField] private Color defaultBackgroundColor = new Color(1, 1, 1, 0.05f);
        [SerializeField] private Color highlightedBackgroundColor = new Color(1, 1, 1, 0.2f);
        
        /// <summary>
        /// Gets the command text for this item
        /// </summary>
        /// <returns>The command text string</returns>
        public string GetCommandText()
        {
            return commandText != null ? commandText.text : "";
        }
        
        /// <summary>
        /// Set the command item data
        /// </summary>
        /// <param name="command">The voice command</param>
        /// <param name="description">Description of what the command does</param>
        /// <param name="borderColor">Color for the left accent border</param>
        public void SetData(string command, string description, Color borderColor)
        {
            if (commandText != null)
            {
                commandText.text = command;
            }
            
            if (descriptionText != null)
            {
                descriptionText.text = description;
            }
            
            if (leftBorder != null)
            {
                leftBorder.color = borderColor;
            }
            
            if (background != null)
            {
                background.color = defaultBackgroundColor;
            }
        }
        
        /// <summary>
        /// Dim the command item when disabled
        /// </summary>
        /// <param name="dim">Whether to dim the item</param>
        public void SetDimmed(bool dim)
        {
            float alpha = dim ? 0.6f : 1.0f;
            
            if (commandText != null)
            {
                Color color = commandText.color;
                commandText.color = new Color(color.r, color.g, color.b, alpha);
            }
            
            if (descriptionText != null)
            {
                Color color = descriptionText.color;
                descriptionText.color = new Color(color.r, color.g, color.b, alpha);
            }
            
            if (leftBorder != null)
            {
                Color color = leftBorder.color;
                leftBorder.color = new Color(color.r, color.g, color.b, alpha);
            }
        }
        
        /// <summary>
        /// Highlight this command when it's detected or active
        /// </summary>
        /// <param name="highlight">Whether to highlight the item</param>
        public void SetHighlighted(bool highlight)
        {
            if (background != null)
            {
                background.color = highlight 
                    ? highlightedBackgroundColor
                    : defaultBackgroundColor;
            }
        }
        
        /// <summary>
        /// Reset this item to default state
        /// </summary>
        public void ResetState()
        {
            SetDimmed(false);
            SetHighlighted(false);
        }
    }
}