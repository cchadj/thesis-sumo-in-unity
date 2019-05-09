using System.Collections;
using System.Collections.Generic;
using RiseProject.Kirlitsias;
using UnityEngine;


namespace UtilityAI
{
    public class MoveBackwards : Action
    {
        private HumanContext mContext;
        private CrossingPoint currentCrossingPoint;
        public override void Execute(IContext context)
        {
            //Debug.Log("So call me maybe");
            INIT(context);


            //if (mContext.agent.agentType.CheckDistanceToGoal())
            //{
            //    mContext.agent.Cache.AddIntermediateGoalFirst(GetCrossingPoint().SafetyPosition);
            //    mContext.agent.SetStateWantsToCross();
            //}
        }
        public override void Stop(IContext context)
        {
             
        }
        private CrossingPoint GetCrossingPoint()
        {
            return mContext.details.crossingPoints[
                mContext.agent.Cache.GetIndexOfCurrentIntermediateGoal(mContext.details.crossingPoints)];
        }

        private bool INIT(IContext context)
        {
            if (mContext != null)
                return true;
            mContext = ((HumanContext)context);
            //mContext.agent.Cache.AddIntermediateGoalFirst(GetCrossingPoint().previousCrossingPoint.SafetyPosition);
            currentCrossingPoint = GetCrossingPoint();
            return false;
        }
    }
}