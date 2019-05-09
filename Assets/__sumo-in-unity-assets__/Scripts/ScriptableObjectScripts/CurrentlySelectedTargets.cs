
using RiseProject.Tomis.DataContainers;
using RiseProject.Tomis.SumoInUnity.SumoTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using Tomis.Utils.Unity;
using UnityEngine;

[CreateAssetMenu]
public class CurrentlySelectedTargets : SingletonScriptableObject<CurrentlySelectedTargets>, ISerializationCallbackReceiver
{
   
    private ISelectableTraciVariable _selectedObject;

    [SerializeField] private SumoNetworkData _networkData;
    [SerializeField] private GameEvent _onVehicleSelect;
    [SerializeField] private GameEvent _onObjectSelect;

    public event EventHandler<EventArgs> OnVehicleSelected;
    public event EventHandler<EventArgs> OnVehicleDeselected;
    
    [SerializeField, ReadOnly] private Transform _selectedTransform;

    public Transform SelectedTransform { get => _selectedTransform; set => _selectedTransform = value; }
    public ISelectableTraciVariable SelectedObject { get => _selectedObject; set => _selectedObject = value; }

    /// <summary>
    /// Returns the traci variable 
    /// </summary>
    /// <typeparam name="T"> The excpected returned Traci variable </typeparam>
    /// <returns> The selected traci variable or null if type is not as what expected </returns>
    public T GetSelectedTraciVariable<T>() where T: TraCIVariable
    {
        /* https://stackoverflow.com/questions/982952/c-sharp-generics-and-type-checking */
        Type typeExpected = typeof(T);
        T traciVariable = ((T)SelectedObject?.GetTraciVariable<T>());
        Type typeGot = traciVariable?.GetType();
       
        return typeExpected == typeGot ? traciVariable : default(T);
    }

    /// <summary>
    /// Returns true if a target is selected. Use Unselect to unselect target.
    /// </summary>
    /// <returns> Returns true if a target is selected. False otherwise. </returns>
    public bool IsATargetAlreadySelected() { return SelectedObject != null && SelectedTransform != null; }

    public void OnAfterDeserialize()
    {
        Unselect();
    }

    public void OnBeforeSerialize()
    {
        
    }

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
        SelectedTransform = selectedTransform;
        SelectedObject = selectableTraciVariable;

        // THEN raise even because other systems use this CurrenltySelectedTargets object.
        _onObjectSelect.Raise();
        Vehicle v = selectableTraciVariable.GetTraciVariable<Vehicle>();
        if (v)
        {
            _onVehicleSelect.Raise();
            OnVehicleSelected?.Invoke(this, null);
        }

    }

    public void Unselect()
    {
        SelectedObject = null;
        SelectedTransform = null;
        OnVehicleDeselected?.Invoke(this, null);
    }
}
