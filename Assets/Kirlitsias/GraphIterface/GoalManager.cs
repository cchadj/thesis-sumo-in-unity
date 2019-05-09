using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityAI;
/// <summary>
/// This class is used to add logic about the goals our agent will have.
/// This will start by having some random goals and evolve to goals that are generated
/// from the agents desires.
/// First though we should try to find a good structure that wont need anymore changes.
/// Something that just works. This class is going to provide original goals, not the intermediate ones
/// used for crossings => That means we may need another independent system to do that work properly, and finally
/// remove it from the agents main logic.
/// </summary>
public class GoalManager : MonoBehaviour {

    private List<GameObject> goals;
    private Agent agent;
	// Use this for initialization
	void Start () {
        agent = GetComponent<Agent>();
        goals=new List<GameObject>(GameObject.FindGameObjectsWithTag("goal"));
	}

    private int index = 0;
    /// <summary>
    /// Returns an original goal.
    /// </summary>
    public Transform GetNextGoal()
    {
        //Debug.Log("is AvatarMask here");
        return goals[index++ % goals.Count].transform;
    }
}
public class NavAgentGoal : IGoalNeeds
{
    public UnityEngine.AI.NavMeshAgent nav;
    private Transform goal = null;
    public NavAgentGoal(UnityEngine.AI.NavMeshAgent nav)
    {
        this.nav = nav;
    }

    public bool SetGoal(Vector3 goal)
    {
        bool ret = false;
        nav.path.ClearCorners();
        ret = nav.SetDestination(goal);
        int mult = 1;
        while (!ret)
        {
            Debug.Log("Can't set goal :(");
            Debug.DrawRay(nav.transform.position, (goal - nav.transform.position).normalized * 5, Color.red);
            goal = goal + (goal - nav.transform.position).normalized*0.1f*mult;
            ret = nav.SetDestination(goal);
            mult++;
        }
        UnityEngine.AI.NavMeshPath pat = new UnityEngine.AI.NavMeshPath();
        nav.CalculatePath(goal, pat);
        nav.path = pat;
        return ret;
    }

    public bool PathStillPending()
    {
        return nav.pathPending;
    }

    public float ReachedGoal()
    {
        return nav.remainingDistance;
    }

    public bool SetGoalFromTransform(Transform trans)
    {
        bool ret = false;
        ret = nav.SetDestination(trans.position);
        UnityEngine.AI.NavMeshPath pat = new UnityEngine.AI.NavMeshPath();
        nav.CalculatePath(trans.position, pat);
        nav.path = pat;
    
        return ret;
    }

    public Vector3 CurrentIntermediateGoal(int i)
    {
        if (i > nav.path.corners.Length - 1)
            return Vector3.zero;
        Vector3 ret = nav.path.corners[i];
        ret.y = nav.gameObject.transform.position.y;
        return ret;
    }
    
    public void VisualizeCorners()
    {
        for (int i = 0; i < nav.path.corners.Length-1; i++)
        {
            Debug.DrawRay(nav.path.corners[i], nav.path.corners[i + 1] - nav.path.corners[i], Color.white, 5);
            GameObject obj = new GameObject();
            obj.transform.position = nav.path.corners[i];
            obj.name = "obj:" + i;
        }
    }

    /// <summary>
    /// Number of corners to search on
    /// </summary>
    /// <returns></returns>
    public int NumberOfCorners()
    {
        return nav.path.corners.Length;
    }
    public Vector3[] CopyOfPathCorners()
    {
        return (Vector3[]) nav.path.corners.Clone();
    }
}