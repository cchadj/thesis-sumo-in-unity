using UnityEngine;
using System.Text;
using Google.Protobuf;
using System;
using CodingConnected.TraCI.NET;
using UnityEngine.Serialization;

namespace RiseProject.Tomis.SumoInUnity.SumoTypes
{
    [Serializable]
    public class Lane : TraCIVariableShaped
    {
        [SerializeField] private uint index;
        [SerializeField] private string edgeId;
        [SerializeField] private float speed;
        [SerializeField] private float length;
        [SerializeField] private Edge edge;
        [SerializeField] private string fromNodeId;
        [SerializeField] private string toNodeId;
        public Vector3 centerOfMass;

        /// <summary> A running number, starting with zero at the right-most lane </summary>
        public uint Index { get => index; set => index = value; }
        /// <summary> The parent edge of this lane (Optional) </summary>
        public Edge Edge { get => edge; set => edge = value; }
        /// <summary> The ID of the parent edge of this lane </summary>
        public string EdgeId { get => edgeId; set => edgeId = value; }
        /// <summary> The maximum speed allowed on this lane [m/s] </summary>
        public float Speed { get => speed; set => speed = value; }
        /// <summary> The length of this lane [m] </summary>
        public float Length { get => length; set => length = value; }
        /// <summary> optional width </summary>
        public float Width { get; set; }
        /// <summary> WROOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOONG NO FROM AND TO NODES FOR LANES The id of the node this lane starts from </summary>
        public string FromNodeId { get => fromNodeId; set => fromNodeId = value; }
        /// <summary> WROOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOONG NO FROM AND TO NODES FOR LANES The id of the node this lane ends to </summary>
        public string ToNodeId { get => toNodeId; set => toNodeId = value; }
        
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