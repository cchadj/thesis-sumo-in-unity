using RiseProject.Tomis.DataHolders;
using UnityEditor;
using UnityEngine;

namespace RiseProject.Tomis.Editors
{
    [CustomEditor(typeof(SumoNetworkData))]
    public class SumoNetworkDataEditor : Editor
    {

        private SumoNetworkData NetworkData { get { return target as SumoNetworkData; } }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Add Mock Vehicle to departed Vehicles"))
            {
                NetworkData.AddMockVehicleToDeparted();
            }

            if (GUILayout.Button("Clear all dictionaries"))
            {
                NetworkData.ClearDictionaries();
            }

            if (GUILayout.Button("Clear vehicle dictionaries"))
            {
                NetworkData.ClearVehicleDictionaries();
            }
        }

    }
}