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
    /// Custom editor for CloudClock component that provides an organized and user-friendly
    /// interface for configuring cloud time synchronization settings.
    /// </summary>
    [CustomEditor(typeof(CloudClock))]
    public class CloudClockEditor : Editor
    {
        private SerializedProperty _tickDelayProp;

        /// <summary>
        /// Called when the editor is enabled, caches serialized properties for better performance
        /// </summary>
        private void OnEnable()
        {
            _tickDelayProp = serializedObject.FindProperty("_tickDelay");
        }

        /// <summary>
        /// Draws the custom inspector GUI for CloudClock component
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawBasicSettings();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the basic timing and retry settings section
        /// </summary>
        private void DrawBasicSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Basic Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_tickDelayProp,
                new GUIContent("Tick Delay", "Time in seconds between each clock update tick"));

            EditorGUILayout.Space();
        }
    }
}