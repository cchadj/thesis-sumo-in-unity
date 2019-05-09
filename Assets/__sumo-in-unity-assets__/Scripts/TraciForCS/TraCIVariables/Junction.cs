using System;
using CodingConnected.TraCI.NET;
using UnityEngine;

namespace RiseProject.Tomis.SumoInUnity.SumoTypes
{
    [Serializable]
    public class Junction : TraCIVariableShaped
    {
        public byte ContextDomain { get; } = TraCIConstants.CMD_GET_JUNCTION_VARIABLE;

        public enum JunctionType
        {
            Priority,
            TrafficLight,
            RightBeforeLeft,
            Unregulated,
            TrafficLightUnregulated,
            PriorityStop,
            AlwaysStop,
            RailSignal,
            Zipper,
            RailCrossing,
            TrafficLightRightOnRed,
            DeadEnd,
            InternAl,
        }

        [SerializeField] private Vector2 rawPosition;
        [SerializeField] private JunctionType type;
        [SerializeField, ReadOnly] private Vector3 position;
        [SerializeField] private string[] incLanes;
        [SerializeField] private string[] intLanes;

        public string Type
        {
            set
            {
                type = (JunctionType)Enum.Parse(typeof(JunctionType), value, ignoreCase:true);
            }
        }

        public JunctionType GetJunctionType() { return type; }
        public string GetJunctionTypeAsString() {
            return type.ToString().ToLower();
        }

        public Vector3 Position { get => position;}

        public Vector2 Raw2DPosition
        {
            get => rawPosition;
            set
            {
                rawPosition = value;
                position = new Vector3(rawPosition.x, 0f, rawPosition.y);
            }
        }

        /// <summary> The ids of the lanes that end at the intersection; sorted by direction, clockwise, with direction up = 0  </summary>
        public string[] IncLanes { get => incLanes; set => incLanes = value; }
        /// <summary> The IDs of the lanes within the intersection </summary>
        public string[] IntLanes { get => intLanes; set => intLanes = value; }
    }
}