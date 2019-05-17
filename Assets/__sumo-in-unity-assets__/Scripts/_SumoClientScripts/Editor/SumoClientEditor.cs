﻿using RiseProject.Tomis.SumoInUnity;
using System.IO;
using Tomis.ClientSide;
using UnityEditor;
using UnityEngine;
using Tomis.UnityEditor.Utilities;
using Tomis.Utils;
using UnityQuery;

namespace RiseProject.Tomis.Editors
{
    [CustomEditor(typeof(SumoClient))]
    public class SumoClientEditor : Editor
    {
        private SumoClient Client => target as SumoClient;

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(Client, "ApplicationManager");
            Undo.RecordObject(this, "thisEditor");

            var rect = Rect.zero;

            EditorHelper.CreateDivider("Connection Settings", "Connect to SUMO server settings");

            EditorGUILayout.BeginHorizontal();
            Client.Ip = EditorGUILayout.TextField(
                new GUIContent
                {
                    text = "IP: ",
                    tooltip = "Sumo server IP. Used to serve sumo aswell."
                }, Client.Ip);

            Client.RemotePort = EditorGUILayout.IntField(
                new GUIContent
                {
                    text = "Port: ",
                    tooltip = "Port where Sumo Server listens to. Used to serve sumo aswel."
                }, Client.RemotePort);            
            EditorGUILayout.EndHorizontal();
            
            EditorHelper.CreateDivider("Serve Sumo Settings", "Settings when creating process from C# code");

            Client.ShouldServeSumo = EditorGUILayout.Toggle(new GUIContent()
            {
                text = "Fire up sumo server?",
                tooltip = "Serve sumo and connect to it or connect to a different sumo server."
            }, Client.ShouldServeSumo);

            if (Client.ShouldServeSumo)
            {
                Client.UseLocalSumo = EditorGUILayout.Toggle(new GUIContent()
                {
                    text = "Use Local sumo executables?",
                    tooltip = "Use Sumo executables from the asset folder or from what is found at PATH?"
                }, Client.UseLocalSumo);
               
                EditorGUILayout.BeginHorizontal();
                Client.UseSumoGui = EditorGUILayout.Toggle(new GUIContent()
                {
                    text = "Show Sumo Gui",
                    tooltip = "To be used only for testing. Makes simulation considerably slower."
                }, Client.UseSumoGui);

                Client.NumberOfConnections = EditorGUILayout.IntField(new GUIContent()
                {
                    text = "Number Of Connections",
                    tooltip = "How many connections should the server support. (leave it at 1)"
                }, Client.NumberOfConnections);
                EditorGUILayout.EndHorizontal();

                Client.StepLength = EditorGUILayout.Slider(
                    new GUIContent
                    {
                        text = "Step Length: ",
                        tooltip = "How much time should each simulation step simulate (ms)"
                    }, Client.StepLength, 0.01f, 2f);

                Client.BeginStep = EditorGUILayout.IntField(new GUIContent
                {
                    text = "Begin Step: ",
                    tooltip = " the -b option in sumo.exe and sumo-gui.exe.  0 for the start "
                }, Client.BeginStep);

                Client.redirectionMode = (SumoProcessRedirectionMode) EditorGUILayout.EnumPopup(
                    new GUIContent
                    {
                        text = " Output Redirection Mode ",
                        tooltip = " How will the output of the SUMO process will be redirected "
                    },
                    Client.redirectionMode);


                if (GUILayout.Button("Select Sumo Configuration File"))
                {
                    var p =
                        Path.Combine(Application.streamingAssetsPath, "sumo-scenarios");
                    Debug.Log(p);

                    var filters = new[] {"Sumo configuration files", "sumocfg,sumo.cfg"};
                    Client.SumocfgFile = EditorUtility.OpenFilePanelWithFilters("SUMO configuration files",
                        p,
                        filters);
                }

                if (Client.SumocfgFile.IsNullOrEmpty())
                    EditorGUILayout.HelpBox("!WARNING: No sumocfg file selected!", MessageType.Warning);
                else
                    EditorGUILayout.LabelField(
                        new GUIContent
                        {
                            text = "Selected File: " + Path.GetFileName(Client.SumocfgFile),
                            tooltip = "Full Path: " + Client.SumocfgFile
                        }, EditorStyles.boldLabel);
            }

            EditorHelper.CreateDivider("Performance Settings",
                "Settings related to performance of reading polled data");

            Client.SubscriptionType = (SubscriptionType)EditorGUILayout.EnumPopup(
               new GUIContent
               {
                   text = "Subscription Type: ",
                   tooltip =
                       "Use Variable or Context subscriptions. Check sumo wiki for TraCI. "
               }, Client.SubscriptionType);

            Client.UseMultithreading = EditorGUILayout.Toggle(
                new GUIContent
                {
                    text = "Multithreading",
                    tooltip = "Use a different thread for Client Simulation Step"
                }, Client.UseMultithreading);


            EditorHelper.CreateDivider("Logging And Debugging Settings",
                "Settings about logging fps and debugging features");
            Client.CreateFpsByVehicleCountPlot = EditorGUILayout.Toggle(
                new GUIContent
                {
                    text = "Create a fps by vehicle_count plot",
                    tooltip = "Creates a csv with number of fps and count of vehicles.\n" +
                              "Also creates a plot with fps in Y axis and number of vehicles on X axis"
                }, Client.CreateFpsByVehicleCountPlot);


            Client.CreateSimStepExecutionTimeByVehicleCountPlot = EditorGUILayout.Toggle(
                new GUIContent
                {
                    text = "Create sim_step_execution_time by vehicle_count plot",
                    tooltip = "Makes a csv with  the time each sim step takes to execute and count of vehicles\n" +
                              "Also creates a plot with execution time in Y axis and number of vehicles on X axis"
                }, Client.CreateSimStepExecutionTimeByVehicleCountPlot);
            
            EditorUtility.SetDirty(Client);
            EditorUtility.SetDirty(this);
        }
    }
}