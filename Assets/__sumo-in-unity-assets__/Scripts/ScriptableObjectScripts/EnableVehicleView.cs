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
        _selectedTargets.VehicleSelected += SelectedTargetsVehicleSelected;

        _selectedTargets.VehicleDeselected +=
        delegate
        {
            if(_curView)
                _curView.enabled = false;
        };
        enabled = false;
    }

    private void SelectedTargetsVehicleSelected(object sender, SelectedVehicleEventArgs e)
    {
        _curView = e.VehicleView;
        _curView.enabled = true;
    }
}
