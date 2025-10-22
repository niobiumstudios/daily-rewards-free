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
    /// Provides reusable editor layout tools and UI components for creating
    /// consistent and professional custom inspectors in the Unity Editor.
    /// </summary>
    public static class EditorTools
    {
        private static readonly Color InactiveHeaderColor = new Color(0.8f, 0.8f, 0.8f);
        private const float HeaderTopSpacing = 3f;
        private const float HeaderBottomSpacing = 3f;
        private const float ContentLeftSpacing = 5f;
        private const float ContentTopSpacing = 2f;
        private const float ContentBottomSpacing = 3f;

        /// <summary>
        /// Draws a collapsible header section with persistent state storage
        /// </summary>
        /// <param name="text">The header label text to display</param>
        /// <returns>True if the header is expanded, false if collapsed</returns>
        /// <example>
        /// if (EditorTools.DrawHeader("Configuration Settings"))
        /// {
        ///     EditorTools.BeginContents();
        ///     // Draw your content here
        ///     EditorTools.EndContents();
        /// }
        /// </example>
        public static bool DrawHeader(string text)
        {
            string editorPrefsKey = $"Niobium.EditorTools.{text}";
            bool isExpanded = EditorPrefs.GetBool(editorPrefsKey, true);

            // Add spacing before header
            GUILayout.Space(HeaderTopSpacing);

            // Apply background color when collapsed
            if (!isExpanded)
            {
                GUI.backgroundColor = InactiveHeaderColor;
            }

            GUILayout.BeginHorizontal();
            GUI.changed = false;

            // Format header text with styling
            string formattedText = $"<b><size=11>{text}</size></b>";
            formattedText = isExpanded ? $"\u25BC {formattedText}" : $"\u25BA {formattedText}";

            // Draw toggle-able header
            bool newState = GUILayout.Toggle(true, formattedText, "dragtab", GUILayout.MinWidth(20f));
            if (newState != isExpanded)
            {
                isExpanded = newState;
                EditorPrefs.SetBool(editorPrefsKey, isExpanded);
            }

            GUILayout.Space(2f);
            GUILayout.EndHorizontal();

            // Reset background color
            GUI.backgroundColor = Color.white;

            // Add spacing when collapsed
            if (!isExpanded)
            {
                GUILayout.Space(HeaderBottomSpacing);
            }

            return isExpanded;
        }

        /// <summary>
        /// Begins a content area with proper indentation and spacing.
        /// Must be paired with EndContents().
        /// </summary>
        /// <example>
        /// EditorTools.BeginContents();
        /// // Draw your content here
        /// EditorTools.EndContents();
        /// </example>
        public static void BeginContents()
        {
            GUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
            GUILayout.Space(ContentLeftSpacing);
            GUILayout.BeginVertical();
            GUILayout.Space(ContentTopSpacing);
        }

        /// <summary>
        /// Ends a content area and applies proper spacing.
        /// Must be paired with BeginContents().
        /// </summary>
        /// <example>
        /// EditorTools.BeginContents();
        /// // Draw your content here
        /// EditorTools.EndContents();
        /// </example>
        public static void EndContents()
        {
            GUILayout.Space(ContentBottomSpacing);
            GUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(ContentBottomSpacing);
        }

        /// <summary>
        /// Draws a horizontal line separator in the inspector
        /// </summary>
        /// <param name="height">Height of the separator line in pixels</param>
        public static void DrawSeparator(float height = 1f)
        {
            Rect rect = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));
            Color originalColor = GUI.color;
            GUI.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = originalColor;
        }

        /// <summary>
        /// Draws a help box with consistent styling
        /// </summary>
        /// <param name="message">The help message to display</param>
        /// <param name="messageType">The type of message (Info, Warning, Error)</param>
        public static void DrawHelpBox(string message, MessageType messageType = MessageType.Info)
        {
            EditorGUILayout.HelpBox(message, messageType);
        }

        /// <summary>
        /// Creates a section with a colored background for grouping related fields
        /// </summary>
        /// <param name="backgroundColor">Background color for the section</param>
        /// <param name="padding">Padding around the content</param>
        public static void BeginSection(Color backgroundColor, int padding = 10)
        {
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;
            GUILayout.BeginVertical("Box", GUILayout.ExpandWidth(true));
            GUILayout.Space(padding);
            GUI.backgroundColor = originalColor;
        }

        /// <summary>
        /// Ends a colored background section
        /// </summary>
        /// <param name="padding">Padding around the content</param>
        public static void EndSection(int padding = 10)
        {
            GUILayout.Space(padding);
            GUILayout.EndVertical();
        }
    }
}