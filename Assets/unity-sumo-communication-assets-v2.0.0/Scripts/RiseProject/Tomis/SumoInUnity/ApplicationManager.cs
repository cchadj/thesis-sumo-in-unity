﻿using RiseProject.Tomis.DataHolders;
using Tomis.Utils.Unity;
using UnityEngine;

namespace RiseProject.Tomis.SumoInUnity
{
    [DefaultExecutionOrder(-400)]
    [RequireComponent(typeof(SumoClient), typeof(VehicleSimulator))]
    public class ApplicationManager : SingletonMonoBehaviour<ApplicationManager>
    {
        private VehicleSimulator Simulator { get; set; }
        private SumoClient Client { get; set; }

        [field: SerializeField] public bool DontUseVehicleSimulator { get; set; }

        [field: SerializeField] public bool DontUseSumo { get; set; }

        [field: SerializeField] private bool UseReporter { get; set; }
        private SimulationStartupData StartupData { get; set; }

        private void Awake()
        {
            Client = GetComponent<SumoClient>();
            Simulator = GetComponent<VehicleSimulator>();

            StartupData = SimulationStartupData.Instance;

            if (StartupData && StartupData.UseStartupData)
            {
                DontUseSumo = StartupData.dontUseSumo;
                DontUseVehicleSimulator = StartupData.dontUSeVehicleSimulator;
 
                UseReporter = StartupData.useReporter;
            }
        }

        /// <summary>
        /// Start Sumo Server and connect the client to it
        /// </summary>
        private void Start()
        {
            var reporter = (Reporter) FindObjectOfType(typeof(Reporter));
            if (!UseReporter && reporter)
            {
                reporter.enabled = true;
                reporter.gameObject.SetActive(false);
            }
            else if (UseReporter && !reporter)
            {
                var newReporterGo = new GameObject("reporter");
                newReporterGo.AddComponent<Reporter>();
                newReporterGo.AddComponent<ReporterMessageReceiver>();
            }

            /* Want to keep my Step functions clean of if statements*/
            if (!DontUseSumo)
            {
                
                
                Client.ServeSumo();
                System.Threading.Thread.Sleep(2000);

                // Wait a bit before connecting to sumo to make sure that we are connected;
                Client.ConnectToSumo();
                
                InvokeRepeating(DontUseVehicleSimulator ? 
                        nameof(StepNoSimulator) : nameof(Step), 0f,
                    Client.StepLength);
            }
            else
            {
                if (!DontUseVehicleSimulator)
                    InvokeRepeating(nameof(StepNoSumo), 0f, Client.StepLength);
            }
        }

        public void TerminateSimulation()
        {
            Debug.Log("Closing Application and Gui");
            if (Client != null)
                Client.Terminate();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }

        private void Step()
        {
            Client.MakeSimulationStep();
            Simulator.UpdateVehiclePositions();
        }

        /// <summary>
        /// Used for testing purposes to fix and test vehicle movement when sumo is not available
        /// </summary>
        private void StepNoSumo()
        {
            Simulator.UpdateVehiclePositions();
        }

        /// <summary>
        /// Used for testing purposes to see how much does rendering the vehicles affects the fps.
        /// </summary>
        private void StepNoSimulator()
        {
            Client.MakeSimulationStep();
        }
    }
}