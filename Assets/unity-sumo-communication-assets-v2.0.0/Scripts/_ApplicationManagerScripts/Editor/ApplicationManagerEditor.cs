using UnityEngine;
using UnityEditor;
using System;
using RiseProject.Tomis.SumoInUnity;

namespace RiseProject.Tomis.Editors
{
    [CustomEditor(typeof(ApplicationManager))]
    public class ApplicationManagerEditor : Editor
    {
        private ApplicationManager AppManager { get { return target as ApplicationManager; } }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(AppManager, "ApplicationManager");
            Undo.RecordObject(this, "thisEditor");
            EditorGUILayout.LabelField(new GUIContent("DEBUG and Profiling settings"), EditorStyles.boldLabel);
            AppManager.DontUseVehicleSimulator = (bool)EditorGUILayout.Toggle(
                 new GUIContent
                 {
                     text = "Don't Use Vehicle Simulator: ",
                     tooltip = " Stop rendering the vehicles. Used for testing purposes to see how much does the rendering of the vehicle affects the fps"
                 },
                 AppManager.DontUseVehicleSimulator);
            AppManager.DontUseSumo = (bool)EditorGUILayout.Toggle(
                 new GUIContent
                 {
                     text = "Don't Use SUMO: ",
                     tooltip = " Don't execute and use sumo. Used for testing when SUMO is not installed in the machine"
                 },
                 AppManager.DontUseSumo);
            EditorUtility.SetDirty(AppManager);
            EditorUtility.SetDirty(this);
        }
    }

}