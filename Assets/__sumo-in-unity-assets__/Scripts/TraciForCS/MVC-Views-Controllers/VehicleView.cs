using System;
using System.Globalization;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using RiseProject.Tomis.DataContainers;
using RiseProject.Tomis.SumoInUnity.SumoTypes;
using Zenject;

namespace RiseProject.Tomis.SumoInUnity.MVC
{
    /// <summary>
    /// Intended use. Here there will be texts and labels (UI canvas)
    /// to change the state of the vehicle
    /// </summary>
    [RequireComponent(typeof(Car), typeof(VehicleController))]
    public class VehicleView : SumoTypeView<VehicleController, Vehicle>
    {
        
        private CurrentlySelectedTargets _selectedTargets;
        
        #region initial text values
        private string _initialVehicleIDText;
        private string _initialSumoSpeedText;
        private string _initialActualSpeedText;
        private string _initialAccelerationText;
        private string _initialPositionText;
        private string _initialRouteIDText;
        private string _initialEdgeIDText;
        private string _initialLaneIDText;
        private string _initialLaneIndexText;
        private string _initialRouteEdgeListIDsText;

        #endregion initial text values
        #region UI Components        

        private Car _car;

        public TextMeshProUGUI VehicleIdText { get; set; }
        public TextMeshProUGUI SumoSpeedText { get; set; }
        private TextMeshProUGUI SumoAccelerationText { get; set; }
        private TextMeshProUGUI ActualSpeedText { get; set; }
        private TextMeshProUGUI PositionText { get; set; }
        private TextMeshProUGUI RouteIdText { get; set; }
        private TextMeshProUGUI EdgeIdText { get; set; }
        private TextMeshProUGUI LaneIdText { get; set; }
        private TextMeshProUGUI LaneIndexText { get; set; }
        private TextMeshProUGUI RouteDetailsRouteIdText { get; set; }

        private TextMeshProUGUI RouteEdgeListIDsText { get; set; }

        private TMP_InputField SetSpeedInputField { get; set; }
        private Button ApplySpeedButton { get; set;  }
        
        #endregion UI Components 



        public Vehicle Vehicle { get; set; }

        [Inject]
        private void Construct(
            CurrentlySelectedTargets selectedTargets,
            VehicleCanvas vehicleCanvas )
        {
            _selectedTargets = selectedTargets;
            Canvas = vehicleCanvas;
        }
        
        protected override void Awake()
        {
            VehicleIdText = Canvas.VehicleIdText;
            EdgeIdText = Canvas.EdgeIdText;
            LaneIdText = Canvas.LaneIdText;
            PositionText = Canvas.PositionText;
            SumoSpeedText = Canvas.SumoSpeedText;
            ActualSpeedText = Canvas.ActualSpeedText;
            SumoAccelerationText = Canvas.SumoAccelerationText;
            LaneIndexText = Canvas.LaneIdText;
            RouteEdgeListIDsText = Canvas.RouteEdgeListIDsText;
            RouteIdText = Canvas.RouteIdText;
            SetSpeedInputField = Canvas.SetSpeedInputField;
            RouteDetailsRouteIdText = Canvas.RouteDetailsRouteIdText;
            ApplySpeedButton = Canvas.ApplySpeedButton;
            
            _initialVehicleIDText = VehicleIdText.text;
            _initialEdgeIDText = EdgeIdText.text;
            _initialLaneIDText = LaneIdText.text;
            _initialPositionText = PositionText.text;
            _initialSumoSpeedText = SumoSpeedText.text;
            _initialRouteEdgeListIDsText = RouteEdgeListIDsText?.text;
            _initialRouteIDText = RouteIdText.text;
            _initialAccelerationText = SumoAccelerationText.text;

            enabled = false;
        }

        public void UpdateReferencesToMatchAttachedVehicle()
        {
            _car = GetComponent<Car>();
            Vehicle = _car.TraciVariable;
            Controller = GetComponent<VehicleController>();
        }

        private void Vehicle_PositionAndAngleChanged(object sender, System.EventArgs e)
        {
             UpdateMobilityTexts();
        }
        
        private void OnEnable()
        {
            if (!Canvas)
                return;
            
            Vehicle.VehicleTransformChanged += Vehicle_PositionAndAngleChanged;
            Vehicle.OnDispose += VehicleOnOnDispose;
            if(ApplySpeedButton)
                ApplySpeedButton.onClick.AddListener(ApplySpeed);
//            
//            if (_selectedTargets.selectedTransform == transform)
//                UpdateView();
        }

        private void VehicleOnOnDispose(object sender, EventArgs e)
        {
             Vehicle.VehicleTransformChanged -=  Vehicle_PositionAndAngleChanged;
             enabled = false;
        }

        private void OnDisable()
        {
            if (!VehicleIdText)
                return;
            
            if(!ApplySpeedButton)
                return;
            
            ApplySpeedButton.onClick.RemoveListener(ApplySpeed);
            if(Vehicle)
                Vehicle.VehicleTransformChanged -= Vehicle_PositionAndAngleChanged;
        }

        private void Update()
        {
            UpdateView();
        }

        public void ApplySpeed()
        {
            GetComponent<VehicleController>().SetSpeed(float.Parse(SetSpeedInputField.text, CultureInfo.InvariantCulture.NumberFormat));
        }

        protected override void UpdateView()
        {
            
            VehicleIdText?.SetText($"{_initialVehicleIDText} {Vehicle.ID}");
            RouteIdText?.SetText($"{_initialRouteIDText} {Vehicle.RouteId}");
            EdgeIdText?.SetText($"{_initialEdgeIDText} {Vehicle.EdgeId}");
            LaneIdText?.SetText($"{_initialLaneIDText} {Vehicle.LaneId}");
            LaneIndexText?.SetText($"{_initialLaneIDText} {Vehicle.LaneId}");
            RouteEdgeListIDsText?.SetText($"{_initialRouteEdgeListIDsText} {Vehicle.EdgeIDs}");
        }

        protected void UpdateMobilityTexts()
        {
            UpdateView();
            SumoSpeedText?.SetText($"{_initialSumoSpeedText} {Vehicle.Speed.ToString("0.0000"),7}");
            SumoAccelerationText?.SetText($"{_initialAccelerationText} {Vehicle.Acceleration.ToString("0.0000"),7}");
            PositionText?.SetText(string.Format("{0} ({1,4}, {2,4})",
             _initialPositionText,
             Vehicle.Position.x.ToString("0"), 
             Vehicle.Position.z.ToString("0")));
        }

    }
}

