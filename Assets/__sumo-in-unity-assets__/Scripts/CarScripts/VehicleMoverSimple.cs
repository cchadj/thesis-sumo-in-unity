using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using RiseProject.Tomis.CustomTypes;
using RiseProject.Tomis.SumoInUnity.SumoTypes;

using UnityEngine;

namespace RiseProject.Tomis.VehicleControl
{
    [RequireComponent(typeof(CarVisualController))]
    public class VehicleMoverSimple : VehicleMoverByTransform
    {        
        protected override void Awake()
        {
            base.Awake();

            NumberOfStepsUsedForPathCalculation = 0;
            numberOfLagSteps = 0;
            WantToSubscribeToVehicle = true;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!SumoVehicle)
                return;
            SumoVehicle.VehicleArrived += SumoVehicleVehicleArrived;
            SumoVehicle.VehicleExitedContextRange += SumoVehicleVehicleArrived;
            transform.localPosition = SumoVehicle.Position;
            transform.localRotation = Quaternion.Euler(0f, SumoVehicle.Angle, 0f);
        }

        private void SumoVehicleVehicleArrived(object sender, Vehicle.VehicleArgs e)
        {
            OnReachedCurrentDestination();
        }


        protected override void OnDisable()
        {
            base.OnDisable();
            if (SumoVehicle)
            {
                SumoVehicle.VehicleArrived -= SumoVehicleVehicleArrived;
                SumoVehicle.VehicleExitedContextRange -= SumoVehicleVehicleArrived;
            }

        }

        protected override void Vehicle_VehicleTransformChanged(object sender, Vehicle.VehicleArgs e)
        {
            base.Vehicle_VehicleTransformChanged(sender, e);
            
            var thisTransform = transform;
            
            var newTransform = e.VehicleTransform;
            var oldPosition = thisTransform.localPosition;
            var newPosition = newTransform.position;

            var dist = Vector3.Distance(oldPosition, newPosition);
            VisualController.speed = dist / (TimeSinceLastVehicleTransformChange/1000f);
            
            thisTransform.localPosition = new Vector3(newTransform.position.x, /* CarRayCaster.FrontRayHitInfo.point.y + */ VehicleConfig.SharedVehicleData.GroundOffset, newTransform.position.z);
            thisTransform.localRotation = newTransform.rotation;
        }
    }
}