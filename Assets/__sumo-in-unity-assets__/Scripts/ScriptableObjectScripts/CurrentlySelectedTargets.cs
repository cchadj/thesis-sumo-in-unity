using RiseProject.Tomis.DataContainers;
using RiseProject.Tomis.SumoInUnity.SumoTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using RiseProject.Tomis.SumoInUnity.MVC;
using Tomis.Utils.Unity;
using UnityEngine;
using Zenject;

public class CurrentlySelectedTargets : SingletonScriptableObject<CurrentlySelectedTargets>
{
    private SumoNetworkData _networkData;
    private SelectedTargetEventArgs lastEventArgs;
    
    public event EventHandler<SelectedTargetEventArgs> OnVehicleSelected;
    public event EventHandler<EventArgs> OnVehicleDeselected;

    [Header("Debug")]
    [SerializeField, ReadOnly] private Transform selectedTransform;
    [SerializeField, ReadOnly] private ISelectableTraciVariable _selectedObject;

    /// <summary>
    /// Returns the traci variable 
    /// </summary>
    /// <typeparam name="T"> The excpected returned Traci variable </typeparam>
    /// <returns> The selected traci variable or null if type is not as what expected </returns>
    public T GetSelectedTraciVariable<T>() where T: TraCIVariable
    {
        /* https://stackoverflow.com/questions/982952/c-sharp-generics-and-type-checking */
        Type typeExpected = typeof(T);
        T traciVariable = ((T)_selectedObject?.GetTraciVariable<T>());
        Type typeGot = traciVariable?.GetType();
       
        return typeExpected == typeGot ? traciVariable : default(T);
    }

    [Inject]
    private void Construct(
        SumoNetworkData networkData
            )
    {
        _networkData = networkData;

    }
    
    /// <summary>
    /// Returns true if a target is selected. Use Unselect to unselect target.
    /// </summary>
    /// <returns> Returns true if a target is selected. False otherwise. </returns>
    public bool IsATargetAlreadySelected => _selectedObject != null && selectedTransform != null;

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

    public bool SelectRandomVehicle()
    {
        if(_networkData.VehiclesLoadedShared.Count == 0)
            return false;

        var v = RandomValues(_networkData.VehiclesLoadedShared).Take<Vehicle>(1).First();
        if(v)
        {   
            Select(v.AttachedVehicleTransform, v.AttachedVehicleTransform.GetComponent<ISelectableTraciVariable> ());
            return true;
        }
            return false;
    }

    public void Select(Transform selectedTransform, ISelectableTraciVariable selectableTraciVariable)
    {
        if (selectedTransform == null || selectableTraciVariable == null)
            return;

        Unselect();

        // FIRST select
        selectedTransform = selectedTransform;
        _selectedObject = selectableTraciVariable;

        var v = selectableTraciVariable.GetTraciVariable<Vehicle>();
        if (v)
        {
            OnVehicleSelected?.Invoke(this, new SelectedTargetEventArgs(selectableTraciVariable, selectedTransform));
        }

    }


    public void Unselect()
    {
        _selectedObject = null;
        selectedTransform = null;
     
        OnVehicleDeselected?.Invoke(this, lastEventArgs);
    }
    

}

public class SelectedTargetEventArgs : EventArgs
{
    public readonly ISelectableTraciVariable SelectableTraciVariable;
    public readonly Transform SelectedTransform;
    // is null if not a vehicle
    public readonly VehicleView VehicleView;
   

    public SelectedTargetEventArgs(
        ISelectableTraciVariable selectableTraciVariable, 
        Transform selectedTransform
        )
    {
        SelectableTraciVariable = selectableTraciVariable;
        SelectedTransform = selectedTransform;
        VehicleView = selectedTransform.parent.GetComponent<VehicleView>();
    }
}