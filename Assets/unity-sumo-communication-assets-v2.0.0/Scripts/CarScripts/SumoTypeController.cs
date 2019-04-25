using RiseProject.Tomis.DataHolders;
using RiseProject.Tomis.SumoInUnity.SumoTypes;
using UnityEngine;
using UnityEngine.Serialization;

namespace RiseProject.Tomis.SumoInUnity.MVC
{
    public class SumoTypeController<T>: MonoBehaviour where T : TraCIVariable
    {
        [SerializeField, ReadOnly] private T traCIVariable;

        private void Awake()
        {
            if(_commands == null)
                Commands = SumoCommands.Instance;
        }

        private SumoCommands _commands;
        protected SumoCommands Commands {
            get
            {
                if (_commands == null)
                    _commands = SumoCommands.Instance;
                
                return _commands;
            }
            private set => _commands = value;
        }

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

