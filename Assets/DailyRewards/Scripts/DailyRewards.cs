/***************************************************************************\
Project:      Daily Rewards
Copyright (c) Niobium Studios.
Author:       Guilherme Nunes Barbosa (contact@niobiumstudios.com)
\***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;

namespace Niobium
{
    /// <summary>
    /// Manages daily reward system that tracks player login streaks and distributes rewards
    /// based on consecutive daily claims. Handles reward cycling, reset logic, and time validation.
    /// </summary>
    public class DailyRewards : DailyRewardsCore<DailyRewards>
    {
        [Header("Reward Configuration")]
        [Tooltip("List of rewards in sequential order for the reward cycle")]
        public List<Reward> Rewards;

        [Header("Behavior Settings")]
        [Tooltip("Keep the reward UI open even when no rewards are available to claim")]
        public bool KeepOpen = true;

        [Tooltip("Reset reward progress to day 1 if player misses more than one day")]
        public bool ResetPrize = true;

        [Header("Events")]
        [Tooltip("Triggered when player successfully claims a daily reward")]
        public UnityEvent<Reward> OnClaimReward;

        // Player progress state
        [Header("Debug - Read Only")]
        [Tooltip("Last time the player claimed a reward (read-only)")]
        [SerializeField] protected DateTime _lastRewardTime;

        [Tooltip("Currently available reward index that can be claimed (read-only)")]
        protected int _availableReward;

        [Tooltip("Last reward index that was claimed by the player (read-only)")]
        private int _lastReward;

        // Constants
        private const string LAST_REWARD_TIME_KEY = "LastRewardTime";
        private const string LAST_REWARD_KEY = "LastReward";

        public int LastReward => _lastReward;
        public int AvailableReward => _availableReward;

        /// <summary>
        /// Initializes the daily rewards system by loading player progress
        /// </summary>
        protected override void Initialize()
        {
            CheckRewards();
        }

        /// <summary>
        /// Handles application pause/resume to check for reward availability changes
        /// </summary>
        /// <param name="pauseStatus">True if application is pausing, false if resuming</param>
        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus) // Application resumed
            {
                CheckRewards();
            }
        }

        /// <summary>
        /// Calculates the time difference until next reward is available
        /// </summary>
        /// <returns>TimeSpan until next reward can be claimed</returns>
        public TimeSpan GetTimeUntilNextReward()
        {
            var now = CloudClock.Instance.GetTime();
            var nextAvailableTime = _lastRewardTime.AddDays(1);
            return nextAvailableTime - now;
        }

        /// <summary>
        /// Checks if player is eligible to claim a reward based on last claim time
        /// </summary>
        /// <returns>True if a reward is available to claim</returns>
        public bool CanClaimReward()
        {
            return _availableReward > 0;
        }

        /// <summary>
        /// Validates and updates reward availability based on player's last claim time
        /// </summary>
        public void CheckRewards()
        {
            LoadPlayerProgress();

            var now = CloudClock.Instance.GetTime();
            var timeSinceLastClaim = now - _lastRewardTime;

            int daysSinceLastClaim = (int)timeSinceLastClaim.TotalDays;

            DetermineRewardAvailability(daysSinceLastClaim);
        }

        /// <summary>
        /// Loads player reward progress from persistent storage
        /// </summary>
        private void LoadPlayerProgress()
        {
            string lastClaimedTimeStr = PlayerPrefs.GetString(GetLastRewardTimeKey(), string.Empty);
            _lastReward = PlayerPrefs.GetInt(GetLastRewardKey(), 0);

            if (string.IsNullOrEmpty(lastClaimedTimeStr))
            {
                // First time user - show first reward
                _availableReward = 1;
                return;
            }

            _lastRewardTime = DateTime.ParseExact(lastClaimedTimeStr, FMT, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Determines which reward should be available based on days since last claim
        /// </summary>
        /// <param name="daysSinceLastClaim">Number of full days since last reward claim</param>
        private void DetermineRewardAvailability(int daysSinceLastClaim)
        {
            switch (daysSinceLastClaim)
            {
                case 0:
                    // Claimed today - no reward available
                    _availableReward = 0;
                    break;

                case 1:
                    // Perfect streak - next reward available
                    HandleConsecutiveClaim();
                    break;

                case >= 2:
                    // Missed one or more days
                    HandleMissedDays(daysSinceLastClaim);
                    break;

                default:
                    // Future date or error - maintain current state
                    break;
            }
        }

        /// <summary>
        /// Handles reward progression for consecutive daily claims
        /// </summary>
        private void HandleConsecutiveClaim()
        {
            if (LastReward >= Rewards.Count)
            {
                // Cycle complete - restart from first reward
                _availableReward = 1;
                _lastReward = 0;
            }
            else
            {
                // Progress to next reward
                _availableReward = LastReward + 1;
            }
        }

        /// <summary>
        /// Handles reward state when player misses one or more days
        /// </summary>
        /// <param name="daysSinceLastClaim">Number of days missed</param>
        private void HandleMissedDays(int daysSinceLastClaim)
        {
            if (ResetPrize && daysSinceLastClaim >= 2)
            {
                // Reset to first reward after missing multiple days
                _availableReward = 1;
                _lastReward = 0;
            }
        }

        /// <summary>
        /// Claims the currently available reward and updates player progress
        /// </summary>
        public void ClaimPrize()
        {
            if (!CanClaimReward())
            {
                Debug.LogWarning("Attempted to claim reward but none is available or already claimed.");
                return;
            }

            try
            {
                var reward = GetReward(_availableReward);
                ClaimReward(reward);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error claiming reward: {ex.Message}");
            }

            // Refresh reward state
            CheckRewards();
        }

        /// <summary>
        /// Processes reward claim and updates persistent storage
        /// </summary>
        /// <param name="reward">Reward to be claimed</param>
        private void ClaimReward(Reward reward)
        {
            // Update claim time
            var now = CloudClock.Instance.GetTime();
            string lastClaimedStr = now.ToString(FMT);

            PlayerPrefs.SetInt(GetLastRewardKey(), _availableReward);
            PlayerPrefs.SetString(GetLastRewardTimeKey(), lastClaimedStr);
            PlayerPrefs.Save();

            // Notify subscribers
            OnClaimReward?.Invoke(reward);

            Debug.Log($"Daily Reward [{reward.name}] claimed successfully for instance {InstanceId}");

            // Reset available reward to prevent duplicate claims
            _availableReward = 0;
        }

        /// <summary>
        /// Gets the reward for a specific day in the cycle
        /// </summary>
        /// <param name="day">Day number (1-based index)</param>
        /// <returns>Reward for the specified day</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when day is invalid</exception>
        public Reward GetReward(int day)
        {
            if (day < 1 || day > Rewards.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(day),
                    $"Day must be between 1 and {Rewards.Count}. Received: {day}");
            }

            return Rewards[day - 1];
        }

        /// <summary>
        /// Gets the current streak length based on consecutive claims
        /// </summary>
        /// <returns>Current consecutive day streak</returns>
        public int GetCurrentStreak()
        {
            var now = CloudClock.Instance.GetTime();
            var timeSinceLastClaim = now - _lastRewardTime;

            // If claimed today or yesterday, streak is active
            if (timeSinceLastClaim.TotalDays < 2)
            {
                return LastReward;
            }

            // Streak broken
            return 0;
        }

        /// <summary>
        /// Gets the storage key for last reward time based on instance ID
        /// </summary>
        /// <returns>Unique storage key</returns>
        private string GetLastRewardTimeKey()
        {
            return string.IsNullOrEmpty(InstanceId)
                ? LAST_REWARD_TIME_KEY
                : $"{LAST_REWARD_TIME_KEY}_{InstanceId}";
        }

        /// <summary>
        /// Gets the storage key for last reward index based on instance ID
        /// </summary>
        /// <returns>Unique storage key</returns>
        private string GetLastRewardKey()
        {
            return string.IsNullOrEmpty(InstanceId)
                ? LAST_REWARD_KEY
                : $"{LAST_REWARD_KEY}_{InstanceId}";
        }

        /// <summary>
        /// Resets all daily reward progress for testing purposes
        /// </summary>
        public override void OnReset()
        {
            PlayerPrefs.DeleteKey(GetLastRewardKey());
            PlayerPrefs.DeleteKey(GetLastRewardTimeKey());
            PlayerPrefs.Save();

            _lastRewardTime = DateTime.MinValue;
            _availableReward = 1;
            _lastReward = 0;

            OnInitialize?.Invoke();

            Debug.Log("Daily Rewards reset successfully.");
        }
    }
}