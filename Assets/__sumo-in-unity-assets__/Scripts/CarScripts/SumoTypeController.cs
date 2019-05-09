using RiseProject.Tomis.DataContainers;
using RiseProject.Tomis.SumoInUnity.SumoTypes;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace RiseProject.Tomis.SumoInUnity.MVC
{
    public class SumoTypeController<T>: MonoBehaviour where T : TraCIVariable
    {
        [SerializeField, ReadOnly] private T traCIVariable;

        [Inject]
        private void Construct(SumoCommands sumoCommands)
        {
            Commands = sumoCommands;
        }

        protected SumoCommands Commands { get; private set; }

        public T TraCIVariable { 
            get => traCIVariable;
            set
            {
                ID = value.ID;
                traCIVariable = value;
            } 
        }
        [field: SerializeField, Rename("id"), ReadOnly] public string ID { get; private set; }


    }
}

