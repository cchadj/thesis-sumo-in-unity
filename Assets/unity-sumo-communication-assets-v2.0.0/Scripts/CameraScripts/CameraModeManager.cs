using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraModeManager : MonoBehaviour
{
    [SerializeField]
    private CurrentlySelectedTargets currentlySelectedTargets;

    private Camera _mainCamera;

    private MouseOrbit _mouseOrbitMode;
    private ExtendedFlycam _flyCameraMode;
    private List<MonoBehaviour> _cameraModes = new List<MonoBehaviour>();

    private MonoBehaviour _lastMode;
    private MonoBehaviour _penultimateMode;

    public MouseOrbit MouseOrbitMode { get => _mouseOrbitMode; set => _mouseOrbitMode = value; }
    public ExtendedFlycam FlyCameraMode { get => _flyCameraMode; set => _flyCameraMode = value; }
    public Camera MainCamera { get => _mainCamera; set => _mainCamera = value; }
    public List<MonoBehaviour> CameraModes { get => _cameraModes; set => _cameraModes = value; } 

    private void Start()
    {
        MainCamera = Camera.main;
        MouseOrbitMode = MainCamera.GetComponent<MouseOrbit>();
        FlyCameraMode = MainCamera.GetComponent<ExtendedFlycam>();
        
        CameraModes.Add(MouseOrbitMode);
        CameraModes.Add(FlyCameraMode);

        FlyCameraMode.enabled = true;
        _lastMode = FlyCameraMode;
        MouseOrbitMode.enabled = false;
    }

    public void DisableAllModes()
    {
        foreach(MonoBehaviour modes in CameraModes )
        {
            modes.enabled = false;
        }
    }

   public void SelectedObjectMode()
    {
        if (currentlySelectedTargets.SelectedTransform == null)
            return;

        DisableAllModes();
        
        MouseOrbitMode.enabled = true;
        // OnEnabled is called on MouseObitMode when is disabled and then enabled
        // so if already enabled I should also update it's target.
        MouseOrbitMode.UpdateTarget();
        _penultimateMode = _lastMode;
        _lastMode = MouseOrbitMode;
    }

    public void FlyMode()
    {
        DisableAllModes();
        FlyCameraMode.enabled = true;
        _penultimateMode = _lastMode;
        _lastMode = FlyCameraMode;
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
