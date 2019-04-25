using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class LaneDataDisplayTexts : DataDisplayTexts
{
    [SerializeField]
    private Text _edgeID;
    [SerializeField]
    private Text _laneID;
    [SerializeField]
    private Text _speed;

    #region UI Text Fields
    public Text LaneIDText
    {
        get
        {
            return _laneID;
        }
        set
        {
            _laneID = value;
        }
    }
    public Text EdgeIDText
    {
        get
        {
            return _edgeID;
        }
        set
        {
            _edgeID = value;
        }
    }
    public Text SpeedText
    {
        get
        {
            return _speed;
        }
        set
        {
            _speed = value;
        }
    }
    #endregion
}
