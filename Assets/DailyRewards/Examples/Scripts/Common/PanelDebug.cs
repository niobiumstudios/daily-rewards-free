/***************************************************************************\
Project:      Daily Rewards
Copyright (c) Niobium Studios.
Author:       Guilherme Nunes Barbosa (contact@niobiumstudios.com)
\***************************************************************************/

using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Niobium
{
    /// <summary>
    /// Debug panel for testing and development purposes that provides time manipulation
    /// and system reset functionality for the reward systems.
    /// </summary>
    public class PanelDebug : Panel
    {
        [Header("UI References")]
        [Tooltip("Text component that displays the current cloud clock time")]
        [SerializeField] private TMP_Text _textCurrentTime;

        /// <summary>
        /// Initializes the debug panel and subscribes to cloud clock events
        /// </summary>
        private void Start()
        {
            CloudClock.OnTickTime += OnTickTime;
        }

        /// <summary>
        /// Cleans up event subscriptions when the panel is destroyed
        /// </summary>
        private void OnDestroy()
        {
            CloudClock.OnTickTime -= OnTickTime;
        }

        /// <summary>
        /// Advances the debug time by one day for testing daily reward cycles
        /// </summary>
        public void OnClickDebugAdvanceDay()
        {
            CloudClock.Instance.DebugTime = CloudClock.Instance.DebugTime.Add(TimeSpan.FromDays(1));
        }

        /// <summary>
        /// Advances the debug time by one hour for testing timed rewards
        /// </summary>
        public void OnClickDebugAdvanceHour()
        {
            CloudClock.Instance.DebugTime = CloudClock.Instance.DebugTime.Add(TimeSpan.FromHours(1));
        }

        /// <summary>
        /// Resets all reward systems by clearing player preferences and debug time
        /// </summary>
        public void OnClickDebugReset()
        {
            CloudClock.Instance.Reset();
        }

        /// <summary>
        /// Reloads the current scene to test initialization and persistence
        /// </summary>
        public void OnClickDebugReload()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// Handles cloud clock tick events to update the current time display
        /// </summary>
        /// <param name="time">Current server time</param>
        /// <param name="step">Time elapsed since last tick</param>
        private void OnTickTime(DateTime time, float step)
        {
            if (_textCurrentTime != null)
            {
                _textCurrentTime.text = time.ToString("G");
            }
        }
    }
}