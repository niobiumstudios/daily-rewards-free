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
    /// Abstract base class for reward display panels that provides common functionality
    /// for showing reward information and visual feedback to players.
    /// </summary>
    public abstract class PanelReward : Panel
    {
        [Header("Reward Display")]
        [Tooltip("Text component that displays the reward message to the player")]
        [SerializeField] protected TMP_Text _textReward;

        [Tooltip("Image component that displays the reward icon or sprite")]
        [SerializeField] protected Image _imageReward;

        /// <summary>
        /// Displays the reward information to the player and opens the panel
        /// </summary>
        /// <param name="reward">The reward to display</param>
        public virtual void ShowReward(Reward reward)
        {
            Open();

            _imageReward.sprite = reward.sprite;
            var unit = reward.name;
            var rewardQt = reward.quantity;

            if (rewardQt > 1)
                _textReward.text = string.Format("You got {0} {1}!", reward.quantity, unit);
            else
                _textReward.text = string.Format("You got {0}!", unit);
        }

        /// <summary>
        /// Resets the panel state and UI elements
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Updates the panel UI with current reward system state
        /// </summary>
        public abstract void UpdateUI();
    }
}