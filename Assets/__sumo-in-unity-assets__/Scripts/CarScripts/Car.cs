using UnityEngine;

using RiseProject.Tomis.SumoInUnity.SumoTypes;
using RiseProject.Tomis.SumoInUnity.MVC;
using Zenject;

namespace RiseProject.Tomis.DataContainers
{
    /// <summary>
    /// Contains all the data needed from the simulation.
    /// Also contains displaying information such as offset ground .
    /// </summary>
    public class Car : TraciVariableData<Vehicle>
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

        public class Factory : PlaceholderFactory< GameObject, Car>
        {
            
        }
    }

}
