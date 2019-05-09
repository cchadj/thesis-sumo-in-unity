using RiseProject.Tomis.SumoInUnity;
using Tomis.Utils.Unity;

using UnityEngine;
using UnityEngine.Serialization;

namespace RiseProject.Tomis.DataContainers
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
        public bool createFpsByVehicleCountPlot = true;
        public SubscriptionType subscriptionType;
        public bool useLocalSumo;
        public bool dontUseSumo;
        public bool dontUseVehicleSimulator;
        public bool useReporter;
        public float stepLength = 0.03f;
        public bool useMultithreading;
        public bool createSimStepDelayByVehicleCount;
        public SumoProcessRedirectionMode redirectionMode;
    }
}

