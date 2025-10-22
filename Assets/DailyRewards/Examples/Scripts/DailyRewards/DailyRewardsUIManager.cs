/***************************************************************************\
Project:      Daily Rewards
Copyright (c) Niobium Studios.
Author:       Guilherme Nunes Barbosa (contact@niobiumstudios.com)
\***************************************************************************/

using UnityEngine;

namespace Niobium
{
    /// <summary>
    /// UI Manager specifically for Daily Rewards system that handles panel initialization
    /// and reward interface management.
    /// </summary>
    public class DailyRewardsUIManager : UIManager
    {
        [Header("Daily Rewards Reference")]
        [Tooltip("Reference to the DailyRewards component")]
        public DailyRewards DailyRewards;

        [Header("Panels")]
        [Tooltip("Reference to the daily rewards display panel")]
        public PanelDailyRewards PanelDailyRewards;

        /// <summary>
        /// Initializes all panels by setting up reward panel and closing them initially
        /// </summary>
        protected override void InitializePanels()
        {
            ((PanelRewardDailyReward)PanelReward).Initialize(this);
            this.PanelReward.Close();
            PanelDailyRewards.Close();
        }

        /// <summary>
        /// Called when the system is initialized to set up the rewards panel
        /// </summary>
        public override void OnInitialize()
        {
            PanelDailyRewards.Initialize(this);
        }
    }
}