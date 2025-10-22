/***************************************************************************\
Project:      Daily Rewards
Copyright (c) Niobium Studios.
Author:       Guilherme Nunes Barbosa (contact@niobiumstudios.com)
\***************************************************************************/

using UnityEngine;

namespace Niobium
{
    /// <summary>
    /// Abstract base class for UI managers that control reward system interfaces
    /// and provide common functionality for panel management.
    /// </summary>
    public abstract class UIManager : MonoBehaviour
    {
        [Header("Common Panels")]
        [Tooltip("Reference to the reward display panel for showing earned rewards")]
        public PanelReward PanelReward;

        /// <summary>
        /// Initializes the UI manager and sets up panel references
        /// </summary>
        protected virtual void Awake()
        {
            InitializePanels();
        }

        /// <summary>
        /// Initializes all UI panels and their references
        /// </summary>
        protected abstract void InitializePanels();

        /// <summary>
        /// Called when the reward system is fully initialized and ready
        /// </summary>
        public abstract void OnInitialize();
    }
}