using System;
using System.Collections;
using RiseProject.Tomis.CustomTypes;
using UnityEditor;
using UnityEngine;


namespace RiseProject.Tomis.VehicleControl
{
    [RequireComponent(typeof(CarVisualController))]
    public class VehicleMoverByLinearInterpolation : VehicleMoverByInterpolation
    {

        [Header("Debug")] [SerializeField] private bool showGizmos;
        protected override void Awake()
        {
            base.Awake();

            NumberOfStepsUsedForPathCalculation = 0;
            numberOfLagSteps = 0;
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

        private Vector3 targetPos;

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
        protected override IEnumerator ChangeVehiclePositionAndOrientation(MyTransform startingTransform, MyTransform targetTransform)
        {
            var lerpRate = 0f;
            _isPositionChanged = false;

            targetPos = targetTransform.position;
            
            
            for (lerpRate = 0f; lerpRate < 1.0f; lerpRate += _lerpIncrease)
            {
                // Interpolate 
                var interpolatedPosition = Vector3.Lerp(startingTransform.position, targetTransform.position, lerpRate);
                var interpolatedAngle = Mathf.LerpAngle(startingTransform.angle,    targetTransform.angle, lerpRate);

                // Place
                Transform.localPosition = new Vector3(interpolatedPosition.x, Transform.localPosition.y, interpolatedPosition.z);
                Transform.localRotation = Quaternion.Euler(new Vector3(0, interpolatedAngle, 0));
                
                        
                // height from raycasting 
                //if (m_carRayCaster.FrontRayDidHit)
                //    transform.localPosition = new Vector3(curPosition.x,/* CarRayCaster.FrontRayHitInfo.point.y + */ VehicleConfig.SharedVehicleData.GroundOffset, curPosition.z);
               
                yield return new WaitForSeconds(_coroutineTime);
            }
            
            Transform.localPosition = new Vector3(targetTransform.position.x, /* CarRayCaster.FrontRayHitInfo.point.y + */ VehicleConfig.SharedVehicleData.GroundOffset, targetTransform.position.z);
            Transform.localRotation = targetTransform.rotation;

            _isPositionChanged = true;
        }


        private static readonly Vector3 CubeSize = Vector3.one * 2;
        private void OnDrawGizmos()
        {
            if(!showGizmos)
                return;
            
            Gizmos.color = Color.green;
            Gizmos.DrawCube(ParentTransform.TransformPoint(_startingTransform.position), CubeSize);
            
            Gizmos.color = Color.red;
            Gizmos.DrawCube(ParentTransform.TransformPoint(targetPos), CubeSize);
            
            Gizmos.color = Color.white;
            Gizmos.DrawCube(ParentTransform.TransformPoint(_targetTransform.position), CubeSize);
        }
    }
}

