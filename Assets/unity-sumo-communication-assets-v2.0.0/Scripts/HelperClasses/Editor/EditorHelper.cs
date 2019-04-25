using UnityEditor;
using UnityEngine;

namespace Tomis.UnityEditor.Utilities
{

    public static class EditorHelper
    {
        public static Rect RecordLastRect()
        {
                        
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.EndHorizontal();           
            EditorGUILayout.BeginVertical();
            EditorGUILayout.EndVertical();
            
            return GUILayoutUtility.GetLastRect();
        }
        /// <summary>
        /// Pass RecordLastRect 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="tooltip"></param>
        /// <param name="rect"></param>
        public static void CreateDivider(string text, string tooltip)
        {
            var rect = RecordLastRect();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider, GUILayout.MaxWidth(rect.width / 4), GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField(new GUIContent
            {
                text = text ,
                tooltip = tooltip
            },GUILayout.MaxWidth(rect.width / 2), GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider,GUILayout.MaxWidth(rect.width / 4), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
        }
    }
}

