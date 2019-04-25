using RiseProject.Tomis.SumoInUnity.MVC;

using UnityEngine;

public class EnableVehicleView : MonoBehaviour {

    [SerializeField] private CurrentlySelectedTargets selectedTargets;

    private VehicleView _curView;

    private void Awake()
    {
        selectedTargets.OnVehicleSelected += 
            delegate
            {
                _curView = selectedTargets.SelectedTransform.GetComponent<VehicleView>();
                _curView.enabled = true;
            };

        selectedTargets.OnVehicleDeselected +=
        delegate
        {
            if(_curView)
                _curView.enabled = false;
        };
        enabled = false;
    }




}
