/***************************************************************************\
Project:      Daily Rewards
Copyright (c) Niobium Studios.
Author:       Guilherme Nunes Barbosa (contact@niobiumstudios.com)
\***************************************************************************/

namespace Niobium
{
    /// <summary>
    /// Specialized reward panel for Daily Rewards system that handles reward display
    /// and panel management specific to daily reward claims.
    /// </summary>
    public class PanelRewardDailyReward : PanelReward
    {
        protected DailyRewardsUIManager _manager;

        /// <summary>
        /// Initializes the panel with the UI manager reference
        /// </summary>
        /// <param name="manager">Reference to the Daily Rewards UI Manager</param>
        public void Initialize(DailyRewardsUIManager manager)
        {
            this._manager = manager;
        }

        /// <summary>
        /// Closes the reward panel and optionally closes the main rewards panel
        /// </summary>
        public override void Close()
        {
            base.Close();

            var keepOpen = _manager.DailyRewards.KeepOpen;
            if (!keepOpen)
                _manager.PanelDailyRewards.Close();
        }

        /// <summary>
        /// Resets the panel state
        /// </summary>
        public override void Reset()
        {
            _manager.PanelDailyRewards.ReadyToClaim = false;
        }

        /// <summary>
        /// Updates the main rewards panel UI
        /// </summary>
        public override void UpdateUI()
        {
            _manager.PanelDailyRewards.UpdateUI();
        }
    }
}