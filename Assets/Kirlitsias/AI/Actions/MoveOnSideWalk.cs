using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UtilityAI
{
    public class MoveOnSideWalk : Action
    {
        private HumanContext mContext;
        private Vector3 dir;

        //private Vector3 totalMovementOnSidewak = Vector3.zero;
        private Vector3 originalSidewalkGoal = Vector3.zero;
        public override void Execute(IContext context)
        {
            //Debug.Log("SidewalkMate");
            mContext = (HumanContext)context;
            dir= mContext.details.GetSidewalkDirNew().normalized;
            if (MoveCurrentGoal(dir)){
                mContext.agent.agentType.SetGoalPosition(mContext.goal);
            }
        }
        
        /// <summary>
        /// This will move the current goal across the sidewalk if efficient.
        /// </summary>
        private bool MoveCurrentGoal(Vector3 dir)
        {
            float dis1 = (mContext.originalGoal - mContext.goal).magnitude;
            float dis2= (mContext.originalGoal - (mContext.goal + dir)).magnitude;
            float dis3 = (mContext.originalGoal - (mContext.goal - dir)).magnitude;
            if (originalSidewalkGoal == Vector3.zero)//Accepts a small inaccuracy
            {
                originalSidewalkGoal = mContext.goal;
            }
            if (dis3<dis1)
            {
                //mContext.goal = mContext.goal - dir;       
                mContext.goal = mContext.agent.transform.position - dir*2;
                return true;
            }
            if (dis2 < dis1)
            {
                //mContext.goal = mContext.goal + dir;
                mContext.goal = mContext.agent.transform.position + dir*2;
                return true;
            }return false;
        }

        public override void Stop(IContext context)
        {
            Debug.Log("Does nothing");
            Debug.DrawRay(mContext.goal, mContext.goal - originalSidewalkGoal, Color.black);
            mContext.agent.Cache.ModifyPointAcrossTheRoad(mContext.agent.State, mContext.goal-originalSidewalkGoal);
            originalSidewalkGoal = Vector3.zero;
            //throw new System.NotImplementedException();
        }
    }
}