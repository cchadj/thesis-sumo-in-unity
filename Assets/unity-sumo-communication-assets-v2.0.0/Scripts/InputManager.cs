using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private CurrentlySelectedTargets selectedTargets;
    [SerializeField] private GameEvent _pauseGameEvent;
    [SerializeField] private GameEvent _followRandomVehicleGameEvent;
    [SerializeField] private GameEvent _deselectObjectGameEvent;
    [SerializeField] private GameEvent _menuOpenGameEvent;
    [SerializeField] private GameEvent _menuCloseGameEvent;
    [SerializeField] private GameEvent _searchModeEntered;
    [SerializeField] private GameEvent _searchModeExited;
    [SerializeField] private GameEvent _selectModeEntered;
    [SerializeField] private GameEvent _selectModeExited;

    private bool _isMenuOpen = false;
    private bool _isSearchMenuOpen = false;

    public GameEvent PauseGameEvent { get => _pauseGameEvent; set => _pauseGameEvent = value; }
    public GameEvent FollowRandomVehicleGameEvent { get => _followRandomVehicleGameEvent; set => _followRandomVehicleGameEvent = value; }
    public GameEvent DeselectObjectGameEvent { get => _deselectObjectGameEvent; set => _deselectObjectGameEvent = value; }
    public GameEvent MenuOpenGameEvent { get => _menuOpenGameEvent; set => _menuOpenGameEvent = value; }
    public GameEvent MenuCloseGameEvent { get => _menuCloseGameEvent; set => _menuCloseGameEvent = value; }
    public GameEvent SearchModeEntered { get => _searchModeEntered; set => _searchModeEntered = value; }
    public GameEvent SearchModeExited { get => _searchModeExited; set => _searchModeExited = value; }
    public GameEvent SelectModeEntered { get => _selectModeEntered; set => _selectModeEntered = value; }
    public GameEvent SelectModeExited { get => _selectModeExited; set => _selectModeExited = value; }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PauseGameEvent?.Raise();
        }
        else if (Input.GetKeyDown(KeyCode.R)) /* Follow random vehicle */
        {
            selectedTargets.SelectRandomVehicle();
            FollowRandomVehicleGameEvent?.Raise();
        }
        else if (Input.GetKeyDown(KeyCode.Space)) /* Get Into Fly mode */
        {
            if(selectedTargets.IsATargetAlreadySelected())
            {
                DeselectObjectGameEvent?.Raise();
                selectedTargets.Unselect();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isSearchMenuOpen)
            {
                SearchModeExited.Raise();
                _isSearchMenuOpen = false;
            }
            else if (_isMenuOpen)
            {
                MenuCloseGameEvent.Raise();
                _isMenuOpen = false;
            }
            else if(selectedTargets.IsATargetAlreadySelected())
            {
                DeselectObjectGameEvent?.Raise();
                selectedTargets.Unselect();
            }
            else
            {
                MenuOpenGameEvent.Raise();
                _isMenuOpen = true;
            }
        }
        else if(Input.GetKeyDown(KeyCode.V))
        {   
            if(!_isSearchMenuOpen)
            {
                SearchModeEntered.Raise();
                SelectModeEntered.Raise();
                _isSearchMenuOpen = true;
            }
        }
    }
}
