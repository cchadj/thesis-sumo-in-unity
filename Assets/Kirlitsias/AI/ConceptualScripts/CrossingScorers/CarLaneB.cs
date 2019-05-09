using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UtilityAI
{
    public class CarLaneB : CarContextualScorer
    {
        public float min;
        public float max;
        private float distance;
        protected override float RawScore(IContext context)
        {
            base.RawScore(context);
            if (Input.GetKey(KeyCode.P))
            {
                Debug.Log("Problem Not");

                return 0;
            }
            if (mostImportantCar == null)
                return 1;
            distance=(mostImportantCar.transform.position- GetAgent(context).transform.position).magnitude;
            return (ThresholdAndNormalize(distance));
        }
        
        private float ThresholdAndNormalize(float val)
        {
            if (val > max)
                val = max;
            return val / max;
        }

        private Agent GetAgent(IContext context)
        {
            return ((HumanContext)context).agent;
        }
    }

    public abstract class CarContextualScorer : ContextualScorer
    {
        protected MovingEntity mostImportantCar;
        protected void InitializeCar(IContext context)
        {
            //if (mostImportantCar != null)
            //    return;
            mostImportantCar = ((HumanContext)context).details.GetMostImportantMovingEntity();     
        }
        protected void UpdateCar(IContext context)
        {
            mostImportantCar = ((HumanContext)context).details.GetMostImportantMovingEntity();
        }

        protected override float RawScore(IContext context)
        {
            InitializeCar(context);
            return 0;
        }

        protected float DistanceToCar(IContext context)
        {
            return (mostImportantCar.transform.position - ((HumanContext)context).agent.transform.position).magnitude;
        }
        protected Vector3 DistanceVector(IContext context)
        {
            return (mostImportantCar.transform.position - ((HumanContext)context).agent.transform.position).normalized;
        }
        protected float CarSpeed(IContext context)
        {
            return mostImportantCar.Velocity.magnitude;
        }
        protected Vector3 CarSpeedVector(IContext context)
        {
            return mostImportantCar.Velocity.normalized;
        }
        protected bool DoICare(IContext context)
        {
            if (Vector3.Dot(CarSpeedVector(context), DistanceVector(context)) > 0)
            {
                return true;
            }
            return false;
        }
    }


}