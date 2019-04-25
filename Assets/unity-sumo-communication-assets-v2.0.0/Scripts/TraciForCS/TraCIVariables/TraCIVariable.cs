﻿using System;
using RiseProject.Tomis.CustomTypes;
using UnityEngine;

namespace RiseProject.Tomis.SumoInUnity.SumoTypes
{
    [Serializable]
    public abstract class TraCIVariable : ScriptableObject
    {
        [SerializeField] private string _ID;
        public string ID
        {
            get
            {
                if (!_isInstantiated)
                    throw new InvalidOperationException(" TraCIVariable was not instantiated.\n " +
                                                        "Make sure to use Instantiate(ID) when creating this object.");
                return _ID;
            }
            private set => _ID = value;
        }

        public bool IsInstantiated { get => _isInstantiated; set => _isInstantiated = value;}
        private bool _isInstantiated = false;

        private void Instantiate()
        {
            _isInstantiated = true;
        }

        /// <summary>
        /// This method should always be called or Exception is thrown.
        /// Instantiate overloads should be implemented.
        /// </summary>
        /// <param name="id"> The id of this TraCIVariable </param>
        public virtual void Instantiate(string id)
        {
            Instantiate();
            ID = id;            
        }

        /// <summary>
        /// Disables the TraCIVariable
        /// </summary>
        public void Disable()
        {
            _isInstantiated = false;
        }
    }
}