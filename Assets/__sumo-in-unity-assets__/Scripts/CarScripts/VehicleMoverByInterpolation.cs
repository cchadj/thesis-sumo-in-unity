using System;
using System.Collections;
using System.Collections.Generic;

using RiseProject.Tomis.CustomTypes;
using RiseProject.Tomis.SumoInUnity.SumoTypes;

using UnityEngine;


namespace RiseProject.Tomis.VehicleControl
{
    /// <summary>
    /// Meant to be abstract 
    /// </summary>
    [RequireComponent(typeof(CarVisualController))]
    public class VehicleMoverByInterpolation : VehicleMoverByTransform
    {
        [SerializeField] private bool _useQueuedPositions;
        [SerializeField] protected int _lerpSteps = 15;
        /// <summary>
        /// Calculate coroutine wait time only once.
        /// SimulationStepLength/ _lerpSteps
        /// </summary>
        protected float _coroutineTime;
        /// <summary>
        /// How much the lerprate will increase.
        /// 1.0f / _lerpSteps
        /// </summary>
        protected float _lerpIncrease;                     
        protected Queue<MyTransform> _vehiclePositionQueue;

        /// <summary> </summary>
        protected bool _isPositionChanged = false;
        /// <summary> The position and orientation that is used initialy. for the interpolation. </summary>
        protected MyTransform _startingTransform = new MyTransform();
        /// <summary> The position and orientation should be at the end of the interpolation. </summary>
        protected MyTransform _targetTransform = new MyTransform();

        public int LerpSteps { get => _lerpSteps; set => _lerpSteps = value; }

        protected bool UseQueuedPositions
        {
            get => _useQueuedPositions;
            set
            {
                if (_vehiclePositionQueue == null)
                    _vehiclePositionQueue = new Queue<MyTransform>();

                _useQueuedPositions = value;

                if (_useQueuedPositions == true && !IsSubscribedToVehicle)
                    SubscribeToVehicle();

                if (_useQueuedPositions == false && IsSubscribedToVehicle)
                    UnsubscribeFromVehicle();
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _coroutineTime = SharedVehicleData.SimulationStepLength / _lerpSteps;
            _lerpIncrease = 1.0f / _lerpSteps;
            WantToSubscribeToVehicle = UseQueuedPositions;
        }

        //TODO handle all cases of enabling and disabling UseQueuedPositionos
        // Use on enable because the same script will be enabled/disabled multiple times.
        // Start() and Awake() are called exactly one time.(When the game is instansiated)
        protected override void OnEnable()
        {
            // Initialising data used for polling
            base.OnEnable();

            _isPositionChanged = true;

            transform.localPosition = SumoVehicle.Position;
            transform.localRotation = Quaternion.Euler(0f, SumoVehicle.Angle, 0f);

            /* Subscription happened on base.OnEnable so we don't have to check for null
             since the queue is initialised there */
            if(UseQueuedPositions)
                _vehiclePositionQueue.Enqueue(SumoVehicle.PositionAndOrientation);


        }

        protected override void UnsubscribeFromVehicle()
        {
            base.UnsubscribeFromVehicle();
            _vehiclePositionQueue?.Clear();
        }

        protected override void SubscribeToVehicle()
        {
            base.SubscribeToVehicle();
            if (UseQueuedPositions && _vehiclePositionQueue == null)
                _vehiclePositionQueue = new Queue<MyTransform>();
            else if(_vehiclePositionQueue != null)
                _vehiclePositionQueue.Clear();
        }

        /// <summary>
        /// Save positional data in queue each time new vehicle data is polled from sumo.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void Vehicle_VehicleTransformChanged(object sender, Vehicle.VehicleArgs e)
        {
            _vehiclePositionQueue.Enqueue(e.VehicleTransform);
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            if (!SumoVehicle || !isActiveAndEnabled || SumoVehicle.StepsInSimulation < numberOfLagSteps)
                return;

            if (CarRayCaster.FrontRayDidHit && CarRayCaster.BackRayDidHit)
            {
                _frontHit = CarRayCaster.frontRayHitInfo;
                _backHit = CarRayCaster.backRayHitInfo;
            }

            VisualController.speed = SumoVehicle.Speed;

            var oldVehicleAngle = transform.localRotation;
            var curVehicleAngle = Quaternion.Euler(transform.localRotation.x, SumoVehicle.Angle * Time.deltaTime, transform.localRotation.z);

            if (Quaternion.Angle(oldVehicleAngle, curVehicleAngle) > 1f)
            {
                //float oldAngleYaxis = transform.localRotation.y;
                float newAngleYaxis = SumoVehicle.Angle;

                // get the signed difference in these angles
                float angleDiff = Mathf.DeltaAngle(SumoVehicle.PrevAngle, SumoVehicle.Angle);

                VisualController.steerOrientation = angleDiff < 0 ?
                        CarVisualController.Steer.Left : CarVisualController.Steer.Right;
            }
            else
                VisualController.steerOrientation = CarVisualController.Steer.None;
            
            /* https://stackoverflow.com/questions/40708014/difference-between-enabled-isactiveandenabled-and-activeinhierarchy-in-unity */
            if (_isPositionChanged)
            {
                OnReachedCurrentDestination();

                if (!enabled)
                    return;
                  
                _startingTransform.position = new Vector3(transform.localPosition.x, 0f, transform.localPosition.z);
                _startingTransform.angle = transform.localRotation.eulerAngles.y;

                SetTarget();

                StartCoroutine(ChangeVehiclePositionAndOrientation());
            }
        }

        /// <summary>
        /// Meant to be abstract but I can not use GetComponent on 
        /// abstract classes so sad.
        /// </summary>
        /// <remars> Meant to be abstract </remars>
        /// <returns></returns>
        protected virtual IEnumerator ChangeVehiclePositionAndOrientation()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks> Meant to be abstract </remarks>
        protected virtual void SetTarget()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Change Car Angle for turns and slopes.
        /// </summary>
        protected override void ChangeCarAngle()
        {
            return;
            Vector3 frontHitPos, backHitPos;
            Vector3 heading;
            Vector3 directionA;
            if (CarRayCaster.FrontRayDidHit && CarRayCaster.BackRayDidHit)
            {
                frontHitPos = _frontHit.point;
                backHitPos = _backHit.point;
                heading = (frontHitPos - backHitPos);
                directionA = heading / heading.magnitude;
                bool isPitchAngleNegative = frontHitPos.y > backHitPos.y;

                backHitPos.y = frontHitPos.y;

                heading = (frontHitPos - backHitPos);
                Vector3 directionB = heading / heading.magnitude;

                float pitchAngle = isPitchAngleNegative ? -Vector3.Angle(directionA, directionB) : Vector3.Angle(directionA, directionB);

                //Debug.Log("FrontHitPos y " + frontHitPos.y + "BackHitPosY" + backHitPos.y);
                if (frontHitPos.y > backHitPos.y)
                    pitchAngle = -pitchAngle;

                transform.localRotation = Quaternion.Euler(new Vector3(pitchAngle, SumoVehicle.Angle, 0f));
                Debug.DrawLine(transform.position, directionA, UnityEngine.Color.green);
                Debug.DrawLine(transform.position, directionB, UnityEngine.Color.green);
            }
            else
            {
                transform.localRotation = Quaternion.Euler(0f, SumoVehicle.Angle, 0f);
            }

        }

    }
}