using Boo.Lang;
using RiseProject.Tomis.SumoInUnity.SumoTypes;
using UnityEngine;

public class SimulationState : ScriptableObject
{
    [ReadOnly] public ContextSubscriptionState subscriptionState;

    [ReadOnly] public TraCIVariable currentContextSubcribedTraCIVariable;
    [ReadOnly] public string currentContextSubscribedObjectID;



}
