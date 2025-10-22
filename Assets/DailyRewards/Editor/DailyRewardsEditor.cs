/***************************************************************************\
Project:      Daily Rewards
Copyright (c) Niobium Studios.
Author:       Guilherme Nunes Barbosa (contact@niobiumstudios.com)
\***************************************************************************/

using UnityEditor;
using UnityEngine;

namespace Niobium
{
    /// <summary>
    /// Custom editor for DailyRewards component that provides an organized interface
    /// for configuring daily reward cycles, reward items, and system behavior.
    /// </summary>
    [CustomEditor(typeof(DailyRewards))]
    public class DailyRewardsEditor : Editor
    {
        private SerializedProperty _instanceIdProp;
        private SerializedProperty _rewardsProp;
        private SerializedProperty _keepOpenProp;
        private SerializedProperty _resetPrizeProp;
        private SerializedProperty _dateFormatProp;
        private SerializedProperty _onInitializeProp;
        private SerializedProperty _onTickTimeProp;
        private SerializedProperty _onClaimRewardProp;

        /// <summary>
        /// Called when the editor is enabled, caches serialized properties for better performance
        /// </summary>
        private void OnEnable()
        {
            _instanceIdProp = serializedObject.FindProperty("InstanceId");
            _rewardsProp = serializedObject.FindProperty("Rewards");
            _keepOpenProp = serializedObject.FindProperty("KeepOpen");
            _resetPrizeProp = serializedObject.FindProperty("ResetPrize");
            _dateFormatProp = serializedObject.FindProperty("DateFormat");
            _onInitializeProp = serializedObject.FindProperty("OnInitialize");
            _onTickTimeProp = serializedObject.FindProperty("OnTickTime");
            _onClaimRewardProp = serializedObject.FindProperty("OnClaimReward");
        }

        /// <summary>
        /// Draws the custom inspector GUI for DailyRewards component
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawConfigurationSection();
            DrawRewardsSection();
            DrawEventsSection();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the basic configuration settings section
        /// </summary>
        private void DrawConfigurationSection()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_instanceIdProp,
                new GUIContent("Instance ID", "Unique identifier for this Daily Rewards instance. Change if using multiple instances."));

            EditorGUILayout.PropertyField(_keepOpenProp,
                new GUIContent("Keep Open", "Keep the reward UI visible even when no rewards are available to claim"));

            EditorGUILayout.PropertyField(_resetPrizeProp,
                new GUIContent("Reset Prize", "Reset reward progress to day 1 if player misses more than one day"));

            EditorGUILayout.PropertyField(_dateFormatProp,
                new GUIContent("Date Format", "Custom format string for displaying time spans. See TimeSpan format documentation."));

            EditorGUILayout.Space();
        }

        /// <summary>
        /// Draws the rewards configuration section
        /// </summary>
        private void DrawRewardsSection()
        {
            if (EditorTools.DrawHeader("Rewards Cycle"))
            {
                EditorGUILayout.HelpBox("Configure the daily reward cycle. Rewards are awarded in sequence from day 1 onwards. The cycle restarts after completion.", MessageType.Info);

                DrawRewardsList();

                EditorGUILayout.Space();

                if (GUILayout.Button("Add Reward Day", GUILayout.Height(25)))
                {
                    AddReward();
                }

                EditorGUILayout.Space();
            }
        }

        /// <summary>
        /// Draws the list of rewards in the cycle
        /// </summary>
        private void DrawRewardsList()
        {
            if (_rewardsProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("No rewards configured. Add rewards to create a daily reward cycle.", MessageType.Warning);
                return;
            }

            for (int i = 0; i < _rewardsProp.arraySize; i++)
            {
                DrawRewardItem(i);
            }
        }

        /// <summary>
        /// Draws an individual reward configuration for a specific day
        /// </summary>
        /// <param name="index">Index of the reward in the cycle (0-based)</param>
        private void DrawRewardItem(int index)
        {
            if (EditorTools.DrawHeader($"Day {index + 1}"))
            {
                EditorTools.BeginContents();

                SerializedProperty rewardProp = _rewardsProp.GetArrayElementAtIndex(index);
                var idRewardProp = rewardProp.FindPropertyRelative("id");
                var nameRewardProp = rewardProp.FindPropertyRelative("name");
                var quantityRewardProp = rewardProp.FindPropertyRelative("quantity");
                var spriteRewardProp = rewardProp.FindPropertyRelative("sprite");

                // Auto-generate ID if empty
                if (string.IsNullOrEmpty(idRewardProp.stringValue))
                {
                    idRewardProp.stringValue = $"day_{index + 1}";
                }

                EditorGUILayout.PropertyField(idRewardProp,
                    new GUIContent("ID", "Unique identifier for this reward"));

                EditorGUILayout.PropertyField(nameRewardProp,
                    new GUIContent("Name", "Display name shown to players for this reward"));

                EditorGUILayout.PropertyField(quantityRewardProp,
                    new GUIContent("Quantity", "Amount of this reward to award on this day"));

                spriteRewardProp.objectReferenceValue = EditorGUILayout.ObjectField(
                    new GUIContent("Sprite", "Visual icon for the reward"),
                    spriteRewardProp.objectReferenceValue, typeof(Sprite), false);

                EditorTools.EndContents();

                EditorGUILayout.Space();

                if (GUILayout.Button($"Remove Day {index + 1}", GUILayout.Height(20)))
                {
                    if (EditorUtility.DisplayDialog("Remove Reward Day",
                        $"Are you sure you want to remove the reward for Day {index + 1}?",
                        "Remove", "Cancel"))
                    {
                        _rewardsProp.DeleteArrayElementAtIndex(index);
                    }
                }

                EditorGUILayout.Space();
            }
        }

        /// <summary>
        /// Draws the events section
        /// </summary>
        private void DrawEventsSection()
        {
            if (EditorTools.DrawHeader("Events"))
            {
                EditorGUILayout.HelpBox("Configure UnityEvents for handling daily reward system callbacks in your game code.", MessageType.Info);

                EditorGUILayout.PropertyField(_onInitializeProp,
                    new GUIContent("On Initialize", "Triggered when the daily reward system is fully initialized"));

                EditorGUILayout.PropertyField(_onTickTimeProp,
                    new GUIContent("On Tick Time", "Triggered on each time tick with current time and elapsed time"));

                EditorGUILayout.PropertyField(_onClaimRewardProp,
                    new GUIContent("On Claim Reward", "Triggered when a player claims a daily reward"));
            }
        }

        /// <summary>
        /// Adds a new reward to the cycle with default values
        /// </summary>
        private void AddReward()
        {
            int newIndex = _rewardsProp.arraySize;
            _rewardsProp.InsertArrayElementAtIndex(newIndex);

            var newRewardProp = _rewardsProp.GetArrayElementAtIndex(newIndex);
            var idProp = newRewardProp.FindPropertyRelative("id");
            var nameProp = newRewardProp.FindPropertyRelative("name");
            var quantityProp = newRewardProp.FindPropertyRelative("quantity");

            idProp.stringValue = $"day_{newIndex + 1}";
            nameProp.stringValue = $"Day {newIndex + 1} Reward";
            quantityProp.intValue = 1;
        }
    }
}