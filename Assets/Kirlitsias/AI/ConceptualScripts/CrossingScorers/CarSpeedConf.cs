using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UtilityAI
{
    public class CarSpeedConf : CarContextualScorer
    {
        public float minSpeed = 0;
        public float maxSpeed = 0;
        protected override float RawScore(IContext context)
        {
            base.RawScore(context);
            if (mostImportantCar == null)
                return 0;
            return (ThresholdAndNormalize(mostImportantCar.Velocity.magnitude));
        }

        private float ThresholdAndNormalize(float val)
        {
            if (val > maxSpeed)
                val = maxSpeed;
            return val / maxSpeed;
        }

        private Agent GetAgent(IContext context)
        {
            return ((HumanContext)context).agent;
        }

    }
}