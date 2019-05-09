using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UtilityAI
{
    public class CrossDesicion : Action
    {
        private HumanContext mContext;
        public override void Execute(IContext context)
        {
            mContext=(HumanContext)context;
            mContext.agent.SetStateCrossing();
            //mContext.agent.State = State.CROSSING;
            //mContext.agent.ai = new AI(wantsToCrossBrain);
            //mContext.goal = mContext.agent.Cache.GetNextGoal();
            //mContext.agent.agentType.SetGoalPosition(mContext.goal);
            ///Should also give the new goal to the agentType so it utilizes the pathfinding logic.
            ///Should probably save the previous goal somewhere because the next AI may need to
            ///choose between going back to safety or running forward to next point in order to avoid any
            ///dangers in the environment.
            //Debug.Log("I am crossing mate ;)");
        }

        public override void Stop(IContext context)
        {
            Debug.LogError("WHat is dis in action !");
            throw new System.NotImplementedException();
        }
    }
}