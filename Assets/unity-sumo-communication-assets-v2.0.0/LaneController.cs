using System;
using System.Collections.Generic;
using CodingConnected.TraCI.NET;
using RiseProject.Tomis.DataHolders;
using RiseProject.Tomis.SumoInUnity.SumoTypes;
using UnityEngine;

namespace RiseProject.Tomis.SumoInUnity.MVC
{
    public class LaneController : SumoTypeController<Lane>, IContextSubscriptable
    {
        public void SubscribeContext(List<byte> listOfVariablesToSubscribeTo, double beginTime, double endTime, byte contextDomain, double dist)
        {
            Commands.LaneCommands.SubscribeContext(ID, beginTime, endTime, contextDomain, dist, 
                listOfVariablesToSubscribeTo);
        }

        private readonly List<byte> VariablesToSubscribeTo = new List<byte>
        {
            TraCIConstants.VAR_POSITION,
            TraCIConstants.VAR_ANGLE
        };

        [SerializeField] private float radius;
        [ContextMenu("Subscribe Context")]
        public void SubscribeContext()
        {
            SubscribeContext(VariablesToSubscribeTo, 0, 10000, Vehicle.ContextDomain, radius);
        }
        
//        #if UNITY_EDITOR
//        private void OnDrawGizmos()
//        {
//            GUI.color = Color.green;
//            UnityEditor.Handles.DrawWireDisc(transform.GetChild(0).position ,Vector3.up, radius);
//        }
//        #endif
//
//        private void Update()
//        {
//            enabled = false;
//            var client = SumoClient.Instance;
//            if (client.UseContextSubscription)
//            {
//                if(client.IsConnectionInitialized)
//                    SubscribeContext();
//                else
//                    client.ConnectionInisialized += (s, e) => SubscribeContext();
//            }
//
//            enabled = false;
//        }
    }

    public interface IContextSubscriptable
    {
        void SubscribeContext();
    }
}
