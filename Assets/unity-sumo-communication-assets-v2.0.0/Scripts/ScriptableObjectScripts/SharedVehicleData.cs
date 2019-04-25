using RiseProject.Tomis.SumoInUnity;
using Tomis.Utils.Unity;
using UnityEngine;

namespace RiseProject.Tomis.DataHolders
{
    [CreateAssetMenu]
    public class SharedVehicleData : SingletonScriptableObject<SharedVehicleData>
    {
        [SerializeField]
        private float _simulationStep;

        /// <summary> The offset from the ground. </summary>
        [field: SerializeField, Rename("Ground Offset")]
        public float GroundOffset { get; private set; }

       
        public VehicleSimulator VehicleSimulator { get;  set; }


        private void OnEnable()
        {
            VehicleSimulator = VehicleSimulator.Instance;
        }

        /// <summary> Simulated time between each step. To be used by VehicleMover  </summary>
        public float SimulationStepLength { get => _simulationStep; set => _simulationStep = value; }
    }

}
