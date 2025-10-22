/***************************************************************************\
Project:      Daily Rewards
Copyright (c) Niobium Studios.
Author:       Guilherme Nunes Barbosa (contact@niobiumstudios.com)
\***************************************************************************/

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Niobium
{
    /// <summary>
    /// Main panel for displaying daily rewards interface with scrollable reward list,
    /// claim functionality, and time tracking.
    /// </summary>
    public class PanelDailyRewards : Panel
    {
        [Header("UI Templates")]
        [Tooltip("Prefab template for individual daily reward UI elements")]
        [SerializeField] private DailyRewardUI _dailyRewardUITemplate;

        [Header("Panel Elements")]
        [Tooltip("Button for claiming available rewards")]
        [SerializeField] private GameObject _buttonClaim;

        [Tooltip("Button for closing the panel")]
        [SerializeField] private GameObject _buttonClose;

        [Tooltip("Text displaying time until next available reward")]
        [SerializeField] private TMP_Text _textTimeDue;

        [Tooltip("Scroll rect for navigating through rewards")]
        [SerializeField] private ScrollRect _scrollRect;

        [Header("Status")]
        [Tooltip("Indicates if a reward is currently available to claim")]
        public bool ReadyToClaim;

        protected DailyRewardsUIManager _manager;
        private List<DailyRewardUI> _dailyRewardsUI = new List<DailyRewardUI>();
        private bool _isInitialized;

        private void Awake()
        {
            _buttonClose.SetActive(false);
        }


        /// <summary>
        /// Handles claim button click to claim the current available reward
        /// </summary>
        public void OnClickClaim()
        {
            _manager.DailyRewards.ClaimPrize();
            ReadyToClaim = false;
            UpdateUI();
        }

        /// <summary>
        /// Initializes the list of daily reward UI elements based on configured rewards
        /// </summary>
        private void InitializeDailyRewardsUI()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;

            for (int i = 0; i < _manager.DailyRewards.Rewards.Count; i++)
            {
                int day = i + 1;
                var reward = _manager.DailyRewards.GetReward(day);

                var dailyRewardUI = Instantiate(_dailyRewardUITemplate, _scrollRect.content);
                dailyRewardUI.day = day;
                dailyRewardUI.reward = reward;
                dailyRewardUI.Initialize();
                _dailyRewardsUI.Add(dailyRewardUI);
            }

            _dailyRewardUITemplate.gameObject.SetActive(false);
        }

        /// <summary>
        /// Updates the entire UI based on current reward states
        /// </summary>
        public void UpdateUI()
        {
            _manager.DailyRewards.CheckRewards();

            bool isRewardAvailableNow = false;
            var lastReward = _manager.DailyRewards.LastReward;
            var availableReward = _manager.DailyRewards.AvailableReward;

            foreach (var dailyRewardUI in _dailyRewardsUI)
            {
                var day = dailyRewardUI.day;

                if (day == availableReward)
                {
                    dailyRewardUI.state = DailyRewardUI.DailyRewardState.UNCLAIMED_AVAILABLE;
                    isRewardAvailableNow = true;
                }
                else if (day <= lastReward)
                {
                    dailyRewardUI.state = DailyRewardUI.DailyRewardState.CLAIMED;
                }
                else
                {
                    dailyRewardUI.state = DailyRewardUI.DailyRewardState.UNCLAIMED_UNAVAILABLE;
                }

                dailyRewardUI.Refresh();
            }

            _buttonClaim.SetActive(isRewardAvailableNow);
            _buttonClose.SetActive(!isRewardAvailableNow);

            if (isRewardAvailableNow)
            {
                SnapToReward();
                _textTimeDue.text = "You can claim your reward!";
            }

            ReadyToClaim = isRewardAvailableNow;
        }

        /// <summary>
        /// Scrolls the view to focus on the current or next available reward
        /// </summary>
        public void SnapToReward()
        {
            Canvas.ForceUpdateCanvases();

            var lastRewardIdx = _manager.DailyRewards.LastReward;

            if (_dailyRewardsUI.Count - 1 < lastRewardIdx)
                lastRewardIdx++;

            if (lastRewardIdx > _dailyRewardsUI.Count - 1)
                lastRewardIdx = _dailyRewardsUI.Count - 1;

            var content = _scrollRect.content;
            float normalizePosition = 1 - ((float)lastRewardIdx / ((float)content.childCount - 2));
            _scrollRect.verticalNormalizedPosition = normalizePosition;
        }

        /// <summary>
        /// Checks time difference until next reward and updates display
        /// </summary>
        private void CheckTimeDifference()
        {
            TimeSpan difference = _manager.DailyRewards.GetTimeUntilNextReward();

            if (difference.TotalSeconds <= 0)
            {
                ReadyToClaim = true;
                UpdateUI();
                SnapToReward();
                return;
            }

            string formattedTs = _manager.DailyRewards.GetFormattedTime(difference);
            _textTimeDue.text = formattedTs;
        }

        /// <summary>
        /// Handles reward claim event and shows reward message
        /// </summary>
        /// <param name="dailyRewards">The daily rewards instance</param>
        /// <param name="day">The day that was claimed</param>
        public void OnClaimPrize(DailyRewards dailyRewards, int day)
        {
            var reward = dailyRewards.GetReward(day);
            _manager.PanelReward.ShowReward(reward);
        }

        /// <summary>
        /// Initializes the panel with the UI manager and sets up display
        /// </summary>
        /// <param name="manager">Reference to the Daily Rewards UI Manager</param>
        public void Initialize(DailyRewardsUIManager manager)
        {
            this._manager = manager;

            var showWhenNotAvailable = manager.DailyRewards.KeepOpen;
            var isRewardAvailable = manager.DailyRewards.AvailableReward > 0;

            UpdateUI();

            var isShow = showWhenNotAvailable || (!showWhenNotAvailable && isRewardAvailable);
            if (isShow)
            {
                InitializeDailyRewardsUI();
                Open();
            }
            else
            {
                Close();
            }

            UpdateUI();
            SnapToReward();
            CheckTimeDifference();
        }

        /// <summary>
        /// Handles time tick events to update countdown display
        /// </summary>
        /// <param name="dateTime">Current time</param>
        /// <param name="tick">Time elapsed since last tick</param>
        public void OnTickTime(DateTime dateTime, float tick)
        {
            CheckTimeDifference();
        }
    }
}