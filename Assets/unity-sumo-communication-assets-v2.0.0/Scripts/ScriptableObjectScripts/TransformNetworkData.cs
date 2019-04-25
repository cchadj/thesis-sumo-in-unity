using RiseProject.Tomis.Util.Serializable;
using Tomis.Utils.Unity;
using UnityEngine;

namespace RiseProject.Tomis.DataHolders
{
    [CreateAssetMenu]
    public class TransformNetworkData : SingletonScriptableObject<TransformNetworkData>
    {
        /// <summary> A map for lane ids to their transforms </summary>
        //SerializeField
        [Tooltip("A map bettween lane ID and it's corresponding GameObjects")]
        private IDtoGameObjectsDictionary _laneIDGameObjectPairs;
        //[SerializeField]
        [Tooltip("A map bettween vehicle ID and it's corresponding GameObject")]
        private IDtoGameObjectDictionary _vehicleGameObjects;

        public IDtoGameObjectsDictionary LaneIDGameObjectPairs { get => _laneIDGameObjectPairs; set => _laneIDGameObjectPairs = value; }
        public IDtoGameObjectDictionary VehicleGameObjects { get => _vehicleGameObjects; set => _vehicleGameObjects = value; }
    }

}
