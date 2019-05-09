using RiseProject.Tomis.SumoInUnity.SumoTypes;

using UnityEngine;

namespace RiseProject.Tomis.SumoInUnity.MVC
{
    public abstract class SumoTypeView<T, V> : MonoBehaviour where T: SumoTypeController<V> where V: TraCIVariable
    {
        protected VehicleCanvas Canvas { get; set; }
        T _controller;
        public T Controller { get => _controller; protected set => _controller = value; }

        public T GetController()
        {
            return _controller;
        }

        /// <summary>
        /// Intended to initialise controller with
        /// </summary>
        protected abstract void Awake();

        protected abstract void UpdateView();
    }
}

