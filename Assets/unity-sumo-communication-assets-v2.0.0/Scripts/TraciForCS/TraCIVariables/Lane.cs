using UnityEngine;
using System.Text;
using Google.Protobuf;
using System;
using CodingConnected.TraCI.NET;

namespace RiseProject.Tomis.SumoInUnity.SumoTypes
{
    [Serializable]
    public class Lane : TraCIVariableShaped
    {
        [SerializeField] private uint _index;
        [SerializeField] private string _edgeID;
        [SerializeField] private float _speed;
        [SerializeField] private float _length;
        [SerializeField] private Edge _edge;
        [SerializeField] private string _fromNodeID;
        [SerializeField] private string _toNodeID;

        /// <summary> A running number, starting with zero at the right-most lane </summary>
        public uint Index { get => _index; set => _index = value; }
        /// <summary> The parent edge of this lane (Optional) </summary>
        public Edge Edge { get => _edge; set => _edge = value; }
        /// <summary> The ID of the parent edge of this lane </summary>
        public string EdgeID { get => _edgeID; set => _edgeID = value; }
        /// <summary> The maximum speed allowed on this lane [m/s] </summary>
        public float Speed { get => _speed; set => _speed = value; }
        /// <summary> The length of this lane [m] </summary>
        public float Length { get => _length; set => _length = value; }
        /// <summary> optional width </summary>
        public float Width { get; set; }
        /// <summary> WROOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOONG NO FROM AND TO NODES FOR LANES The id of the node this lane starts from </summary>
        public string FromNodeID { get => _fromNodeID; set => _fromNodeID = value; }
        /// <summary> WROOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOONG NO FROM AND TO NODES FOR LANES The id of the node this lane ends to </summary>
        public string ToNodeID { get => _toNodeID; set => _toNodeID = value; }
        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("[Lane Details --> ID: " + ID + " Shape Vector points : ");
            ShapeVertexPoints?.ForEach(vPoint =>
           {
               sb.Append(" " + vPoint.ToString("G4") + " ,");
           });
            sb.Append(" Length: " + Length + " Width : " + Width);
            return sb.ToString();
        }

        public static byte ContextDomain => TraCIConstants.CMD_GET_LANE_VARIABLE;
    }
}