using System;
using TMPro;
using Tomis.Utils.Unity;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class VehicleCanvas : SingletonMonoBehaviour<VehicleCanvas>
{
    [SerializeField] private TextMeshProUGUI vehicleIdText;
    [SerializeField] private TextMeshProUGUI sumoSpeedText;
    [SerializeField] private TextMeshProUGUI sumoAccelerationText;
    [SerializeField] private TextMeshProUGUI actualSpeedText;
    [SerializeField] private TextMeshProUGUI positionText;
    [SerializeField] private TextMeshProUGUI routeIdText;
    [SerializeField] private TextMeshProUGUI edgeIdText;
    [SerializeField] private TextMeshProUGUI laneIdText;
    [SerializeField] private TextMeshProUGUI laneIndexText;
    [SerializeField] private TextMeshProUGUI routeDetailsRouteIdText;
    [SerializeField] private TextMeshProUGUI routeEdgeListIDsText;
    [SerializeField] private TMP_InputField setSpeedInputField;
    [SerializeField] private Button applySpeedButton;

    public TextMeshProUGUI VehicleIdText => vehicleIdText;
    public TextMeshProUGUI SumoSpeedText => sumoSpeedText;
    public TextMeshProUGUI SumoAccelerationText => sumoAccelerationText;
    public TextMeshProUGUI ActualSpeedText => actualSpeedText;
    public TextMeshProUGUI PositionText => positionText; 
    public TextMeshProUGUI RouteIdText => routeIdText;
    public TextMeshProUGUI EdgeIdText => edgeIdText;
    public TextMeshProUGUI LaneIdText => laneIdText;
    
    public TextMeshProUGUI LaneIndexText => laneIndexText;
    public TextMeshProUGUI RouteDetailsRouteIdText => routeDetailsRouteIdText;
    public TextMeshProUGUI RouteEdgeListIDsText => routeEdgeListIDsText;
    public TMP_InputField SetSpeedInputField => setSpeedInputField;
    
    public Button ApplySpeedButton => applySpeedButton;

    private InputManager _inputManager;
    private CurrentlySelectedTargets _selectedTargets;
    
    [Inject]
    private void Construct(
        CurrentlySelectedTargets selectedTargets
        )
    {
        _selectedTargets = selectedTargets;
    }
    private void Start()
    {
        _selectedTargets.OnVehicleSelected += OnVehicleSelected;
        _selectedTargets.OnVehicleDeselected += OnVehicleDeselected;
    }

    private void OnVehicleDeselected(object sender, EventArgs e)
    {
        enabled = false;
    }

    private void OnVehicleSelected(object sender, SelectedTargetEventArgs e)
    {
        enabled = true;
    }
}
