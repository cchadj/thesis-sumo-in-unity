using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RiseProject.Kirlitsias
{
    using static SimpleGraphRoadNetwork;


    /// <summary>
    /// Lane extensions are functions that are needed for various cases.
    /// </summary>
    public static class LaneExtensions
    {
        public static MovingEntity GetMostImportantCar(this SimpleGraphRoadNetwork.Lane lane, Vector3 position,
            CrossingPoint crossingPoint)
        {
            if (GetCarsOnLane(lane).Count == 0)
                return null;
            int index = -1;
            float distance = float.MaxValue;
            float dot = 0;
            for (int i = 0; i < GetCarsOnLane(lane).Count; i++)
            {
                dot = Vector3.Dot((position - GetCarsOnLane(lane)[i].transform.position).normalized,
                    GetCarsOnLane(lane)[i].Velocity.normalized);

                if (dot > 0 && distance > (GetCarsOnLane(lane)[i].transform.position - crossingPoint.SafetyPosition)
                    .magnitude)
                {
                    distance = (GetCarsOnLane(lane)[i].transform.position - crossingPoint.SafetyPosition).magnitude;
                    index = i;
                }
            }

            if (index == -1) ///Checks for cars in other lanes as well.
            {
                for (int i = 0; i < GetCarsOnAdjacentLanes(lane).Count; i++)
                {
                    dot = Vector3.Dot((position - GetCarsOnLane(lane)[i].transform.position).normalized,
                        GetCarsOnLane(lane)[i].Velocity.normalized);

                    if (dot > 0 && distance > (GetCarsOnLane(lane)[i].transform.position - crossingPoint.SafetyPosition)
                        .magnitude)
                    {
                        distance = (GetCarsOnLane(lane)[i].transform.position - crossingPoint.SafetyPosition).magnitude;
                        index = i;
                    }
                }
            }

            if (index == -1)
                return null;
            return GetCarsOnLane(lane)[index];
        }

        public static List<KeyValuePair<MovingEntity, float>> GetFiveMostImportantCars(
            this SimpleGraphRoadNetwork.Lane lane, Vector3 position,
            CrossingPoint crossingPoint)
        {
            if (GetCarsOnLane(lane).Count == 0)
                return null;
            float dot = 0;
            Dictionary<MovingEntity, float> distanceOfCars = new Dictionary<MovingEntity, float>();
            for (int i = 0; i < GetCarsOnLane(lane).Count; i++)
            {
                dot = Vector3.Dot((position - GetCarsOnLane(lane)[i].transform.position).normalized,
                    GetCarsOnLane(lane)[i].Velocity.normalized);

                if (dot > 0)
                {
                    distanceOfCars.Add(GetCarsOnLane(lane)[i],
                        (GetCarsOnLane(lane)[i].transform.position - crossingPoint.SafetyPosition).magnitude);
                }
            }

            var myList = distanceOfCars.ToList();
            myList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));
            //myList.
            return null;
        }

        private static List<MovingEntity> GetCarsOnLane(SimpleGraphRoadNetwork.Lane lane)
        {
            return lane.MovingEntitiesOnLane;
        }

        private static List<MovingEntity> GetCarsOnAdjacentLanes(SimpleGraphRoadNetwork.Lane lane)
        {
            Edge currentLaneEdge = lane.edge;
            int laneId = lane.index;
            List<MovingEntity> allCars = new List<MovingEntity>();
            if (currentLaneEdge == null)
            {
                Debug.Log("Cuurent lane edge is null");
                return new List<MovingEntity>();
            }

            if (currentLaneEdge.StartNode == null)
            {
                Debug.Log("Current lane start node");
                return new List<MovingEntity>();
            }

            foreach (var item in currentLaneEdge.StartNode.InEdges)
            {
                if (item.EndNode == currentLaneEdge.StartNode
                ) //Checks whether edge is in the same road.(the opposite lanes)
                    continue;

                if (item.Lanes.Count == 1)
                {
                    allCars.AddRange(item.Lanes[0].MovingEntitiesOnLane);
                }
                else
                    allCars.AddRange(item.Lanes[laneId].MovingEntitiesOnLane);
            }

            return allCars;
        }
    }

    public static class NodeExtensions
    {
        /// <summary>
        /// Returns the number of cars moving towards a node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static int GetNumberOfCarsGoingTowardsANode(this SimpleGraphRoadNetwork.Node node)
        {
            int numberOfIncomingCars = 0;

            foreach (var item in node.InEdges)
            {
                foreach (var lane in item.Lanes)
                {
                    numberOfIncomingCars += lane.MovingEntitiesOnLane.Count;
                }
            }

            return numberOfIncomingCars;
        }

        /// <summary>
        /// Checks all the lanes that are moving towards a node and retrieves an amount of danger.
        /// </summary>
        /// <returns></returns>
        public static List<LaneDangerToNode> DangerTowardsNode(this SimpleGraphRoadNetwork.Node node)
        {
            List<LaneDangerToNode> dangerOfLane = new List<LaneDangerToNode>();
            LaneDangerToNode carsTowardsNode = new LaneDangerToNode();
            float distance = 0;
            float leastdistance = float.MaxValue;
            MovingEntity movingEntity = null;
            foreach (var item in node.InEdges)
            {
                foreach (var lane in item.Lanes)
                {
                    foreach (var car in lane.MovingEntitiesOnLane)
                    {
                        carsTowardsNode = new LaneDangerToNode();
                        distance = (car.transform.position - ((CarLogic) car).Lane[0]).magnitude;
                        for (int i = 1; i < ((CarLogic) car).Lane.Count; i++)
                        {
                            distance += (((CarLogic) car).Lane[i] - ((CarLogic) car).Lane[i - 1]).magnitude;
                        }

                        if (distance < leastdistance)
                        {
                            leastdistance = distance;
                            movingEntity = car;
                        }
                    }

                    carsTowardsNode.distance = leastdistance;
                    carsTowardsNode.car = movingEntity;
                    carsTowardsNode.lane = lane;
                    dangerOfLane.Add(carsTowardsNode);
                    leastdistance = float.MaxValue;
                    movingEntity = null;
                }
            }

            return dangerOfLane;
        }

        //public static float NodeDanger(this SimpleGraphRoadNetwork.Node node)
        //{
        //    int numberOfIncomingCars = node.GetNumberOfCarsGoingTowardsANode();

        //}

        public struct LaneDangerToNode
        {
            public Lane lane;
            public MovingEntity car;
            public float distance;
        }
    }
}
