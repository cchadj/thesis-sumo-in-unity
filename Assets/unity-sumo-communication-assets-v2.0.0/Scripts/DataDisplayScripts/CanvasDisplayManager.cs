using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasDisplayManager : MonoBehaviour
{
    [SerializeField] private Canvas _menuCanvas;
    [SerializeField] private Canvas _vehicleCanvas;
    [SerializeField] private Canvas _laneCanvas;
    [SerializeField] private Canvas _searchCanvas;

    private List<Canvas> _canvases = new List<Canvas>();

    [SerializeField] private bool _lockMouse;
    private CursorLockMode _wantedCursorLockMode;

    private Canvas VehicleCanvas { get => _vehicleCanvas; set => _vehicleCanvas = value; }
    private Canvas MenuCanvas { get => _menuCanvas; set => _menuCanvas = value; }
    public Canvas LaneCanvas { get => _laneCanvas; set => _laneCanvas = value; }
    public Canvas SearchCanvas { get => _searchCanvas; set => _searchCanvas = value; }

    private SearchElement _searchCanvasSearchElement;
    // Apply requested cursor state
    private static void SetCursorState(CursorLockMode lockMode)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.lockState = lockMode;
        Cursor.visible = (CursorLockMode.Locked != lockMode);
    }

    public void ShowMenuCanvas()
    {
        HideAllCanvases();
        MenuCanvas.enabled = true;
        SetCursorState(CursorLockMode.None);
    }

    public void ShowVehicleCanvas()
    {
        HideAllCanvases();
        _vehicleCanvas.enabled = true;
    }

    public void ShowLaneCanvas()
    {
        HideAllCanvases();
        _laneCanvas.enabled = true;
        SetCursorState(CursorLockMode.None);
    }

    public void ShowSearchCanvas()
    {
        HideAllCanvases();
        SearchCanvas.enabled = true; 
        _searchCanvasSearchElement.GiveSearchFieldFocus(true);
        SetCursorState(CursorLockMode.None);
    }


    public void HideAllCanvases()
    {
        foreach (Canvas c in _canvases)
        c.enabled = false;
        SetCursorState(_lockMouse ? CursorLockMode.Locked : CursorLockMode.None);
    }
 
    private void Start()
    {
        VehicleCanvas.enabled = false;
        MenuCanvas.enabled = false;
        LaneCanvas.enabled = false;
        SearchCanvas.enabled = false;

        _searchCanvasSearchElement = SearchCanvas.GetComponent<SearchElement> ();
        _canvases.Add(VehicleCanvas);
        _canvases.Add(MenuCanvas);
        _canvases.Add(LaneCanvas);
        _canvases.Add(SearchCanvas);

        SetCursorState(_lockMouse? CursorLockMode.Locked : CursorLockMode.None);
    }
}
