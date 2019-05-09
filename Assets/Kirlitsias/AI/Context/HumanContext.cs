using System.Collections;
using System.Collections.Generic;
using RiseProject.Kirlitsias;
using UnityEngine;
namespace UtilityAI {
    [System.Serializable]
    public class HumanContext : IContext
    {
        public Agent agent;
        // Add things the Brain needs to know here, like a list of known enemies or potential cover positions
        /// <summary>
        /// This variable indicates the current goal of the agent.Can be original or an intermediate one
        /// </summary>
        public Vector3 goal;
        /// <summary>
        /// THis goal is the original one, the high level set one ;p that we don't have currently :) .
        /// </summary>
        public Vector3 originalGoal;
        //public GameObject[] cars;
        public SimpleGraphRoadNetwork graphRoadNetwork;
        //public GameObject[] goals;
        public GameObject[] sidewalks;
        public GameObject[] crossings;
        public IntersectionDetails details;
    }
}