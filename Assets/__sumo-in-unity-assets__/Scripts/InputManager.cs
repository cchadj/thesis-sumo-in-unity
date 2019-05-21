using System;
using UnityEngine;
using Zenject;

public class InputManager : ITickable
{
    private CurrentlySelectedTargets _selectedTargets;

    private bool _isMenuOpen = false;
    private bool _isSearchMenuOpen = false;

    public event EventHandler<EventArgs> PauseGameEventRequested;
    public event EventHandler<EventArgs> FollowRandomVehicleRequested;
    public event EventHandler<EventArgs> MenuOpenRequested;
    public event EventHandler<EventArgs> MenuCloseRequested;
    public event EventHandler<EventArgs> SearchModeRequested;
    public event EventHandler<EventArgs> SearchModeExitRequested;
    public event EventHandler<EventArgs> SelectModeEnterRequested;
    public event EventHandler<EventArgs> ExitApplicationRequested;
    public event EventHandler<EventArgs> VehicleDeselectRequested;

    [Inject]
    private void Construct(CurrentlySelectedTargets selectedTargets)
    {
        _selectedTargets = selectedTargets;
    }
    
    private const int NumberOfTapsForEscape = 3;
    private  float _buttonCooler = 0.5f ; // Half a second before reset
    private  int _buttonCount = 0;
    
    public void Tick()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PauseGameEventRequested?.Invoke(this, EventArgs.Empty);
        }
        else if (Input.GetKeyDown(KeyCode.R)) // Follow random vehicle
        {
            FollowRandomVehicleRequested?.Invoke(this, EventArgs.Empty);
        }
        else if (Input.GetKeyDown(KeyCode.Space)) // Get Into Fly mode
        {
            if(_selectedTargets.IsATargetAlreadySelected)
            {
                _selectedTargets.Unselect();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if ( _buttonCooler > 0 && _buttonCount == NumberOfTapsForEscape - 1)
            {
                Debug.Log("Triple escape pressed. Exiting application... ");
                ExitApplicationRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _buttonCooler = 0.5f ; 
                _buttonCount += 1 ;
            }
            
            if ( _buttonCooler > 0 )
            {
                _buttonCooler -= 1 * Time.deltaTime ;
 
            }else{
                _buttonCount = 0 ;
            }
            
            
            if (_isSearchMenuOpen)
            {
                SearchModeExitRequested?.Invoke(this, EventArgs.Empty);
                _isSearchMenuOpen = false;
            }
            else if (_isMenuOpen)
            {
                MenuCloseRequested?.Invoke(this, EventArgs.Empty);
                _isMenuOpen = false;
            }
            else if(_selectedTargets.IsATargetAlreadySelected)
            {
                VehicleDeselectRequested?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                MenuOpenRequested?.Invoke(this, EventArgs.Empty);
                _isMenuOpen = true;
            }
        }
        else if(Input.GetKeyDown(KeyCode.V))
        {   
            if(!_isSearchMenuOpen)
            {
                SearchModeRequested?.Invoke(this, EventArgs.Empty);
                SelectModeEnterRequested?.Invoke(this, EventArgs.Empty);
                _isSearchMenuOpen = true;
            }
        }
    }
}
