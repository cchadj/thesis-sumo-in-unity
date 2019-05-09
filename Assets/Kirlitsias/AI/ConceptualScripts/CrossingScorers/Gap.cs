using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UtilityAI{
    public class Gap : CarContextualScorer
    {
        private float minGapPossible = 0f;
        public float maxGap = 10f;
        private Transform car;
        private float time;
        protected override float RawScore(IContext context)
        {
            base.RawScore(context);
            
            if (mostImportantCar==null || DoICare(context))
            {
                return 1;
            }
            time = DistanceToCar(context) / CarSpeed(context);
            Debug.Log("Time:" + time);

            time = Mathf.Clamp(time, minGapPossible, maxGap);
            Debug.Log("Time:" + time);
            return time / maxGap;           
        }
    }
}