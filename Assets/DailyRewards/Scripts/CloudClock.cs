/***************************************************************************\
Project:      Daily Rewards
Copyright (c) Niobium Studios
Author:       Guilherme Nunes Barbosa (contact@niobiumstudios.com)
\***************************************************************************/

using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Niobium
{
    /// <summary>
    /// Cloud-based time synchronization service that provides accurate server time
    /// instead of relying on device local time. Handles multiple time server fallbacks,
    /// retry logic, and real-time time tracking with tick events.
    /// </summary>
    public class CloudClock : MonoBehaviour
    {
        [Serializable]
        public class CloudClockConfiguration
        {
            [Tooltip("Descriptive name for this time server configuration (for organizational purposes)")]
            public string ServerName = "Time Server";

            [Tooltip("URL endpoint for the time server JSON API")]
            public string Url = "https://worldtimeapi.org/api/timezone/EST";

            [Tooltip("JSON property name that contains the datetime string in the server response")]
            public string DateTimeProperty = "dateTime";
        }

        // Constants
        private const string DEBUG_TIME_KEY = "DebugTimeKey";

        [Header("Timing Settings")]
        [Tooltip("Interval in seconds between time tick updates")]
        [SerializeField] private float _tickDelay = 0.15f;

        // Debug
        [Header("Debug")]
        [Tooltip("Time offset for debugging purposes. Added to the actual time.")]
        public TimeSpan DebugTime;

        // Events
        /// <summary>
        /// Triggered when clock initialization completes. Parameters: success status, error message
        /// </summary>
        public static Action<bool, string> OnInitialize;

        /// <summary>
        /// Triggered on each time tick. Parameters: current DateTime, elapsed time since last tick
        /// </summary>
        public static Action<DateTime, float> OnTickTime;

        /// <summary>
        /// Triggered when debug time is reset
        /// </summary>
        public static Action OnReset;

        // Properties
        /// <summary>
        /// Current state of the cloud clock system
        /// </summary>
        public State currentState { get; private set; }

        /// <summary>
        /// Gets the current synchronized time including debug offsets
        /// </summary>
        public DateTime GetTime() => cloudClockDate + DebugTime;

        // Private fields
        private DateTime cloudClockDate;
        private static int connectionRetries;
        private static CloudClock instance;
        private Coroutine tickTimeCoroutine;

        /// <summary>
        /// Represents the operational states of the CloudClock system
        /// </summary>
        public enum State
        {
            NotInitialized,
            Initializing,
            Initialized,
            FailedToInitialize
        }

        /// <summary>
        /// Singleton instance accessor for CloudClock
        /// </summary>
        public static CloudClock Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<CloudClock>();
                return instance;
            }
        }

        /// <summary>
        /// Initializes the singleton instance and ensures persistence across scenes
        /// </summary>
        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Starts the clock initialization process when the component is enabled
        /// </summary>
        private IEnumerator Start()
        {
            yield return InitializeClockCoroutine();
        }

        /// <summary>
        /// Saves debug time when the application quits or object is destroyed
        /// </summary>
        private void OnDestroy()
        {
            PlayerPrefs.SetInt(DEBUG_TIME_KEY, (int)DebugTime.TotalSeconds);
        }

#if !UNITY_EDITOR
        /// <summary>
        /// Handles application focus changes by reinitializing the clock when focus is regained
        /// </summary>
        /// <param name="focus">Whether the application has gained focus</param>
        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                currentState = State.NotInitialized;
                StopAllCoroutines();
            }
            else
            {
                StartCoroutine(InitializeClockCoroutine());
            }
        }
#endif

        /// <summary>
        /// Main initialization coroutine that sets up the clock and starts the tick system
        /// </summary>
        private IEnumerator InitializeClockCoroutine()
        {
            yield return StartCoroutine(InitializeDate());

            if (currentState != State.Initialized)
                yield break;

            // Ensure only one tick coroutine is running
            if (tickTimeCoroutine != null)
                StopCoroutine(tickTimeCoroutine);

            tickTimeCoroutine = StartCoroutine(TickTime());
        }

        /// <summary>
        /// Initializes the current date time, using either cloud servers or device local time
        /// </summary>
        public IEnumerator InitializeDate()
        {
            cloudClockDate = DateTime.Now;
            currentState = State.Initialized;
            OnInitialize?.Invoke(true, "");
            yield break;
        }

        /// <summary>
        /// Attempts to initialize the cloud clock by connecting to configured time servers
        /// </summary>
        /// <param name="cloudClockList">List of time server configurations to try</param>
        /// <param name="maxRetries">Maximum retry attempts per server</param>
        public IEnumerator InitializeCloudClock(List<CloudClockConfiguration> cloudClockList, int maxRetries)
        {
            currentState = State.Initializing;

            // Try each configured time server in priority order
            foreach (var cloudClock in cloudClockList)
            {
                if (currentState == State.Initialized)
                    yield break;

                connectionRetries = 0;
                string serverResponse = string.Empty;

                // Retry logic for current server
                while (connectionRetries < maxRetries)
                {
                    using (var www = UnityWebRequest.Get(cloudClock.Url))
                    {
                        yield return www.SendWebRequest();

                        if (www.result == UnityWebRequest.Result.Success)
                        {
                            serverResponse = www.downloadHandler.text;
                            Debug.Log($"Successfully connected to time server: {cloudClock.Url}");
                            break;
                        }

                        // Handle connection error
                        connectionRetries++;
                        Debug.LogWarning($"Error loading Cloud Clock {cloudClock.Url}. Retry {connectionRetries}/{maxRetries}. Error: {www.error}");

                        // Brief delay before retry
                        yield return new WaitForSeconds(0.15f);
                    }
                }

                // Process server response if successful
                if (string.IsNullOrEmpty(serverResponse))
                {
                    Debug.LogError($"Failed to get response from Cloud Clock: {cloudClock.Url}");
                    currentState = State.FailedToInitialize;
                    continue;
                }

                // Parse JSON response and extract datetime
                try
                {
                    var jsonData = JSON.Parse(serverResponse);
                    var dateTimeNode = jsonData[cloudClock.DateTimeProperty];

                    if (dateTimeNode == null)
                    {
                        throw new ArgumentNullException($"DateTime property '{cloudClock.DateTimeProperty}' not found in server response.");
                    }

                    cloudClockDate = dateTimeNode.AsDateTime;
                    LoadDebugTime();

                    // Log successful time synchronization
                    var formattedTime = $"{cloudClockDate:yyyy/MM/dd HH:mm:ss}";
                    Debug.Log($"Cloud Clock synchronized successfully: {formattedTime}");

                    currentState = State.Initialized;
                    yield break; // Success - exit the method
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error parsing response from {cloudClock.Url}: {ex.Message}");
                    currentState = State.FailedToInitialize;
                }
            }
        }

        /// <summary>
        /// Continuous coroutine that updates the current time and triggers tick events
        /// </summary>
        protected IEnumerator TickTime()
        {
            var accumulatedTime = 0f;

            while (true)
            {
                accumulatedTime += Time.unscaledDeltaTime;

                if (accumulatedTime >= _tickDelay)
                {
                    // Update the cloud clock date
                    cloudClockDate = cloudClockDate.AddSeconds(accumulatedTime);

                    // Notify subscribers of time tick
                    OnTickTime?.Invoke(cloudClockDate + DebugTime, accumulatedTime);

                    accumulatedTime -= _tickDelay;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Loads saved debug time offset from persistent storage
        /// </summary>
        protected virtual void LoadDebugTime()
        {
            int debugSeconds = PlayerPrefs.GetInt(DEBUG_TIME_KEY, 0);
            DebugTime = new TimeSpan(0, 0, debugSeconds);
        }

        /// <summary>
        /// Resets debug time offset and triggers reset event
        /// </summary>
        public virtual void Reset()
        {
            PlayerPrefs.DeleteKey(DEBUG_TIME_KEY);
            DebugTime = new TimeSpan();
            OnReset?.Invoke();
        }
    }
}