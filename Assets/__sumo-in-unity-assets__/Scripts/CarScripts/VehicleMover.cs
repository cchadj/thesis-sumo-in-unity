using System;
using System.Diagnostics;
using UnityEngine;

using RiseProject.Tomis.SumoInUnity.SumoTypes;
using RiseProject.Tomis.DataContainers;
using Debug = UnityEngine.Debug;

namespace RiseProject.Tomis.VehicleControl
{
    public class VehicleEventArgs : EventArgs
    {
        public readonly string id;

        public VehicleEventArgs(string id = "notAssigned")
        {
            this.id = id;
        }
    }

    [RequireComponent(typeof(VehicleConfigurationData))]
    public class VehicleMover : MonoBehaviour,  IVehicleMoverGettable
    {
        private Stopwatch _recordTimeSinceLastVehTransformChangeSw;
        protected float TimeSinceLastVehicleTransformChange { get; private set; }

        /// <summary>
        /// Manages wheels, indicators e.t.c.
        /// </summary>
        protected CarVisualController VisualController { get; private set; }
        
        /// <summary>
        /// Raised whenever the Car reaches the current destination.
        /// </summary>
        public event EventHandler<VehicleEventArgs> ReachedCurrentDestination;

        private Vehicle _vehicle;

        private bool _wantToSubscribeToVehicle;

        private int _numberOfStepsUsedForPathCalculation;

        /// <summary>
        /// Contains all the information needed for the simulation. (Contains the Vehicle and the SharedVehicleData)
        /// </summary>
        protected VehicleConfigurationData VehicleConfig { get; private set; }

        /// <summary> The vehicle from the simulation. Used to poll data from. </summary>
        protected Vehicle SumoVehicle { get => _vehicle; private set => _vehicle = value; }
        
        protected SharedVehicleData SharedVehicleData { get; set; }

        /// <summary> 
        /// The number of steps the Vehicle should lag behind. 
        /// Used for smoothing and calculating route.
        /// The simulation vehicle will be _numberOfLagSteps ahead of this car.
        /// </summary>
        [field: SerializeField, ReadOnly]
        public int NumberOfLagSteps { get; protected set; }

        /// <summary>
        /// True if is subscribed to the vehicle change tranform event.
        /// <see cref="Vehicle_VehicleTransformChanged"/> is called whenever a new position and orientation
        /// is polled from SUMO.
        /// </summary>
        protected bool IsSubscribedToVehicle { get; private set; } = false;

        /// <summary>
        /// Whether the VehicleMover wants to be subscribed to the vehicle.
        /// SubscribesToVehicle if VehicleMover wants to be subscribed to it and 
        /// unsubscribe from it doesn't want to be subscribed to it.
        /// </summary>
        public bool WantToSubscribeToVehicle
        {
            get => _wantToSubscribeToVehicle;
            set
            {
                _wantToSubscribeToVehicle = value;

                if (IsSubscribedToVehicle && !_wantToSubscribeToVehicle)
                    UnsubscribeFromVehicle();
                else if (!IsSubscribedToVehicle && _wantToSubscribeToVehicle)
                    SubscribeToVehicle();
            }
        }

        /// <summary>
        /// The amount of steps used for calculating smoother curves.
        /// Asks the Vehicle to cache _numberOfStepsForCalculation of positions.
        /// </summary>
        public int NumberOfStepsUsedForPathCalculation
        {
            get => _numberOfStepsUsedForPathCalculation;
            set
            {
                _numberOfStepsUsedForPathCalculation = value;

                if (SumoVehicle && SumoVehicle.NumberOfPositionsToSave != _numberOfStepsUsedForPathCalculation)
                {
                    SumoVehicle.NumberOfPositionsToSave = _numberOfStepsUsedForPathCalculation;
                }
            }
        }

        /// <summary>
        /// GetComponents and Setup properties such as <see cref="NumberOfStepsUsedForPathCalculation"/>,
        /// <see cref="NumberOfLagSteps"/> e.t.c
        /// </summary>
        protected virtual void Awake()
        {
            // Rotate wheels manually by setting the speed of the visual controller
            VisualController = GetComponent<CarVisualController>();
            VehicleConfig = GetComponent<VehicleConfigurationData>();
            
            SumoVehicle = VehicleConfig.TraciVariable;
            SharedVehicleData = VehicleConfig.SharedVehicleData;
            
            _recordTimeSinceLastVehTransformChangeSw = new Stopwatch();
        }

        private bool _isFirstEnable = true;
        protected virtual void OnEnable()
        {
            _recordTimeSinceLastVehTransformChangeSw.Reset();
            
            SumoVehicle = VehicleConfig.TraciVariable;

            if (SumoVehicle == null)
            {
                enabled = false;

                if (_isFirstEnable)
                {
                    _isFirstEnable = false;
                    return;
                }
                    throw new System.Exception("VehicleMover::OnEnable() No Vehicle assigned");
            }

            // Ask Vehicle to cache _numberOfStepsForCalculation positions.
            if (SumoVehicle.NumberOfPositionsToSave != NumberOfStepsUsedForPathCalculation)
                SumoVehicle.NumberOfPositionsToSave = NumberOfStepsUsedForPathCalculation;

            if (SharedVehicleData == null) Debug.LogError("VehicleMover::OnEnable() No SharedVehicleData assigned");

            if (!IsSubscribedToVehicle && WantToSubscribeToVehicle)
                SubscribeToVehicle();
        }

        protected virtual void OnDisable()
        {
            if (IsSubscribedToVehicle)
                UnsubscribeFromVehicle();
        }

        protected virtual void UnsubscribeFromVehicle()
        {
            SumoVehicle.VehicleTransformChanged -= Vehicle_VehicleTransformChanged;
            IsSubscribedToVehicle = false;
        }

        protected virtual void SubscribeToVehicle()
        {
            if(SumoVehicle)
            {
                SumoVehicle.VehicleTransformChanged += Vehicle_VehicleTransformChanged;
                IsSubscribedToVehicle = true;
            }
        }

        /// <summary>
        /// Meant to be abstract but I can not GetComponent on abstract classes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Vehicle_VehicleTransformChanged(object sender, Vehicle.VehicleArgs e)
        {
            TimeSinceLastVehicleTransformChange = _recordTimeSinceLastVehTransformChangeSw.ElapsedMilliseconds;
            _recordTimeSinceLastVehTransformChangeSw.Restart();
        }
        
        /// <summary>
        /// This event should be raised when the vehicle reaches it's current destination in unity.
        /// Returns true if the vehicle was removed by the Vehicle Simulator.
        /// <see cref="VehicleSimulator"/> is subscribed to this event to remove the vehicle if it eligible for deletion.
        /// </summary>
        protected virtual void OnReachedCurrentDestination()
        {
            ReachedCurrentDestination?.Invoke(this, new VehicleEventArgs(_vehicle.ID));
        }
    }
}
