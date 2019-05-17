using System;
using System.Collections;
using UnityEditor;
using UnityEngine;


namespace RiseProject.Tomis.VehicleControl
{
    [RequireComponent(typeof(CarVisualController))]
    public class VehicleMoverByLinearInterpolation : VehicleMoverByInterpolation
    {

        protected override void Awake()
        {
            base.Awake();

            NumberOfStepsUsedForPathCalculation = 0;
            numberOfLagSteps = 0;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }
        /// <summary>
        /// Sets the next target for this vehicle to reach in the next interpolation.
        /// </summary>
        protected override void SetTarget()
        {
            if (UseQueuedPositions)
            {
                var t = _vehiclePositionQueue.Dequeue();
                _targetTransform.position = t.position;
                _targetTransform.angle = t.angle;
                _targetTransform.rotation = t.rotation;
            }
            else
            {
                _targetTransform = SumoVehicle.PositionAndOrientation;
            }
        }

        /******************/
        /* Sir Bored-a-lot*/
        /*    /\/\/\      */
        /*    ||||||      */
        /*   ʕ ͡° ͜ʖ ͡°ʔ     */
        /*    \  _ /      */
        /*   __|  |__     */
        /*  //_     _\\   */
        /* // \    /  \\  */
        /* ||  |__|   ||  */
        /* WW  | ||  WW   */
        /*     | ||       */
        /*     | ||       */
        /*     |_||_      */
        /*     \__\_\     */
        /******************/
        /// <summary>
        /// Change a cars position from starting to target using a simple linear interpolation.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerator ChangeVehiclePositionAndOrientation()
        {
            float lerpRate = 0f;
            _isPositionChanged = false;
            
            for (lerpRate = 0f; lerpRate < 1.0f; lerpRate += _lerpIncrease)
            {
                /* Position interpolation */
                var curPosition = Vector3.Lerp(_startingTransform.position, _targetTransform.position, lerpRate);
                transform.localPosition = new Vector3(curPosition.x, transform.localPosition.y, curPosition.z);

                /* height from raycasting */
                //if (m_carRayCaster.FrontRayDidHit)
                //    transform.localPosition = new Vector3(curPosition.x,/* CarRayCaster.FrontRayHitInfo.point.y + */ VehicleConfig.SharedVehicleData.GroundOffset, curPosition.z);
                
                /* Rotation interpolation */
                float interpolatedAngle = Mathf.LerpAngle(_startingTransform.angle, _targetTransform.angle, lerpRate);
                transform.localRotation = Quaternion.Euler(new Vector3(0, interpolatedAngle, 0));

                yield return new WaitForSeconds(_coroutineTime);
            }
            
            transform.localPosition = new Vector3(_targetTransform.position.x, /* CarRayCaster.FrontRayHitInfo.point.y + */ VehicleConfig.SharedVehicleData.GroundOffset, _targetTransform.position.z);
            transform.localRotation = _targetTransform.rotation;

            _isPositionChanged = true;
        }
    }
}

