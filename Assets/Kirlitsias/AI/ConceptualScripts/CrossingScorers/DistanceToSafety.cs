using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UtilityAI
{
    public class DistanceToSafety : ContextualScorer
    {
        private float min = 0.5f;
        private float max = 3f;
        private float timeToSafety;
        protected override float RawScore(IContext context)
        {
            timeToSafety=((HumanContext)context).agent.TimeToSafetyComfort();
            timeToSafety = Mathf.Clamp(timeToSafety, min, max);
            timeToSafety = (timeToSafety + min) / (min + max);
            //Debug.Log("Time to safety:" + timeToSafety);
            //return timeToSafety;
            return 0.5f;
            //((HumanContext)context).agent
        }
    }
}