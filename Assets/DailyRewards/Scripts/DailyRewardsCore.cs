/***************************************************************************\
Project:      Daily Rewards
Copyright (c) Niobium Studios
Author:       Guilherme Nunes Barbosa (contact@niobiumstudios.com)
\***************************************************************************/

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Niobium
{
    /// <summary>
    /// Abstract base class that provides core functionality for daily reward systems.
    /// Handles cloud clock integration, instance management, and common timing operations.
    /// </summary>
    /// <typeparam name="T">The derived class type for singleton pattern implementation</typeparam>
    public abstract class DailyRewardsCore<T> : MonoBehaviour where T : DailyRewardsCore<T>
    {
        [Header("Instance Configuration")]
        [Tooltip("Unique identifier for this reward instance. Used to differentiate multiple reward systems.")]
        public string InstanceId = "daily_rewards";

        [Tooltip("Custom format string for displaying time spans. See Microsoft TimeSpan format documentation.")]
        public string DateFormat = @"d' Days 'hh\:mm\:ss";

        [Header("Events")]
        [Tooltip("Triggered when the reward system is fully initialized and ready")]
        public UnityEvent OnInitialize;

        [Tooltip("Triggered on each cloud clock tick with current time and elapsed time since last tick")]
        public UnityEvent<DateTime, float> OnTickTime;

        // Constants
        protected const string FMT = "O"; // ISO 8601 format for DateTime serialization

        /// <summary>
        /// Retrieves an instance of the reward system by its unique identifier
        /// </summary>
        /// <param name="id">The instance identifier to search for</param>
        /// <returns>The found instance or null if not found</returns>
        public static T GetInstance(string id = "")
        {
            if (string.IsNullOrEmpty(id))
            {
                id = "daily_rewards"; // Default instance ID
            }

            var instances = FindObjectsOfType<T>();
            foreach (var instance in instances)
            {
                if (instance.InstanceId == id)
                    return instance;
            }

            Debug.LogWarning($"No instance of {typeof(T).Name} found with ID '{id}'");
            return null;
        }

        /// <summary>
        /// Formats a TimeSpan into a human-readable string using the configured date format
        /// </summary>
        /// <param name="timeSpan">The time span to format</param>
        /// <returns>Formatted time string</returns>
        public string GetFormattedTime(TimeSpan timeSpan)
        {
            if (string.IsNullOrEmpty(DateFormat))
            {
                return timeSpan.ToString(@"d' Days 'hh\:mm\:ss"); // Default format
            }

            try
            {
                return timeSpan.ToString(DateFormat);
            }
            catch (FormatException ex)
            {
                Debug.LogError($"Invalid date format string: '{DateFormat}'. Using default format. Error: {ex.Message}");
                return timeSpan.ToString(@"d' Days 'hh\:mm\:ss");
            }
        }

        /// <summary>
        /// Ensures singleton behavior and prevents duplicate instances with the same ID
        /// </summary>
        protected virtual void Awake()
        {
            var instanceCount = GetInstanceCount();

            if (instanceCount > 1)
            {
                Debug.LogError($"Multiple instances with ID '{InstanceId}' found. Destroying {gameObject.name}");
                Destroy(gameObject);
                return;
            }

            // Keep this instance alive across scene loads if it's the only one
            if (instanceCount == 1)
            {
                DontDestroyOnLoad(gameObject);
            }

            SubscribeToCloudClockEvents();
        }

        /// <summary>
        /// Subscribes to cloud clock events for time synchronization
        /// </summary>
        private void SubscribeToCloudClockEvents()
        {
            CloudClock.OnInitialize += OnInitializeCloudClock;
            CloudClock.OnTickTime += OnTickCloudClock;
            CloudClock.OnReset += OnReset;
        }

        /// <summary>
        /// Unsubscribes from cloud clock events to prevent memory leaks
        /// </summary>
        private void UnsubscribeFromCloudClockEvents()
        {
            CloudClock.OnInitialize -= OnInitializeCloudClock;
            CloudClock.OnTickTime -= OnTickCloudClock;
            CloudClock.OnReset -= OnReset;
        }

        /// <summary>
        /// Handles scene loaded events for reinitialization when needed
        /// </summary>
        protected virtual void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        /// <summary>
        /// Cleans up event subscriptions when the component is disabled
        /// </summary>
        protected virtual void OnDisable()
        {
            UnsubscribeFromCloudClockEvents();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// Handles cloud clock tick events and propagates them to derived classes
        /// </summary>
        /// <param name="dateTime">Current server time</param>
        /// <param name="tick">Time elapsed since last tick in seconds</param>
        protected virtual void OnTickCloudClock(DateTime dateTime, float tick)
        {
            OnTickTime?.Invoke(dateTime, tick);
        }

        /// <summary>
        /// Handles cloud clock initialization completion
        /// </summary>
        /// <param name="isSuccess">True if cloud clock initialized successfully</param>
        /// <param name="error">Error message if initialization failed</param>
        protected virtual void OnInitializeCloudClock(bool isSuccess, string error)
        {
            if (!isSuccess)
            {
                Debug.LogError($"Cloud Clock initialization failed: {error}");
                return;
            }

            InitializeSystem();
        }

#if !UNITY_EDITOR
        /// <summary>
        /// Handles application focus events for reward state refresh
        /// </summary>
        /// <param name="hasFocus">True if application has gained focus</param>
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus || CloudClock.Instance.currentState != CloudClock.State.Initialized)
                return;

            RefreshSystem();
        }
#endif

        /// <summary>
        /// Handles scene loaded events for system reinitialization
        /// </summary>
        /// <param name="scene">The loaded scene</param>
        /// <param name="mode">Scene loading mode</param>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (CloudClock.Instance.currentState != CloudClock.State.Initialized)
                return;

            RefreshSystem();
        }

        /// <summary>
        /// Initializes the reward system and triggers initialization events
        /// </summary>
        private void InitializeSystem()
        {
            Initialize();
            OnInitialize?.Invoke();
        }

        /// <summary>
        /// Refreshes the reward system state
        /// </summary>
        private void RefreshSystem()
        {
            Initialize();
            OnInitialize?.Invoke();
        }

        /// <summary>
        /// Counts the number of active instances with the same instance ID
        /// </summary>
        /// <returns>Number of duplicate instances</returns>
        private int GetInstanceCount()
        {
            var instances = FindObjectsOfType<T>();
            int count = 0;

            foreach (var instance in instances)
            {
                if (instance.InstanceId == InstanceId)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Abstract method to be implemented by derived classes for system-specific initialization
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// Abstract method to be implemented by derived classes for reset functionality
        /// </summary>
        public abstract void OnReset();

        /// <summary>
        /// Checks if the cloud clock is properly initialized and ready
        /// </summary>
        /// <returns>True if cloud clock is initialized and available</returns>
        protected bool IsCloudClockReady()
        {
            return CloudClock.Instance != null &&
                   CloudClock.Instance.currentState == CloudClock.State.Initialized;
        }

        /// <summary>
        /// Gets the current server time from cloud clock with fallback to local time
        /// </summary>
        /// <returns>Current server time or local time if cloud clock unavailable</returns>
        protected DateTime GetCurrentTime()
        {
            if (IsCloudClockReady())
            {
                return CloudClock.Instance.GetTime();
            }

            Debug.LogWarning("Cloud Clock not available, using local system time");
            return DateTime.Now;
        }
    }
}