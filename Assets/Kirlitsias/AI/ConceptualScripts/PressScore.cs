using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UtilityAI
{
    public class PressScore : ContextualScorer
    {
        protected override float RawScore(IContext context)
        {
            Debug.Log("Problem");
            if (Input.GetKey(KeyCode.P)){
                Debug.Log("Problem Not");

                return 1;
            }
            return 0;
        }
    }
}