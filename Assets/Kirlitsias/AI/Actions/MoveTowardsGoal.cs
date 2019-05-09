using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UtilityAI
{
    public class MoveTowardsGoal : Action
    {

        public override void Execute(IContext context)
        {
            var c = (HumanContext)context;
            c.agent.transform.position += (c.goal - c.agent.transform.position).normalized*Time.fixedDeltaTime*3;
        }

        public override void Stop(IContext context)
        {

        }
    }
}