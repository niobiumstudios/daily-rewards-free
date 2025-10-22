/***************************************************************************\
Project:      Daily Rewards
Copyright (c) Niobium Studios.
Author:       Guilherme Nunes Barbosa (contact@niobiumstudios.com)
\***************************************************************************/

using System;
using UnityEngine;

namespace Niobium
{
    /// <summary>
    /// Represents a reward that can be earned by players through daily rewards,
    /// mission completion, or other achievement systems.
    /// </summary>
    [Serializable]
    public class Reward
    {
        [Header("Basic Information")]
        [Tooltip("Unique identifier for this reward. Used for tracking and references.")]
        public string id;

        [Tooltip("Display name shown to players for this reward.")]
        public string name;

        [Header("Reward Details")]
        [Tooltip("Quantity of this reward to be awarded to the player.")]
        public int quantity = 1;

        [Tooltip("Visual representation (icon) of the reward for UI display.")]
        public Sprite sprite;
    }
}