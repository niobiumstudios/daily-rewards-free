/***************************************************************************\
Project:      Daily Rewards
Copyright (c) Niobium Studios.
Author:       Guilherme Nunes Barbosa (contact@niobiumstudios.com)
\***************************************************************************/

namespace Niobium.Sample
{
    using UnityEngine;

    /// <summary>
    /// Sample implementation demonstrating how to integrate Daily Rewards, Timed Rewards, 
    /// or Mission Rewards into your project.
    /// 
    /// This class provides a template for handling reward claims and integrating them
    /// with your game's economy and progression systems.
    /// </summary>
    public class IntegrationSample : MonoBehaviour
    {
        /// <summary>
        /// Handles reward claims from Daily Rewards, Timed Rewards, or Mission Rewards systems.
        /// This method should be connected to the OnClaimReward event in the reward system.
        /// </summary>
        /// <param name="reward">The reward object containing reward details and quantity</param>
        /// <example>
        /// // To connect this method to a reward system:
        /// // DailyRewards.Instance.OnClaimReward.AddListener(OnClaimReward);
        /// // DailyMissions.Instance.OnClaimReward.AddListener(OnClaimReward);
        /// </example>
        public void OnClaimReward(Reward reward)
        {
            print($"OnClaimReward {reward.name} {reward.quantity}");

            var rewardsCount = PlayerPrefs.GetInt("MY_REWARD_KEY", 0);
            rewardsCount += reward.quantity;

            PlayerPrefs.SetInt("MY_REWARD_KEY", rewardsCount);
            PlayerPrefs.Save();
        }
    }
}