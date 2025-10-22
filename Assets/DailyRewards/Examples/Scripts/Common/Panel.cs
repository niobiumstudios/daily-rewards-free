/***************************************************************************\
Project:      Daily Rewards
Copyright (c) Niobium Studios.
Author:       Guilherme Nunes Barbosa (contact@niobiumstudios.com)
\***************************************************************************/

using UnityEngine;

namespace Niobium
{
    /// <summary>
    /// Base class for UI panels that provides common functionality for opening and closing
    /// panel interfaces with canvas management.
    /// </summary>
    public class Panel : MonoBehaviour
    {
        [Header("Canvas Configuration")]
        [Tooltip("The Canvas component that controls the visibility of this panel")]
        [SerializeField]
        protected Canvas m_canvas;

        /// <summary>
        /// Opens the panel by enabling the canvas and resetting its position
        /// </summary>
        public virtual void Open()
        {
            transform.localPosition = Vector2.zero;
            m_canvas.enabled = true;
        }

        /// <summary>
        /// Closes the panel by disabling the canvas
        /// </summary>
        public virtual void Close()
        {
            m_canvas.enabled = false;
        }

        /// <summary>
        /// Toggles the panel state between open and closed
        /// </summary>
        public virtual void Toggle()
        {
            if (m_canvas.enabled)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        /// <summary>
        /// Checks if the panel is currently open
        /// </summary>
        /// <returns>True if the panel is open and visible</returns>
        public virtual bool IsOpen()
        {
            return m_canvas != null && m_canvas.enabled;
        }
    }
}