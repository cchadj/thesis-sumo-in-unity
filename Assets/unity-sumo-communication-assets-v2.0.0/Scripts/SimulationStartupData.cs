using RiseProject.Tomis.SumoInUnity;
using Tomis.Utils.Unity;

using UnityEngine;
using UnityEngine.Serialization;

namespace RiseProject.Tomis.DataHolders
{
    [CreateAssetMenu(menuName = "DataPersistence/Startup data")]
    public class SimulationStartupData : SingletonScriptableObject<SimulationStartupData>
    {

        /// <summary>
        /// Whether application will use the data from this script to load.
        /// </summary>
        [field: SerializeField, Header("Debug Related"), Rename("Use Startup Data")] public bool UseStartupData { get; set; }
        [field: SerializeField, Rename("SumoConfiguration Filename")] public string SumoConfigFilename { get; set; }

        [Space(5), Header("Simulation Start up data")]
        [ReadOnly] public bool createFpsByVehicleCountPlot = true;
        [ReadOnly] public bool dontUseSumo;
        [ReadOnly] public bool dontUSeVehicleSimulator;
        [ReadOnly] public bool useReporter;
        [ReadOnly] public float stepLength = 0.03f;
        [ReadOnly] public bool useMultithreading;
        [ReadOnly] public bool createSimStepDelayByVehicleCount;
        [ReadOnly] public bool useContextSubscriptions;
        [ReadOnly] public SumoProcessRedirectionMode redirectionMode;
    }
}

