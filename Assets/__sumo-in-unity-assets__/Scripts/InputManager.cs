using System;
using UnityEngine;
using Zenject;

public class InputManager : ITickable
{
    private CurrentlySelectedTargets _selectedTargets;

    private bool _isMenuOpen = false;
    private bool _isSearchMenuOpen = false;

    public event EventHandler<EventArgs> PauseGameEvent;
    public event EventHandler<EventArgs> FollowRandomVehicleGameEvent;
    public event EventHandler<EventArgs> MenuOpenGameEvent;
    public event EventHandler<EventArgs> MenuCloseGameEvent;
    public event EventHandler<EventArgs> SearchModeEntered;
    public event EventHandler<EventArgs> SearchModeExited;
    public event EventHandler<EventArgs> SelectModeEntered;

    public event EventHandler<EventArgs> ExitApplicationRequested; 

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
            PauseGameEvent?.Invoke(this, EventArgs.Empty);
        }
        else if (Input.GetKeyDown(KeyCode.R)) /* Follow random vehicle */
        {
            _selectedTargets.SelectRandomVehicle();
            FollowRandomVehicleGameEvent?.Invoke(this, EventArgs.Empty);
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
                Debug.Log("Tripple escape pressed");
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
                SearchModeExited?.Invoke(this, EventArgs.Empty);
                _isSearchMenuOpen = false;
            }
            else if (_isMenuOpen)
            {
                MenuCloseGameEvent?.Invoke(this, EventArgs.Empty);
                _isMenuOpen = false;
            }
            else if(_selectedTargets.IsATargetAlreadySelected)
            {
                _selectedTargets.Unselect();
            }
            else
            {
                MenuOpenGameEvent?.Invoke(this, EventArgs.Empty);
                _isMenuOpen = true;
            }
        }
        else if(Input.GetKeyDown(KeyCode.V))
        {   
            if(!_isSearchMenuOpen)
            {
                SearchModeEntered?.Invoke(this, EventArgs.Empty);
                SelectModeEntered?.Invoke(this, EventArgs.Empty);
                _isSearchMenuOpen = true;
            }
        }
    }
}
