using RiseProject.Tomis.DataContainers;
using RiseProject.Tomis.SumoInUnity.SumoTypes;
using RiseProject.Tomis.Util.TraciAuxilliary;
using System;
using UnityEngine;

namespace RiseProject.Tomis.VehicleControl
{

    /// <summary>
    /// Uses the Vehicle and CarVisualController 
    /// </summary>
    [RequireComponent(typeof(CarVisualController), typeof(Car))]
    public class VehicleLightController : MonoBehaviour
    {
        private CarVisualController _visualController;

        private Vehicle _vehicle;

        private Car _vehicleConfig;

        private void Awake()
        {  
            _vehicleConfig = GetComponent<Car>();
            _visualController = GetComponent<CarVisualController>();
        }

        private void OnEnable()
        {
            _vehicle = _vehicleConfig.TraciVariable;
            if (!_vehicle)
                throw new Exception("Don't forget to assign vehicle in VehicleConfigurationData in Vehicle Simulator");
        }

        // Update is called once per frame
        private void Update()
        {
            var curSignal = _vehicle.Signal;
            _visualController.leftBlink = TraCIAuxiliaryMethods.IsLeftBlinkerOn(curSignal);

            _visualController.rightBlink = TraCIAuxiliaryMethods.IsRightBlinkerOn(curSignal);

            _visualController.frontLights = TraCIAuxiliaryMethods.IsEmergencyLightOn(curSignal);

            //if (TraCIAuxiliaryMethods.IsEmergencyLightOn(curSignal))
            //{
            //    CarController.RightBlink = true;
            //    CarController.LeftBlink = true;
            //}
            //else
            //{
            //    CarController.RightBlink = false;
            //    CarController.LeftBlink = false;
            //}
        }
    }

}