using System;
using System.Collections.Generic;
using CodingConnected.TraCI.NET;
using UnityEngine;

namespace RiseProject.Tomis.SumoInUnity.SumoTypes
{
    [Serializable]
    public class Edge : TraCIVariable
    {
        public static byte ContextDomain { get; } = TraCIConstants.CMD_GET_EDGE_VARIABLE;

        [SerializeField]
        private int? _priority;
        [SerializeField]
        private List<Lane> _lanes;
        [SerializeField]
        private string _fromJunctionID;
        [NonSerialized]
        private Junction _fromJunction;
        [SerializeField]
        private string _toJunctionID;
        /// <summary> The id of the node this edge ends to </summary>
        [NonSerialized]
        private Junction _toJunction;

        /// <summary> Indicates how important the road is (optional) </summary>
        public int? Priority { get => _priority; set => _priority = value; }
        /// <summary> The lanes that belong to this edge </summary>
        public List<Lane> Lanes { get => _lanes; set => _lanes = value; }
        /// <summary> The node this edge starts from </summary>
        public Junction FromJunction
        {
            get => _fromJunction;
            set
            {
                _fromJunction = value;
                _fromJunctionID = value.ID;
            }
        }
        /// <summary> The id of the node this edge starts from. Helper function. </summary>
        public string FromJunctionID
        { get => _fromJunctionID;
            set
            { 
                _fromJunctionID = value;
                if(_fromJunction)
                    _fromJunction.Instantiate(value);
            }
        }
        /// <summary> The node this edge ends to </summary>
        public Junction ToJunction
        { get => _toJunction;
            set
            {
                _toJunction = value;
                _toJunctionID = value.ID;
            }
        }
        /// <summary> The if of node this edge ends to. Helper function. </summary>
        public string ToJunctionID
        { get => _toJunctionID;
            set
            {
                _toJunctionID = value;
                if(_toJunction)
                    _toJunction.Instantiate(value);
            }
        }

    }

}