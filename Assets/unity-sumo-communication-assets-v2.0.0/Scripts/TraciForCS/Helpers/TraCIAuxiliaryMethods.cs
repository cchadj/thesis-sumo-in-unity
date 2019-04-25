
using System.Collections.Generic;
using CodingConnected.TraCI.NET.Types;
using UnityEngine;

namespace RiseProject.Tomis.Util.TraciAuxilliary
{ 
    public static class TraCIAuxiliaryMethods
    {
        /* http://webcache.googleusercontent.com/search?q=cache:http://sumo.dlr.de/wiki/TraCI/Vehicle_Signalling */
        /* Vehicle Signal bit positions */
        private static int
            VEH_SIGNAL_BLINKER_RIGHT = 0,
            VEH_SIGNAL_BLINKER_LEFT = 1,
            VEH_SIGNAL_BLINKER_EMERGENCY = 2,
            VEH_SIGNAL_BRAKELIGHT = 3,
            VEH_SIGNAL_FRONTLIGHT =	4;

        #region Static Auxiliary Methods
        private static List<Vector2> GetShapeVectorPoints(List<Position2D> position2Ds)
        {
            List<Vector2> vectorPoints = new List<Vector2>();
            position2Ds.ForEach(point2D => vectorPoints.Add(Raw2DPositionToVector2(point2D)));

            return vectorPoints;
        }

        /// <summary>
        /// Convert the angle to quaternion. (Angle arround y axis).
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Quaternion AngleToQuaternion(float angle)
        {

            return Quaternion.Euler(new Vector3(0f, angle, 0f));
        }

        public static Vector2 Raw2DPositionToVector2(Position2D position2D)
        {
            return new Vector2((float)position2D.X, (float)position2D.Y);
        }
        /// <summary>
        /// Converts Vector3 in XZ plane (0 in Y axis) to Vector2
        /// </summary>
        /// <param name="raw2DPosition"> The raw position as we get it from sumo </param>
        /// <returns></returns>
        public static Vector2 Vector3toVector2(Vector3 v3)
        {
            return new Vector2(v3.x, v3.z);
        }
        /// <summary>
        /// Converts 2d raw Sumo position to Vector3 that is 0 on Y axis.
        /// </summary>
        /// <param name="raw2DPosition"> The raw position as we get it from sumo </param>
        /// <returns></returns>
        public static Vector3 Raw2DPositionToVector3(Position2D raw2DPosition)
        {
            return new Vector3((float)raw2DPosition.X, 0f, (float)raw2DPosition.Y);
        }

        /// <summary>
        /// Converts a Vector3 that is on XZ plane (Y axis is 0) to raw2DSumoPosition.
        /// </summary>
        /// <param name="v">A Vector3 that is 0 on Y axis (and represents the sumo position in the XZ plane)</param>
        /// <returns></returns>
        public static Position2D Vector3ToRaw2DPosition(Vector3 v)
        {
            return new Position2D() { X = v.x, Y = v.z };
        }

        public static List<Vector2> GetVectorListFromPolygon(Polygon polygon)
        {
            List<Vector2> vectorPoints = GetShapeVectorPoints(polygon.Points);

            return vectorPoints;
        }

        #region Signal Checks
        public static bool IsRightBlinkerOn(int signal)
        {
            return IsBitSet(signal, VEH_SIGNAL_BLINKER_RIGHT);
        }
        public static bool IsLeftBlinkerOn(int signal)
        {
            return IsBitSet(signal, VEH_SIGNAL_BLINKER_LEFT);
        }
        public static bool IsBrakeLightOn(int signal)
        {
            return IsBitSet(signal, VEH_SIGNAL_BRAKELIGHT);
        }
        public static bool IsFrongLightOn(int signal)
        {
            return IsBitSet(signal, VEH_SIGNAL_FRONTLIGHT);
        }
        public static bool IsEmergencyLightOn(int signal)
        {
            return IsBitSet(signal, VEH_SIGNAL_BLINKER_EMERGENCY);
        }

        private static bool IsBitSet(int sig, int pos)
        {
            return (sig & (1 << pos)) != 0;
        }
        #endregion Signal Checks
        #endregion Static Methods
    }
}


