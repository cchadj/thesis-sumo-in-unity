using UnityEngine;

using RiseProject.Tomis.SumoInUnity.MVC;
using Zenject;

public class EnableVehicleView : MonoBehaviour {

    private CurrentlySelectedTargets _selectedTargets;
    private VehicleView _curView;

    [Inject]
    private void Construct(CurrentlySelectedTargets selectedTargets)
    {
        _selectedTargets = selectedTargets;
    }

    private void Awake()
    {
        _selectedTargets.OnVehicleSelected += SelectedTargetsOnOnVehicleSelected;

        _selectedTargets.OnVehicleDeselected +=
        delegate
        {
            if(_curView)
                _curView.enabled = false;
        };
        enabled = false;
    }

    private void SelectedTargetsOnOnVehicleSelected(object sender, SelectedTargetEventArgs e)
    {
        _curView = e.VehicleView;
        _curView.enabled = true;
    }
}
