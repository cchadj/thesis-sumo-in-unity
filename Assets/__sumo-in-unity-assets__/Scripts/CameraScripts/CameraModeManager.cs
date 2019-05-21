using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CameraModeManager : IInitializable
{
    private Camera _mainCamera;

    private MouseOrbit _mouseOrbitMode;
    private ExtendedFlycam _flyCameraMode;
    private readonly List<MonoBehaviour> _cameraModes = new List<MonoBehaviour>();

    private MonoBehaviour _lastMode;
    private MonoBehaviour _penultimateMode;

    // Dependencies 
    private CurrentlySelectedTargets _selectedTargets;

    [Inject]
    private void Construct(CurrentlySelectedTargets selectedTargets)
    {
        _selectedTargets = selectedTargets;
        
        _selectedTargets.VehicleSelected   += (sender, args) => EnableOrbitMode();
        _selectedTargets.VehicleDeselected += (sender, args) => EnableFlyMode();
        
    }

    public void Initialize()
    {
        _mainCamera = Camera.main;
        _mouseOrbitMode = _mainCamera.GetComponent<MouseOrbit>();
        _flyCameraMode = _mainCamera.GetComponent<ExtendedFlycam>();
        
        _cameraModes.Add(_mouseOrbitMode);
        _cameraModes.Add(_flyCameraMode);

        _flyCameraMode.enabled = true;
        _lastMode = _flyCameraMode;
        _mouseOrbitMode.enabled = false;
    }

    public void DisableAllModes()
    {
        foreach(MonoBehaviour modes in _cameraModes )
        {
            modes.enabled = false;
        }
    }

   public void EnableOrbitMode()
    {
        if (!_selectedTargets.IsATargetAlreadySelected)
            return;

        DisableAllModes();
        
        _mouseOrbitMode.enabled = true;
        // OnEnabled is called on MouseObitMode when is disabled and then enabled
        // so if already enabled I should also update it's target.
        _penultimateMode = _lastMode;
        _lastMode = _mouseOrbitMode;
    }

    public void EnableFlyMode()
    {
        DisableAllModes();
        _flyCameraMode.enabled = true;
        
        _penultimateMode = _lastMode;
        _lastMode = _flyCameraMode;
    }

    public void EnableLastMode()
    {
        DisableAllModes();
        _lastMode.enabled = true;
    }

    public void EnablePenultimateMode()
    {
        DisableAllModes();
        _penultimateMode.enabled = true;
    }
}
