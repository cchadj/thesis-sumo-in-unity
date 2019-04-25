using RiseProject.Tomis.SumoInUnity.SumoTypes;
using UnityEngine;
using UnityEngine.UI;

public class LaneDataDisplay : DataDisplay
{
    [HideInInspector]
    private Lane _lane;

    [SerializeField]
    private Text _laneIDText;
    [SerializeField]
    private Text _edgeIDText;
    [SerializeField]
    private Text _speedText;

    private Lane Lane
    {
        get
        {
            return _lane;
        }
        set
        {
            _lane = value;
        }
    }

    Text LaneIDText
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
    Text EdgeIDText
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
    Text SpeedText
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

    protected override void Awake()
    {
        base.Awake();
        enabled = false;

    }

    // Update is called once per frame
    void Update()
    {
        Lane lane = CurrentlySelectedTargets.GetSelectedTraciVariable<Lane>();
        if (lane == null)
            return;
        LaneIDText.text = $"Lane ID: {lane.ID}";
        EdgeIDText.text = $"Text ID: {lane.EdgeID}";
        SpeedText.text = $"Maximum speed: {lane.Speed}";
    }

}
