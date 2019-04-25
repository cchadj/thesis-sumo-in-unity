using UnityStandardAssets.Utility;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

#endif

using RiseProject.Tomis.Util.TraciAuxilliary;
using UnityEngine;
using System;

namespace RiseProject.Tomis.VehicleControl
{
    public class DynamicWaypointCircuit : WaypointCircuit
    {

        public class WaypointData
        {
            public readonly Vector3 position;// Position { get; set; }
            public readonly Quaternion rotation;
            public readonly float speed;
            public readonly bool isStopped;
            public  WaypointData nextWaypoint;

            /// <summary>
            /// If nextWaypoint is null that meamns that it's the last one
            /// </summary>
            /// <param name="pos"></param>
            /// <param name="angle"></param>
            /// <param name="speed"></param>
            /// <param name="isStopped"></param>
            /// <param name="nextWaypoint"></param>
            public WaypointData(Vector3 pos, float angle, float speed, bool isStopped, WaypointData nextWaypoint=null)
            {
                position = pos;
                rotation = TraCIAuxiliaryMethods.AngleToQuaternion(angle);
                this.isStopped = isStopped;
                this.speed = speed;
                this.nextWaypoint = nextWaypoint;
            }
        }

        private List<WaypointData> _wayPoints;

        protected override void Awake()
        {
            _wayPoints = new List<WaypointData>();
        }

        public WaypointData[] WaypointsData
        {
            get => _wayPoints.ToArray();
        }

        public override RoutePoint GetRoutePoint(float dist)
        {
            return new RoutePoint(Vector3.zero, Vector3.zero);
        }

        public override Vector3 GetRoutePosition(float dist)
        {
            return Vector3.zero;
        }

        public void AddWaypoint(Vector3 pos, float ang)
        {
            //Transform newTransform = new GameObject().transform;
            //newTransform.name = "haha";
            //newTransform.position = pos;
            //newTransform.rotation = TraCIAuxiliaryMethods.AngleToQuaternion(ang);
            _wayPoints.Add(new WaypointData(pos, ang, 0f, false, null));

        }

        public WaypointData AddWaypoint(Vector3 pos, float ang, float speed, bool isStopped, WaypointData nextWaypoint =null)
        {
            WaypointData newWaypoint = new WaypointData(pos, ang, speed, isStopped, nextWaypoint);
            _wayPoints.Add(newWaypoint);
            return newWaypoint;
        }

        protected override void DrawGizmos(bool selected)
        {
            waypointList.circuit = this;
            if (Waypoints.Length > 1)
            {
                NumPoints = Waypoints.Length;

                Gizmos.color = selected ? Color.yellow : new Color(1, 1, 0, 0.5f);
                Vector3 prev = Waypoints[0].position;

                for (int n = 0; n < Waypoints.Length; n++)
                {
                    Vector3 next = WaypointsData[(n + 1) % WaypointsData.Length].position;
                    Gizmos.DrawLine(prev, next);
                    prev = next;
                }


            }
        }
    }
}