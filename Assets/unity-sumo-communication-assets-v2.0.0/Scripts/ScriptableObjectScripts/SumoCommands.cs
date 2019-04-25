using CodingConnected.TraCI.NET.Commands;
using Tomis.Utils.Unity;
using UnityEngine;

[CreateAssetMenu]
public class SumoCommands : SingletonScriptableObject<SumoCommands>
{
    /// <summary>
    /// If using multithreading then all commands that are executed should use this lock.
    ///
    /// i.e lock(ClientLock) { ControlCommand.SimStep(); }
    /// </summary>
    public readonly object ClientLock = new object();
    
    #region Command Declarations
    private ControlCommands _controlCommands;
    private VehicleCommands _vehicleCommands;
    private SimulationCommands _simulationCommands;
    private RouteCommands _routeCommands;
    private JunctionCommands _junctionCommands;
    private LaneCommands _laneCommands;
    private VehicleTypeCommands _vehicleTypeCommands;

    /// <summary>
    /// http://sumo.dlr.de/wiki/TraCI/Control-related_commands
    /// </summary>
    public ControlCommands ControlCommands
    {
        get
        {
            if (_controlCommands == null)
                throw new ConnectionNotInitializedException("The connection was not initialized yet " +
                    "and the commands have no effect.");
            return _controlCommands;
        }
        internal set
        {
            _controlCommands = value;
        }
    }
    /// <summary>
    /// http://sumo.dlr.de/wiki/TraCI/Vehicle_Value_Retrieval GET
    /// http://sumo.dlr.de/wiki/TraCI/Change_Vehicle_State SET
    /// </summary>
    public VehicleCommands VehicleCommands
    {
        get
        {
            if (_vehicleCommands == null)
                throw new ConnectionNotInitializedException("The connection was not initialized yet " +
                    "and the commands have no effect.");
            return _vehicleCommands;
        }

        set
        {
            _vehicleCommands = value;
        }
    }
    /// <summary>
    /// http://sumo.dlr.de/wiki/TraCI/Simulation_Value_Retrieval GET
    /// http://sumo.dlr.de/wiki/TraCI/Change_Simulation_State SET
    /// </summary>
    public SimulationCommands SimulationCommands
    {
        get
        {
            if (_simulationCommands == null)
                throw new ConnectionNotInitializedException("The connection was not initialized yet " +
                    "and the commands have no effect.");
            return _simulationCommands;
        }

        set
        {
            _simulationCommands = value;
        }
    }
    /// <summary>
    /// http://sumo.dlr.de/wiki/TraCI/Route_Value_Retrieval GET
    /// http://sumo.dlr.de/wiki/TraCI/Change_Route_State SET
    /// </summary>
    public RouteCommands RouteCommands
    {
        get
        {
            if (_routeCommands == null)
                throw new ConnectionNotInitializedException("The connection was not initialized yet " +
                    "and the commands have no effect.");
            return _routeCommands;
        }

        set
        {
            _routeCommands = value;
        }
    }
    /// <summary>
    /// http://sumo.dlr.de/wiki/TraCI/Junction_Value_Retrieval GET
    /// </summary>
    public JunctionCommands JunctionCommands
    {
        get
        {
            if (_junctionCommands == null)
                throw new ConnectionNotInitializedException("The connection was not initialized yet " +
                    "and the commands have no effect.");
            return _junctionCommands;
        }

        set
        {
            _junctionCommands = value;
        }
    }
    /// <summary>
    /// http://sumo.dlr.de/wiki/TraCI/Lane_Value_Retrieval GET
    /// http://sumo.dlr.de/wiki/TraCI/Change_Lane_State SET
    /// </summary>
    public LaneCommands LaneCommands
    {
        get
        {
            if (_laneCommands == null)
                throw new ConnectionNotInitializedException("The connection was not initialized yet " +
                    "and the commands have no effect.");
            return _laneCommands;
        }

        set
        {
            _laneCommands = value;
        }
    }
    /// <summary>
    /// http://sumo.dlr.de/wiki/TraCI/VehicleType_Value_Retrieval GET
    /// http://sumo.dlr.de/wiki/TraCI/Change_VehicleType_State SET
    /// </summary>
    public VehicleTypeCommands VehicleTypeCommands
    {
        get
        {
            if (_vehicleTypeCommands == null)
                throw new ConnectionNotInitializedException("The connection was not initialized yet " +
                    "and the commands have no effect.");
            return _vehicleTypeCommands;
        }

        set
        {
            _vehicleTypeCommands = value;
        }
    }

    #endregion Command Declarations 
}
