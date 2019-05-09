using System.Collections;
using System.Collections.Generic;
using RiseProject.Kirlitsias;
using UnityEngine;

namespace UtilityAI { 
    public class MoveFreely : Action
    {
        private HumanContext mContext;
        private CrossingPoint currentCrossingPoint;
        public override void Execute(IContext context)
        {
            //Debug.Log("So call me maybe");
            mContext = ((HumanContext)context);
            currentCrossingPoint=mContext.details.crossingPoints[mContext.agent.Cache.
                GetIndexOfCurrentIntermediateGoal(mContext.details.crossingPoints)];
            currentCrossingPoint.FindIntersectionFreely(mContext.agent.transform.position, mContext.originalGoal);
            mContext.goal = currentCrossingPoint.SafetyPosition;
            mContext.agent.agentType.SetGoalPosition(mContext.goal);
            ///SHould add details that will make the agent move freely in the road.
            ///I have to find a way that decides whether is safe to do that for an agent.
            ///For example use profile values of the agent.
            //Debug.Log("Freely");
        }
        public override void Stop(IContext context)
        {
            currentCrossingPoint.FindNewSafetyPosition(mContext.agent.transform.position);
            mContext.goal = currentCrossingPoint.SafetyPosition;
            mContext.agent.agentType.SetGoalPosition(mContext.goal);
            //Debug.LogError("WHat is dis in action !");
            //throw new System.NotImplementedException();
        }
    }
}