using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RiseProject.Tomis.DataContainers;
using RiseProject.Tomis.SumoInUnity.MVC;
using RiseProject.Tomis.SumoInUnity.SumoTypes;
using RiseProject.Tomis.Util.Serializable;
using RiseProject.Tomis.VehicleControl;
using Tomis.Utils.Unity;
using UnityEngine;
using Zenject;

namespace RiseProject.Tomis.SumoInUnity
{
    public class VehicleSimulator : SingletonMonoBehaviour<VehicleSimulator>
    {
        private IDtoGameObjectDictionary _gameObjectByVehicleId;
        private Dictionary<string, Transform> _vehiclesEligibleForDeletion = new Dictionary<string, Transform>();
        
        private Transform _roadNetwork;

        [SerializeField, Tooltip("The prefab used for visualising the vehicles"), Rename("Vehicle Prefab")]
        private GameObject vehiclePrefab;
        
        private readonly Queue<Transform> _vehicleTransformPool = new Queue<Transform>();

        private SumoNetworkData _sumoNetworkData;

        private SumoToUnityGameObjectMap _sumoToUnityGameObjectMap = null;
        
        private SharedVehicleData SharedVehicleData { get; set; }
        private SumoClient _client;
        private bool _onlyShowContextRangeVehicles;
        private Car.Factory _carFactory;
        
        public Transform GeneratedVehiclesParent { private get; set; }

        private const int PoolSize = 200;

            
        [Inject]
        private void Construct(
            SumoNetworkData networkData, 
            SumoClient client,
            Car.Factory carFactory,
            SumoToUnityGameObjectMap sumoToUnityGameObjectMap
            )
        {
            _sumoNetworkData = networkData;
            _client = client;
            _carFactory = carFactory;
            _sumoToUnityGameObjectMap = sumoToUnityGameObjectMap;
        }
        
        private void Awake()
        {
            var startUpData = SimulationStartupData.Instance;

            if (startUpData && startUpData.UseStartupData && startUpData.dontUseVehicleSimulator)
            {
                enabled = false;
                return;
            }
            
            _client = SumoClient.Instance;
            _onlyShowContextRangeVehicles = _client.SubscriptionType == SubscriptionType.Context;
            
            _sumoNetworkData = SumoNetworkData.Instance;
            SharedVehicleData = SharedVehicleData.Instance;
            
            if( GetComponent<ApplicationManager> ().DontUseVehicleSimulator)
                return;
            
            _roadNetwork = GameObject.FindGameObjectsWithTag("GeneratedRoadNetwork")[0].transform;

            /* Generate an empty gameobject for the instantiated vehicles */
            var vehiclesEmptyGameObject = new GameObject();
            GeneratedVehiclesParent = vehiclesEmptyGameObject.transform;
            GeneratedVehiclesParent.name = "GeneratedVehicles";
            GeneratedVehiclesParent.transform.position = _roadNetwork.position;
            _gameObjectByVehicleId = new IDtoGameObjectDictionary();
            _vehiclesEligibleForDeletion = new Dictionary<string, Transform>();
            
                _sumoToUnityGameObjectMap.VehicleGameObjects = _gameObjectByVehicleId;
            
            PopulateVehicleTransformPool(PoolSize);
        }

        private static List<Color> CarColors = new List<Color>
        {
            Color.black,
            Color.blue,
            Color.red,
            Color.cyan,
            Color.magenta,
            Color.grey,
            Color.green

        };
        
        private void PopulateVehicleTransformPool(int numberOfTransforms)
        {
            for (var i = 0; i < numberOfTransforms; i++)
            {

                var car = _carFactory.Create(vehiclePrefab);

                car.GetComponent<CarVisualController>().CarHullColor = CarColors[
                (int) UnityEngine.Random.Range(0, CarColors.Count - 1)];
                var mover = car.GetComponent<VehicleMover>();
                
                if (mover)
                    mover.enabled = false;
                
                var lc = car.GetComponent<VehicleLightController>();
                
                if (lc)
                    lc.enabled = false;
                
                GameObject o;
                (o = car.gameObject).SetActive(false);
                o.layer = 11;
                
                car.transform.parent = GeneratedVehiclesParent.transform;
                _vehicleTransformPool.Enqueue(car.transform);
            }
        }
        
        /// <summary>
        /// Create, Remove and update Vehicle Positions.
        /// </summary>
        public void UpdateVehiclePositions()
        {
            VehicleDictionary vehiclesEntered;
            VehicleDictionary vehiclesLeft;
            
            if (_onlyShowContextRangeVehicles)
            {
                vehiclesEntered = _sumoNetworkData.VehiclesEnteredContextRange;
                vehiclesLeft = _sumoNetworkData.VehiclesExitedContextRange;
            }
            else
            {
                vehiclesEntered = _sumoNetworkData.VehiclesDepartedShared;
                vehiclesLeft = _sumoNetworkData.VehiclesArrivedShared;
            }
            
            // Vehicles that reached their destination in the simulation become eligible for deletion
            foreach (var idVehiclePair in vehiclesLeft)
            {
                
                var vehicle = idVehiclePair.Value;

                if (vehicle.IsEligibleForDeletion)
                {
                   // A vehicle can be in vehiclesLeft more than one time and so if already is eligible for deletion continue 
                   continue;
                }
                
                var isVehicleFound = _gameObjectByVehicleId.TryGetValue(idVehiclePair.Key, out var vehicleGameObjectToDestroy);
                if (!isVehicleFound)
                {
                     Debug.LogError("Vehicle with id " + idVehiclePair.Key + " not found to be removed.");
                    continue;
                }

                vehicle.IsEligibleForDeletion = true;
                _vehiclesEligibleForDeletion.Add(idVehiclePair.Key, vehicleGameObjectToDestroy.transform);
                idVehiclePair.Value.VehicleState = Vehicle.SimulationState.Arrived;
            }

            /* Create Vehicle GameObjects for vehicles that have just entered the unity simulation  */
            foreach (var departedVehicle in vehiclesEntered)
            {
                var position = departedVehicle.Value.Position;
                var id = departedVehicle.Key;

                var newVehicleTransform = RetrieveVehicleTransform();
                newVehicleTransform.gameObject.name = "vehicle" + departedVehicle.Value.ID;
                newVehicleTransform.localPosition = position;
                newVehicleTransform.rotation = Quaternion.Euler(0f, departedVehicle.Value.Angle, 0f);

                var curVehicle = departedVehicle.Value;
                curVehicle.AttachedVehicleTransform = newVehicleTransform;
                // Attached the new transform to the Vehicle SO. Useful for testing. (SO = ScriptableObject)
                _gameObjectByVehicleId.Add(departedVehicle.Key, newVehicleTransform.gameObject);

                // ------------------------ ADD AND SETUP SCRIPTS TO NEW GAME OBJECT ------------------------ //
                var vehicleConfiguration = newVehicleTransform.GetComponent<Car>();
                vehicleConfiguration.SharedVehicleData = SharedVehicleData;
                vehicleConfiguration.TraciVariable = curVehicle;
                vehicleConfiguration.enabled = true;
                
                var vehicleController = newVehicleTransform.GetComponent<VehicleController>();
                vehicleController.TraCIVariable = curVehicle;

                var vehicleView = newVehicleTransform.GetComponent<VehicleView>();
                vehicleView.UpdateReferencesToMatchAttachedVehicle();
                vehicleView.Vehicle = curVehicle;

                var vehicleMover = newVehicleTransform.GetComponent<VehicleMover>();
                if (vehicleMover)
                {
                    //_vehicleMoverByVehicleId.Add(id, vehicleMover);
                    vehicleMover.ReachedCurrentDestination += VehicleMoverReachedCurrentDestination;
                    vehicleMover.enabled = true;
                }
                else
                {
                    throw new Exception("VehicleSimulator::UpdateVehiclePositions VehicleMover is null. Is the class abstract?" +
                        "Does the car have a VehicleMover?");
                }
                var vehicleLightControl = newVehicleTransform.GetComponent<VehicleLightController>();
                if (vehicleLightControl)
                {
                    //_lightControllerByVehicleId.Add(id, vehicleLightControl);
                    vehicleLightControl.enabled = true;
                }

                var renderedFader = newVehicleTransform.gameObject.AddComponent<FadeOutAll>();
                //_renderFaderById.Add(id, renderedFader);
            }
            vehiclesEntered.Clear();
        }

        private void VehicleMoverReachedCurrentDestination(object sender, VehicleEventArgs e)
        {
            // Attempt to disable vehicle if the vehicle reached in destination both in unity
            // and in sumo. If the vehicle is disabled then unsubscribes from the vehicle mover
            if (DisableVehicle(e.id))
                ((VehicleMover) sender).ReachedCurrentDestination -= VehicleMoverReachedCurrentDestination;
        }

        private Transform RetrieveVehicleTransform()
        {
            Transform newVehicleTransform;
            if(_vehicleTransformPool.Any())
               newVehicleTransform = _vehicleTransformPool.Dequeue();
            else
            {
                PopulateVehicleTransformPool(PoolSize);
                newVehicleTransform = _vehicleTransformPool.Dequeue();
            }

            // REMEMBER TO SET POOL RETRIEVED TRANSFORM ACTIVE AND THEN DEACTIVATE IT ON DESTRUCTION
            newVehicleTransform.gameObject.SetActive(true);
            return newVehicleTransform;
        }

        /// <summary>
        /// This is to be called when a car reaches it's destination.
        /// Check if the car is eligible for deletion and then delete it.
        /// </summary>
        /// <param name="id"> The car to attempt to destroy </param>
        /// <returns> true if the vehicle was disabled and false otherwise</returns>
        private bool DisableVehicle(string id)
        {
            if (!_vehiclesEligibleForDeletion.ContainsKey(id))
                return false;

            if (_vehiclesEligibleForDeletion.TryGetValue(id, out var vehicleToDelete))
            {
                // Deactivate it and put it back on queue
                var vehicleConfigurationData = vehicleToDelete.GetComponent<Car>();
                
                vehicleConfigurationData.TraciVariable.Disable();
                


                _vehiclesEligibleForDeletion.Remove(id);           

                StartCoroutine(DisableVehicle(vehicleToDelete, id));
            }
            return true;
        }

        
        // Cache Waitforseconds to reduce allocation
        private static WaitForSeconds WaitForSeconds = new WaitForSeconds(1f);
        
        /// <summary>
        /// Utility function used to fade out vehicle and remove the vehicle from the pool.
        /// </summary>
        /// <param name="vehicleToDelete"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private IEnumerator DisableVehicle(Transform vehicleToDelete, string id)
        {
            var car = _gameObjectByVehicleId[id];
            _gameObjectByVehicleId.Remove(id);

            var fader = car.GetComponent<FadeOutAll>();
            fader.Fade(0f);

            yield return WaitForSeconds; // new WaitForSeconds(fader.duration)
            
            var vehicleMover = car.GetComponent<VehicleMover> ();
            var lightControl = car.GetComponent<VehicleLightController>();
            var vehicleConfigurationData = car.GetComponent<Car>();
            
            vehicleToDelete.gameObject.SetActive(false);
            
            
            if (!vehicleMover)
                Debug.LogError($"VehicleSimulator::DestroyCar({id}) Vehicle with id {id} has no VehicleMover");
            else
                vehicleMover.enabled = false;

            if (!lightControl)
                Debug.LogError($"VehicleSimulator::DestroyCar({id}) Vehicle with id {id} has no LightControl");
            else
                lightControl.enabled = false;

            if (!vehicleConfigurationData)
                Debug.LogError($"VehicleSimulator::DestroyCar({id}) Vehicle with id {id} has no vehicleConfigurationData");
            else
                vehicleConfigurationData.enabled = false;
            
            
            _vehicleTransformPool.Enqueue(vehicleToDelete);
            yield return null;
        }
    }
}