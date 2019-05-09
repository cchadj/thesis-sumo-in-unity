using System;
using UnityEngine;
using RiseProject.Tomis.SumoInUnity.SumoTypes;

namespace RiseProject.Tomis.DataContainers
{

    /// <summary>
    /// A class to attach to unity objects that represent TraciVariables like 
    /// routes, junctions, vehicles e.t.c. It is used to make some attributes accessed 
    /// easily for each unity object.
    /// </summary>
    public abstract class TraciVariableData<T> : MonoBehaviour, ISelectableTraciVariable where T : TraCIVariable
    {
        [SerializeField, ReadOnly] private T traciVariable;
        [SerializeField, ReadOnly] private string _ID;

        public T TraciVariable
        {
            get => traciVariable;
            set
            {
                _ID = value.ID;
                traciVariable = value;
            }
        }
        public string ID { get => _ID; }

        public T1 GetTraciVariable<T1>() where T1 : TraCIVariable
        {
            try
            {
                return (T1)Convert.ChangeType(TraciVariable, typeof(T1));
            }
            catch
            {
                return default(T1);
            }
        }

        public bool SetTraciVariable<T1>(T1 traciVariable) where T1 : TraCIVariable
        {
            try
            {
                TraciVariable = (T)Convert.ChangeType(TraciVariable, typeof(T));
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}