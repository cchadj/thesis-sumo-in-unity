using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UtilityAI;
[System.Serializable]
public class AgentTypeNavmesh : IAgentType
{
    public NavMeshAgent nav;
    private Agent agent;
    private HumanContext context;
    //private Transform goalTransform;
    //private Vector3 goalPosition;
    
    public AgentTypeNavmesh(Agent agent,IContext context)
    {

        nav=agent.transform.GetComponent<NavMeshAgent>();
        this.context = (HumanContext)context;
        this.agent = agent;
    }
    private float threshold;
    public bool CheckDistanceToGoal()
    {
        if (agent.State == State.TOWARDS_CROSSING)
            threshold = 2;
        else
            threshold = 1f;
        context.goal.y = agent.transform.position.y;
        if ((agent.transform.position - context.goal).magnitude < threshold)
        {
            return true;
        }
        return false;
    }

    public void MoveTowardsGoal()
    {
        ///It is not really needed because it is handled from the navmesh agent script.
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="goal"></param>
    public void SetGoalPosition(Vector3 goal)
    {
        //if(!nav.hasPath)
            nav.SetDestination(context.goal);
    }

    public void SetGoalTransform(TrackedReference goal)
    {
        throw new System.NotImplementedException();
    }

    public void SetAgentMaximumSpeed()
    {
        nav.speed = agent.MaximumSpeed;
    }
    public void ResetAgentSpeedToNormal()
    {
        nav.speed = agent.ComfortSpeed;
    }
}
