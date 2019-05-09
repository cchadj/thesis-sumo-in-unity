
using RiseProject.Tomis.SumoInUnity.SumoTypes;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class VehicleDataDisplay : DataDisplay
{
    [HideInInspector]
    private Vehicle _vehicle;
    private Vehicle Vehicle
    {
        get
        {
            return _vehicle;
        }
        set
        {
            _vehicle = value;
        }
    }
    private bool _isFirstUpdate = true;
    protected override void Awake()
    {
        base.Awake();
    }

    #region UI Text Fields
    [SerializeField]
    private Text _vehicleIDText;
    public Text VehicleIDText
    {
        get
        {
            return _vehicleIDText;
        }

        set
        {
            _vehicleIDText = value;
        }
    }

    [SerializeField]
    private Text _speedText;
    public Text SpeedText
    {
        get
        {
            return _speedText;
        }

        set
        {
            _speedText = value;
        }
    }

    [SerializeField]
    private Text _positionText;
    public Text PositionText
    {
        get
        {
            return _positionText;
        }

        set
        {
            _positionText = value;
        }
    }

    [SerializeField]
    private Text _routeIDText;
    public Text RouteIDText
    {
        get
        {
            return _routeIDText;
        }
        private set
        {
            _routeIDText = value;
        }
    }

    [SerializeField]
    private Text _edgeIDText;
    public Text EdgeIDText
    {
        get
        {
            return _edgeIDText;
        }
        set
        {
            _edgeIDText = value;
        }
    }

    [SerializeField]
    private Text _laneIDText;
    public Text LaneIDText
    {
        get
        {
            return _laneIDText;
        }
        set
        {
            _laneIDText = value;
        }
    }

    [SerializeField]
    private Text _laneIndexText;
    public Text LaneIndexText
    {
        get
        {
            return _laneIndexText;
        }
        set
        {
            _laneIndexText = value;
        }
    }

    [SerializeField]
    private Text _routeDetailsRouteIDText;
    public Text RouteDetailsRouteIDText
    {
        get
        {
            return _routeDetailsRouteIDText;
        }
        set
        {
            _routeDetailsRouteIDText = value;
        }
    }

    [SerializeField]
    private Text _routeDetailsEdgeListIDsText;
    public Text RouteDetailsEdgeListIDsText
    {
        get
        {
            return _routeDetailsEdgeListIDsText;
        }
        set
        {
            _routeDetailsEdgeListIDsText = value;
        }
    }
    #endregion

    protected override void OnEnable()
    {
        base.OnEnable();
        Vehicle = CurrentlySelectedTargets.GetSelectedTraciVariable<Vehicle>();
        if (Vehicle == null)
        {
            M_DisplayCanvas.enabled = false;
            throw new NoVehicleSelectedException("No vehicle is selected and Vehicle Canvas is being enabled");
        }
    }
    // Update is called once per frame
    protected virtual void Update()
    {
        if(_isFirstUpdate || !M_DisplayCanvas.enabled || Vehicle == null)
        {
            if(_isFirstUpdate)
                _isFirstUpdate = !_isFirstUpdate;
            return;
        }
        
        VehicleIDText.text = $"Vehicle ID: {Vehicle.ID }";
        SpeedText.text = $"Speed: {Vehicle.Speed.ToString()}";
        PositionText.text = $"Position {Vehicle.Position.ToString("G4")}";
        RouteIDText.text = $"Route ID: {Vehicle.RouteId}";
        EdgeIDText.text = $"Edge ID: {Vehicle.EdgeId}";
        LaneIDText.text = $"Lane ID: {Vehicle.LaneId}";
        LaneIndexText.text = $"Lane Index: {Vehicle.LaneIndex}";

        #region route details texts
        RouteDetailsRouteIDText.text = RouteIDText.text;
        RouteDetailsEdgeListIDsText.text = $"Edge IDs: { string.Join(",",Vehicle.EdgeIDs.ToArray())}";
        #endregion
    }
}
