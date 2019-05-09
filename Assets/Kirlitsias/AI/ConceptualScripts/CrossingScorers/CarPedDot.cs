using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UtilityAI
{
    public class CarPedDot : CarContextualScorer
    {
        private Vector3 carVel;
        private Vector3 relVelo;
        private Vector3 distanceVector;
        protected override float RawScore(IContext context)
        {
            base.RawScore(context);
            if (mostImportantCar == null)
                return 1;
            carVel = mostImportantCar.Velocity;
            relVelo= (((HumanContext)context).agent.AgentPreferredVelocityOnCrossing()-carVel).normalized;
            distanceVector = (((HumanContext)context).agent.transform.position - mostImportantCar.transform.position).normalized;
            //Debug.Log(((Vector3.Dot(relVelo, distanceVector) + 1) / 2f));
            return ((Vector3.Dot(relVelo,distanceVector)+1)/2f);
        }
    }
}