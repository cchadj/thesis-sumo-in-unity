using UnityEngine;

using RiseProject.Tomis.SumoInUnity.SumoTypes;
using RiseProject.Tomis.SumoInUnity.MVC;

namespace RiseProject.Tomis.DataHolders
{
    /// <summary>
    /// Contains all the data needed from the simulation.
    /// Also contains displaying information such as offset ground .
    /// </summary>
    public class VehicleConfigurationData : TraciVariableData<Vehicle>
    {
        /// <summary> Contains configuration data for display purposes such as offset from the ground. </summary>

        private SharedVehicleData _sharedVehicleData;

        public SharedVehicleData SharedVehicleData
        {
            get
            {
                if (!_sharedVehicleData)
                    _sharedVehicleData = SharedVehicleData.Instance;
                return _sharedVehicleData;
            }
            set => _sharedVehicleData = value;
        }
    }

}
