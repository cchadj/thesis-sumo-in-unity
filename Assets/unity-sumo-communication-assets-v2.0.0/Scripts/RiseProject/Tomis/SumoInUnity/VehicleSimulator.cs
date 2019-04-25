﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RiseProject.Tomis.DataHolders;
using RiseProject.Tomis.SumoInUnity.MVC;
using RiseProject.Tomis.SumoInUnity.SumoTypes;
using RiseProject.Tomis.Util.Serializable;
using RiseProject.Tomis.VehicleControl;
using Tomis.Utils.Unity;
using UnityEngine;

namespace RiseProject.Tomis.SumoInUnity
{
    public class VehicleSimulator : SingletonMonoBehaviour<VehicleSimulator>
    {   
        private IDtoGameObjectDictionary GameObjectByVehicleId { get; set; }
        private Dictionary<string, Transform> _vehiclesEligibleForDeletion;
        private Dictionary<string, VehicleMover> _vehicleMoverByVehicleId;
        private Dictionary<string, VehicleLightController> _lightControllerByVehicleId;
        private Dictionary<string, FadeOutAll> _renderFaderById;

        private Transform _roadNetwork;

        [SerializeField, Tooltip("The prefab used for visualising the vehicles"), Rename("Vehicle Prefab")]
        private Transform vehiclePrefab;
        
        private Queue<Transform> VehicleTransformPool { get; } = new Queue<Transform>();


        private SumoNetworkData _sumoNetworkData;

        private SumoNetworkData SumoNetworkData
        {
            get
            {
                if (_sumoNetworkData == null)
                    _sumoNetworkData = SumoNetworkData.Instance;
                return _sumoNetworkData;
            }
            set => _sumoNetworkData = value;
            
        }

        private TransformNetworkData _transformNetworkData = null;
        private TransformNetworkData TransformNetworkData {
            get
            {
                if (_transformNetworkData == null)
                    _transformNetworkData = TransformNetworkData.Instance;
                
                if (_transformNetworkData == null)

                    throw new NullReferenceException("Could not retrieve TransformNetworkData singleton ");
                return _transformNetworkData;
            }
            set => _transformNetworkData = value;
        }
        private SharedVehicleData SharedVehicleData { get; set; }
        private SumoClient Client { get; set; }
        private bool _onlyShowContextRangeVehicles;
        
        public Transform GeneratedVehiclesParent { private get; set; }

        private int _poolSize = 200;

        private void Awake()
        {
            var startUpData = SimulationStartupData.Instance;

            if (startUpData && startUpData.UseStartupData && startUpData.dontUSeVehicleSimulator)
            {
                enabled = false;
                return;
            }
            
            Client = SumoClient.Instance;
            _onlyShowContextRangeVehicles = Client.UseContextSubscription;
            
            TransformNetworkData = TransformNetworkData.Instance;
            SumoNetworkData = SumoNetworkData.Instance;
            SharedVehicleData = SharedVehicleData.Instance;
            
            if( GetComponent<ApplicationManager> ().DontUseVehicleSimulator)
                return;
            
            _roadNetwork = GameObject.FindGameObjectsWithTag("GeneratedRoadNetwork")[0].transform;

            /* Generate an empty gameobject for the instantiated vehicles */
            var vehiclesEmptyGameObject = new GameObject();
            GeneratedVehiclesParent = vehiclesEmptyGameObject.transform;
            GeneratedVehiclesParent.name = "GeneratedVehicles";
            GeneratedVehiclesParent.transform.position = _roadNetwork.position;
            GameObjectByVehicleId = new IDtoGameObjectDictionary();
            _vehiclesEligibleForDeletion = new Dictionary<string, Transform>();
            _vehicleMoverByVehicleId = new Dictionary<string, VehicleMover>();
            _lightControllerByVehicleId = new Dictionary<string, VehicleLightController>();
            _renderFaderById = new Dictionary<string, FadeOutAll>();

            if(TransformNetworkData != null)
                TransformNetworkData.VehicleGameObjects = GameObjectByVehicleId;
            
            PopulateVehicleTransformPool(_poolSize);
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
                var newVehicleTransform = Instantiate(vehiclePrefab, new Vector3(), Quaternion.Euler(Vector3.zero));

                newVehicleTransform.GetComponent<CarVisualController>().CarHullColor = CarColors[
                (int) UnityEngine.Random.Range(0, CarColors.Count - 1)];
                var mover = newVehicleTransform.GetComponent<VehicleMover>();
                
                if (mover)
                    mover.enabled = false;
                
                var lc = newVehicleTransform.GetComponent<VehicleLightController>();
                
                if (lc)
                    lc.enabled = false;
                
                GameObject o;
                (o = newVehicleTransform.gameObject).SetActive(false);
                o.layer = 11;
                
                newVehicleTransform.parent = GeneratedVehiclesParent.transform;
                VehicleTransformPool.Enqueue(newVehicleTransform);
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
                vehiclesEntered = SumoNetworkData.VehiclesEnteredContextRange;
                vehiclesLeft = SumoNetworkData.VehiclesExitedContextRange;
            }
            else
            {
                vehiclesEntered = SumoNetworkData.VehiclesDepartedShared;
                vehiclesLeft = SumoNetworkData.VehiclesArrivedShared;
            }
            
            /* Vehicles that reached their destination in the simulation become eligible for deletion   */
            foreach (var idVehiclePair in vehiclesLeft)
            {
                var vehicle = idVehiclePair.Value;
                
                var isVehicleFound = GameObjectByVehicleId.TryGetValue(idVehiclePair.Key, out var vehicleGameObjectToDestroy);
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

                GameObjectByVehicleId.Add(departedVehicle.Key, newVehicleTransform.gameObject);

                // ------------------------ ADD AND SETUP SCRIPTS TO NEW GAME OBJECT ------------------------ //
                var vehicleConfiguration = newVehicleTransform.GetComponent<VehicleConfigurationData>();
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
                    _vehicleMoverByVehicleId.Add(id, vehicleMover);
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
                    _lightControllerByVehicleId.Add(id, vehicleLightControl);
                    vehicleLightControl.enabled = true;
                }

                var renderedFader = newVehicleTransform.gameObject.AddComponent<FadeOutAll>();
                _renderFaderById.Add(id, renderedFader);
            }
            vehiclesEntered.Clear();
        }

        private void VehicleMoverReachedCurrentDestination(object sender, VehicleEventArgs e)
        {
            /* Attempt to disable vehicle if the vehicle reached in destination both in unity
            and in sumo. If the vehicle is disabled then unsubscribes from the vehicle mover */
            if (DisableVehicle(e.id))
                ((VehicleMover) sender).ReachedCurrentDestination -= VehicleMoverReachedCurrentDestination;
        }

        private Transform RetrieveVehicleTransform()
        {
            Transform newVehicleTransform;
            if(VehicleTransformPool.Any())
               newVehicleTransform = VehicleTransformPool.Dequeue();
            else
            {
                PopulateVehicleTransformPool(_poolSize);
                newVehicleTransform = VehicleTransformPool.Dequeue();
            }

            /* REMEMBER TO SET POOL RETRIEVED TRANSFORM ACTIVE AND THEN DEACTIVATE IT ON DESTRUCTION */
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
                var vehicleMover = _vehicleMoverByVehicleId[id];
                var lightControl = _lightControllerByVehicleId[id];
                var vehicleConfigurationData = vehicleToDelete.GetComponent<VehicleConfigurationData>();
                vehicleConfigurationData.TraciVariable.Disable();
                
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

               
                StartCoroutine(DisableVehicle(vehicleToDelete, id));
            }
            return true;
        }

        /// <summary>
        /// Utility function used to fade out vehicle and remove the vehicle from the pool.
        /// </summary>
        /// <param name="vehicleToDelete"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private IEnumerator DisableVehicle(Transform vehicleToDelete, string id)
        {
            var fader = _renderFaderById[id];
            fader.Fade(0f);

            yield return new WaitForSeconds(fader.duration);

            vehicleToDelete.gameObject.SetActive(false);
            GameObjectByVehicleId.Remove(id);
            _lightControllerByVehicleId.Remove(id);
            _vehicleMoverByVehicleId.Remove(id);
            _renderFaderById.Remove(id);
            VehicleTransformPool.Enqueue(vehicleToDelete);
            yield return null;
        }
    }
}