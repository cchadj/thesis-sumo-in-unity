using Boo.Lang;
using RiseProject.Tomis.SumoInUnity.SumoTypes;
using UnityEngine;

public class SimulationState : ScriptableObject
{
    [ReadOnly] public string currentContextSubscribedLaneID;
    [ReadOnly] public Lane currentContextSubscribedLane;   
    
    
}
