using RiseProject.Tomis.SumoInUnity.SumoTypes;
using System.Collections.Generic;

namespace RiseProject.Tomis.SumoInUnity.MVC
{
    /* Find this script in Input Manager */
    public class VehicleController : SumoTypeController<Vehicle>
    {
        public void SetSpeed(float speed)
        {
            lock (Commands.ClientLock)
            {
                Commands.VehicleCommands.SetSpeed(TraCIVariable.ID, speed);
            }
        }

        /// <summary>
        /// Lets the vehicle stop at the given edge, at the given position and lane.
        /// The vehicle will stop for the given duration.
        /// Re-issuing a stop command with the same lane and position allows changing the duration.
        /// Setting the duration to 0 cancels an existing stop.
        /// </summary>
        /// <param name="edgeID"></param>
        /// <param name="endPosition"></param>
        /// <param name="laneIndex"></param>
        /// <param name="duration"></param>
        public void SetStop(string edgeID, float endPosition, byte laneIndex, float duration)
        {
            lock (Commands.ClientLock)
            {
                Commands.VehicleCommands.SetStop(TraCIVariable.ID, edgeID, endPosition, laneIndex, duration);
            }
        }

        /// <summary>
        /// Resumes from a stop.
        /// </summary>
        public void Resume()
        {
            lock (Commands.ClientLock)
            {
                Commands.VehicleCommands.Resume(TraCIVariable.ID);
            }
        }

        /// <summary>
        /// Changes the speed smoothly to the given value over the given amount of time in seconds (can also be used to increase speed).	
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="duration"></param>
        public void SlowDown(float speed, float duration)
        {
            lock (Commands.ClientLock)
            {
                Commands.VehicleCommands.SlowDown(TraCIVariable.ID, speed, duration);
            }
        }

        /// <summary>
        /// Assigns the named route to the vehicle, assuming 
        /// a) the named route exists, and
        /// b) it starts on the edge the vehicle is currently at(1)(2).	
        /// </summary>
        /// <param name="routeID"></param>
        public void SetRouteByID(string routeID)
        {
            lock (Commands.ClientLock)
            {
                Commands.VehicleCommands.SetRoutID(TraCIVariable.ID, routeID);
            }
        }

        /// <summary>
        /// Assigns the list of edges as the vehicle's new route assuming the first edge given is the one the vehicle is curently at(1)(2).    
        /// </summary>
        /// <param name="routesIDs"></param>
        public void SetRoute(List<string> routesIDs)
        {
            lock (Commands.ClientLock)
            {
                Commands.VehicleCommands.SetRoute(TraCIVariable.ID, routesIDs);
            }
        }

        /// <summary>
        /// Removes the defined vehicle. See below.	
        /// The following reasons may be given:
        /// 0: NOTIFICATION_TELEPORT
        /// 1: NOTIFICATION_PARKING
        /// 2: NOTIFICATION_ARRIVED
        /// 3: NOTIFICATION_VAPORIZED
        /// 4: NOTIFICATION_TELEPORT_ARRIVED
        /// </summary>
        /// <param name="reason"> The reason for thies vehicle to be removed </param>
        public void Remove(byte reason)
        {
            lock (Commands.ClientLock)
            {
                Commands.VehicleCommands.Remove(TraCIVariable.ID, reason);
            }
        }
    }
}
