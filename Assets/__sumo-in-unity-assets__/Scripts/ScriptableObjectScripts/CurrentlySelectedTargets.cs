using RiseProject.Tomis.DataContainers;
using RiseProject.Tomis.SumoInUnity.SumoTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CodingConnected.TraCI.NET.Types;
using RiseProject.Tomis.SumoInUnity;
using RiseProject.Tomis.SumoInUnity.MVC;
using Tomis.Utils.Unity;
using UnityEngine;
using Zenject;
using Random = System.Random;

public class CurrentlySelectedTargets : ScriptableObject
{
    // Dependencies
    private SumoNetworkData _networkData;
    private InputManager _inputManager;
    private SumoClient _sumoClient;
    private SumoCommands _sumoCommands;
    private VehicleSimulator _vehicleSimulator;
    private SumoToUnityGameObjectMap _sumoToUnityGameObjectMap;
    
    // Events
    public event EventHandler<SelectedVehicleEventArgs> VehicleSelected;
    public event EventHandler<EventArgs> VehicleDeselected;
    
    //
    private SelectedVehicleEventArgs _currentVehicleEventArgs;

    //static 
    private static readonly Random Random = new Random();
    
    [Header("Debug")]
    [SerializeField, ReadOnly] private Transform selectedTransform;
    [SerializeField, ReadOnly] private ISelectableTraciVariable selectedTraCiVariable;

    /// <summary>
    /// Returns the traci variable 
    /// </summary>
    /// <typeparam name="T"> The excpected returned Traci variable </typeparam>
    /// <returns> The selected traci variable or null if type is not as what expected </returns>
    public T GetSelectedTraciVariable<T>() where T: TraCIVariable
    {
        /* https://stackoverflow.com/questions/982952/c-sharp-generics-and-type-checking */
        Type typeExpected = typeof(T);
        T traciVariable = ((T)selectedTraCiVariable?.GetTraciVariable<T>());
        Type typeGot = traciVariable?.GetType();
       
        return typeExpected == typeGot ? traciVariable : default(T);
    }

    [Inject]
    private void Construct(
        SumoNetworkData networkData,
        InputManager inputManager,
        SumoClient sumoClient,
        SumoCommands sumoCommands,
        VehicleSimulator vehicleSimulator,
        SumoToUnityGameObjectMap sumoToUnityGameObjectMap
            )
    {
        _networkData = networkData;
        _inputManager = inputManager;
        _sumoClient = sumoClient;
        _sumoToUnityGameObjectMap = sumoToUnityGameObjectMap;
        
        _sumoCommands = sumoCommands;
        _inputManager.VehicleDeselectRequested     += (sender, args) => Unselect();
        _inputManager.FollowRandomVehicleRequested += (sender, args) => SelectRandomVehicle();
        _vehicleSimulator = vehicleSimulator;
    }
    
    /// <summary>
    /// Returns true if a target is selected. Use Unselect to unselect target.
    /// </summary>
    /// <returns> Returns true if a target is selected. False otherwise. </returns>
    public bool IsATargetAlreadySelected => selectedTraCiVariable != null && selectedTransform != null;

    public IEnumerable<TValue> RandomValues<TKey, TValue>(IDictionary<TKey, TValue> dict)
    {
        System.Random rand = new System.Random();
        List<TValue> values = Enumerable.ToList(dict.Values);
        int size = dict.Count;
        while (true)
        {
            yield return values[rand.Next(size)];
        }
    }

    private bool SelectRandomVehicle()
    {

        var vehicles = _sumoCommands.VehicleCommands.GetIdList().GetContentAs<List<string>>();
              
        if(vehicles.Count == 0)
            return false;

        var vehicleID = vehicles[Random.Next(vehicles.Count)];

        Vehicle vehicle = null;
        if (_sumoToUnityGameObjectMap.VehicleGameObjects.ContainsKey(vehicleID))
        {
            vehicle = (_sumoToUnityGameObjectMap.VehicleGameObjects[vehicleID].GetComponent<Car>()).TraciVariable;     
        }
        else
        {
            vehicle = ScriptableObject.CreateInstance<Vehicle>();
            vehicle.Instantiate(vehicleID);
            vehicle.SetPositionFromRawPosition2D(_sumoCommands.VehicleCommands.GetPosition(vehicleID).GetContentAs<Position2D>());
            _vehicleSimulator.SetupEnteredVehicle(vehicle);
            if(_sumoClient.SubscriptionType == SubscriptionType.Context)
                _networkData.VehiclesInContextRange.Add(vehicleID, vehicle);
        }
        
        if(vehicle)
        {   
            Select(vehicle.AttachedVehicleTransform, vehicle.AttachedVehicleTransform.GetComponent<ISelectableTraciVariable> ());
            return true;
        }
            return false;
    }
//    
//    private bool SelectRandomVehicle()
//    {
//        
//        var vehicles = 
//            _sumoClient.SubscriptionType == SubscriptionType.Variable
//                ? _networkData.VehiclesLoadedShared
//                : _networkData.VehiclesInContextRange;
//              
//        if(vehicles.Count == 0)
//            return false;
//
//        var v = RandomValues(vehicles).Take<Vehicle>(1).First();
//        if(v)
//        {   
//            Select(v.AttachedVehicleTransform, v.AttachedVehicleTransform.GetComponent<ISelectableTraciVariable> ());
//            return true;
//        }
//        return false;
//    }

    public void Select(Transform selected, ISelectableTraciVariable selectableTraciVariable)
    {
        if (selected == null || selectableTraciVariable == null)
            return;

        selectedTraCiVariable = null;
        selectedTransform = null;

        // FIRST select
        selectedTransform = selected;
        selectedTraCiVariable = selectableTraciVariable;

        var v = selectableTraciVariable.GetTraciVariable<Vehicle>();
        if (v)
        {
            _currentVehicleEventArgs = new SelectedVehicleEventArgs(
                selectableTraciVariable: selectableTraciVariable,
                selectedVehicle: v,
                selectedTransform: selected);
            
            VehicleSelected?.Invoke(this, _currentVehicleEventArgs );
        }

    }

    public void Unselect()
    {
        selectedTraCiVariable = null;
        selectedTransform = null;
     
        VehicleDeselected?.Invoke(this, _currentVehicleEventArgs);
    }
    

}

public class SelectedVehicleEventArgs : EventArgs
{
    public readonly ISelectableTraciVariable SelectableTraciVariable;
    public readonly Vehicle SelectedVehicle;
    public readonly Transform SelectedTransform;
    // is null if not a vehicle
    public readonly VehicleView VehicleView;
   

    public SelectedVehicleEventArgs(
        ISelectableTraciVariable selectableTraciVariable, 
        Vehicle selectedVehicle,
        Transform selectedTransform)
    {
        SelectableTraciVariable = selectableTraciVariable;
        SelectedTransform = selectedTransform;
        SelectedVehicle = selectedVehicle;
        VehicleView = selectedVehicle.AttachedVehicleTransform.GetComponent<VehicleView>();
    }
}