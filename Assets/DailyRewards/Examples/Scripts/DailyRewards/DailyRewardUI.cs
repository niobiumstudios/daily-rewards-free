/***************************************************************************\
Project:      Daily Rewards
Copyright (c) Niobium Studios.
Author:       Guilherme Nunes Barbosa (contact@niobiumstudios.com)
\***************************************************************************/

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Niobium
{
    /// <summary>
    /// UI representation of an individual daily reward showing day, reward information,
    /// and visual state (available, unavailable, or claimed).
    /// </summary>
    public class DailyRewardUI : MonoBehaviour
    {
        [Header("Display Settings")]
        [Tooltip("Whether to show the reward name along with quantity")]
        public bool showRewardName;

        [Header("UI Elements")]
        [Tooltip("Text displaying the day number (e.g., 'Day 12')")]
        public TMP_Text textDay;

        [Tooltip("Text displaying the reward amount or name")]
        public TMP_Text textReward;

        [Tooltip("Background image for the reward display")]
        public Image imageRewardBackground;

        [Tooltip("Image displaying the reward icon")]
        public Image imageReward;

        [Tooltip("Background color when reward is claimed or available to claim")]
        public Color colorClaim;

        private Color colorUnclaimed;

        [Header("Reward Data")]
        [Tooltip("Day number this reward represents")]
        public int day;

        [Tooltip("The reward data associated with this day")]
        [HideInInspector]
        public Reward reward;

        [Tooltip("Current state of this reward")]
        public DailyRewardState state;

        /// <summary>
        /// Possible states for a daily reward
        /// </summary>
        public enum DailyRewardState
        {
            UNCLAIMED_AVAILABLE,
            UNCLAIMED_UNAVAILABLE,
            CLAIMED
        }

        /// <summary>
        /// Initializes color references on awake
        /// </summary>
        void Awake()
        {
            colorUnclaimed = imageReward.color;
        }

        /// <summary>
        /// Initializes the UI with reward data
        /// </summary>
        public void Initialize()
        {
            textDay.text = string.Format("Day {0}", day.ToString());

            if (reward.quantity > 0)
            {
                textReward.text = showRewardName ?
                    reward.quantity + " " + reward.name :
                    reward.quantity.ToString();
            }
            else
            {
                textReward.text = reward.name.ToString();
            }

            imageReward.sprite = reward.sprite;
        }

        /// <summary>
        /// Refreshes the UI appearance based on current state
        /// </summary>
        public void Refresh()
        {
            switch (state)
            {
                case DailyRewardState.UNCLAIMED_AVAILABLE:
                    imageRewardBackground.color = colorClaim;
                    break;
                case DailyRewardState.UNCLAIMED_UNAVAILABLE:
                    imageRewardBackground.color = colorUnclaimed;
                    break;
                case DailyRewardState.CLAIMED:
                    imageRewardBackground.color = colorClaim;
                    break;
            }
        }
    }
}