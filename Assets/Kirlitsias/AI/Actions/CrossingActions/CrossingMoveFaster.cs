using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UtilityAI
{
    /// <summary>
    /// An action that make the agent move faster towards safety.
    /// </summary>
    public class CrossingMoveFaster : Action
    {
        //private Vector3 totalMovementOnSidewak = Vector3.zero;
        private Vector3 originalSidewalkGoal = Vector3.zero;
        public override void Execute(IContext context)
        {
            //((HumanContext)context).agent.agentType.SetAgentMaximumSpeed();
            Debug.Log("Trexteeee");
        }
        public override void Stop(IContext context)
        {
            //((HumanContext)context).agent.agentType.ResetAgentSpeedToNormal();
        }
    }
}