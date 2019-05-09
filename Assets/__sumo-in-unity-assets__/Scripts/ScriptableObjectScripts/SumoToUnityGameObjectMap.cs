using RiseProject.Tomis.Util.Serializable;
using Tomis.Utils.Unity;
using UnityEngine;

namespace RiseProject.Tomis.DataContainers
{
    [CreateAssetMenu(menuName = "DataContainers/SumoToUnityGameObjectMap")]
    public class SumoToUnityGameObjectMap : SingletonScriptableObject<SumoToUnityGameObjectMap>
    {
        /// <summary> A map for lane ids to their transforms </summary>

        [SerializeField, Tooltip("A map bettween lane ID and it's corresponding GameObject. If multiple gameobjects then the first one."), ReadOnly]
        private IDtoGameObjectDictionary _laneIDGameObjectPairs;

        [SerializeField, Tooltip("A map bettween vehicle ID and it's corresponding GameObject"), ReadOnly]
        private IDtoGameObjectDictionary _vehicleGameObjects;

        public IDtoGameObjectDictionary LaneIDGameObjectPairs { get => _laneIDGameObjectPairs; set => _laneIDGameObjectPairs = value; }
        public IDtoGameObjectDictionary VehicleGameObjects { get => _vehicleGameObjects; set => _vehicleGameObjects = value; }
    }

}
