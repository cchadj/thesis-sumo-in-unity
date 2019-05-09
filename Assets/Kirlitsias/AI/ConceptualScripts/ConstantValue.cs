using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityAI;

namespace UtilityAI
{
    public class ConstantValue : ContextualScorer
    {
        public float value = 0.5f;
        protected override float RawScore(IContext context)
        {
            return value;
        }
    }
}