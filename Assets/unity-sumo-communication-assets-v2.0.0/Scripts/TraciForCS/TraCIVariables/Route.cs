
using System.Collections.Generic;
using System.Text;
using System;
using CodingConnected.TraCI.NET;
using UnityEngine;

namespace RiseProject.Tomis.SumoInUnity.SumoTypes
{
    [Serializable]
    public class Route  : TraCIVariable
    {
        public static byte ContextDomain { get; } = TraCIConstants.CMD_GET_ROUTE_VARIABLE;

        [SerializeField]
        private List<string> _edgeIDs;
        /// <summary> The edges the vehicle shall drive along, given as their ids, separated using spaces </summary>
        public List<string> EdgeIDs
        {
            get
            {
                return _edgeIDs;
            }

            set
            {
                _edgeIDs = value;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[Route Details --> ID: " + ID + " ]");
            return sb.ToString();
        }
    }
}