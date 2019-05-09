using System.Collections;
using System.Collections.Generic;
using RiseProject.Kirlitsias;
using UnityEngine;
using UnityEngine.AI;
using static GraphFromXml;
using static RiseProject.Kirlitsias.SimpleGraphRoadNetwork;

public class SimAgent : MonoBehaviour,IEntity
{

    SimpleGraphRoadNetwork graph;
    GoalController controller;
    IGoalNeeds goalSpecifier;
    private NavMeshAgent navMeshAgent;
    public GameObject editableGoal;
    [SerializeField]
    private Profile profile;// = new Profile();

    // Use this for initialization
    IEnumerator Start()
    {
        profile = new Profile();
        //editableGoal = new GameObject();
        yield return new WaitForSeconds(5f);
        navMeshAgent = GetComponent<NavMeshAgent>();
        goalSpecifier = new NavAgentGoal(GetComponent<NavMeshAgent>());
        controller = new GoalController(this,goalSpecifier);
        StartCoroutine(RetrieveGraph());
    }
   
    void Update()
    {
        if(controller!=null)
           editableGoal.transform.position = controller.GetCurrentGoalPosition();
        AgentTimers.AddTime(this);
    }
    
    IEnumerator RetrieveGraph()
    {
        while (graph == null)
        {
            try
            {
                graph = GameObject.Find("Graph").GetComponent<SimpleGraphRoadNetwork>();
            }
            catch (System.Exception ex)
            {

            }
            if (graph == null)
                yield return new WaitForEndOfFrame();
        }
        StartCoroutine(GetGoal());
    }
    
    IEnumerator GetGoal()
    {
        Transform trans = null;
        //yield return new WaitForEndOfFrame();

        while((trans=GetComponent<GoalManager>().GetNextGoal())==null)
            yield return new WaitForEndOfFrame();
        controller.OriginalGoal(trans.position);
        goalSpecifier.SetGoalFromTransform(trans);
        yield return new WaitUntil(() => !goalSpecifier.PathStillPending());
        yield return new WaitForSeconds(3.1f);
        StartCoroutine("QueryRoad");
    }
    private Coroutine WorkC = null;
    private IntersectionDetails details;
    private RoadVisit roadVisit = new RoadVisit();
    IEnumerator QueryRoad()
    {      
        int index = 0;
        new GameObject().transform.position = goalSpecifier.CurrentIntermediateGoal(index);
        Vector3 intersection=Vector3.zero;SimpleGraphRoadNetwork.Edge edge;details=null;
        bool roadQuery = false;
        //New solution
        Transform trans = new GameObject().transform;
        Vector3 lastPositionBetweenCrossing=Vector3.zero;
        Vector3[] corners = ((NavAgentGoal)goalSpecifier).CopyOfPathCorners();
        Debug.Log("Querying road");
        int indexOfLastCorner = -1;

        //for (int i = 0; i < ((NavAgentGoal)goalSpecifier).NumberOfCorners()-1; i++)
        for (int i = 0; i < corners.Length - 1; i++)

        //{
        //    trans.position = goalSpecifier.CurrentIntermediateGoal(i);
        //    Debug.Log("Tries to find intersection");
        //    Debug.DrawRay(trans.position, goalSpecifier.CurrentIntermediateGoal(i + 1) - trans.position, Color.yellow, 5);
        //    if (roadQuery = GraphExtensions.FindIntersectionOfPathWithRoads(graph, trans, out intersection
        //        , out edge, out details, goalSpecifier.CurrentIntermediateGoal(i + 1),roadVisit))
        //    {
        //        roadVisit.Prev = details.edge.mRoad;
        //        indexOfLastCorner = i;
        //        details.intersection = intersection;
        //        details.crossingPoints.Clear();
        //        details.FindInBetweenNodes();
        //        details.mAgent = null;
        //        controller.IntermediateGoals(details.crossingPoints);
        //        lastPositionBetweenCrossing = trans.position;
        //        break;
        //    }
        //    yield return new WaitForSeconds(0.051f);
        //}
        {
            trans.position = corners[i];
            Debug.Log("Tries to find intersection");
            Debug.DrawRay(trans.position, corners[i + 1] - trans.position, Color.yellow, 5);
            if (roadQuery = GraphExtensions.FindIntersectionOfPathWithRoads(graph, trans, out intersection
                , out edge, out details, corners[i + 1], roadVisit))
            {
                roadVisit.Prev = details.edge.mRoad;
                indexOfLastCorner = i;
                details.intersection = intersection;
                details.crossingPoints.Clear();
                details.FindInBetweenNodes();
                details.mAgent = null;
                controller.IntermediateGoals(details.crossingPoints);
                lastPositionBetweenCrossing = trans.position;
                break;
            }
            yield return new WaitForSeconds(0.051f);
        }
        //Debug.Log("Tries to reach goal")

        while ((lastPositionBetweenCrossing - transform.position).magnitude > 1 && ((NavAgentGoal)goalSpecifier).ReachedGoal() > 1f)
            yield return new WaitForSeconds(0.1f);
        //while()
        if (((NavAgentGoal)goalSpecifier).ReachedGoal() < 1f&& details==null)
        {
            StartCoroutine(GetGoal());
            yield break;
        }
        //((NavAgentGoal)goalSpecifier).VisualizeCorners();

        //for (int i = 0; i < corners.Length-1; i++)
        //{
        //    Debug.DrawRay(corners[i], corners[i + 1] - corners[i], Color.green, 2f);
        //}
        List<Vector3> list = new List<Vector3>();
        list.Add(transform.position);
        for (int i = indexOfLastCorner + 1; i < corners.Length; i++)
        {
            list.Add(corners[i]);
        }
        corners = list.ToArray();
        //for (int i = indexOfLastCorner+1; i < corners.Length; i++)
        for (int i = 0; i < corners.Length; i++)
        {
            RaycastHit hit;
            try{ 
            //Debug.DrawRay(corners[i], corners[i + 1] - corners[i], Color.magenta, 2f);
            }catch(System.Exception ex)
            {

            }
            //Debug.DrawRay(transform.position, (corners[i] - transform.position), Color.magenta, 2f);

            if (Physics.Raycast(transform.position, (corners[i] - transform.position), out hit,
                (corners[i] - transform.position).magnitude,1<<LayerMask.NameToLayer("AreaCasting")))
            {
                new GameObject().name = "krokos";
                new GameObject().transform.position = hit.point;
                Debug.Log("Index was:"+ indexOfLastCorner+"|Corner is: " + i);
                controller.SetReferencePoint((corners[i-1]));
                break;
            }
            else
            {
                controller.SetReferencePoint((corners[i]));
            }
        }

        AssignMeAsMemberOfTheRoad();
        WorkC = StartCoroutine(Work());
        //OLD SOLUTION
        //((NavAgentGoal)goalSpecifier).VisualizeCorners();
        //do
        //{
        //    if (((NavAgentGoal)goalSpecifier).ReachedGoal()<1f)
        //    {
        //        StartCoroutine(GetGoal());
        //        yield break;                
        //    }
        //    Debug.Log("Distance is: "+ (transform.position - goalSpecifier.CurrentIntermediateGoal(index)).magnitude);
        //    if((transform.position - goalSpecifier.CurrentIntermediateGoal(index)).magnitude < 1f)
        //    {//Elafiakos
        //        Debug.DrawRay(transform.position, (goalSpecifier.CurrentIntermediateGoal(index) - transform.position), Color.blue, 2f);
        //        Debug.DrawRay(transform.position, (goalSpecifier.CurrentIntermediateGoal(index + 1) - transform.position), Color.blue, 2f);
        //        ((NavAgentGoal)goalSpecifier).VisualizeCorners();
        //        if (roadQuery = GraphExtensions.FindIntersectionOfPathWithRoads(graph, transform, out intersection
        //        , out edge, out details, goalSpecifier.CurrentIntermediateGoal(index + 1)))
        //        {
        //            Debug.Log("Number 2 is calleds");
        //            details.crossingPoints.Clear();
        //            details.FindInBetweenNodes();
        //            int val = 3;
        //            Vector3 intermediate;
        //            while((intermediate = goalSpecifier.CurrentIntermediateGoal(val)) == Vector3.zero)
        //            {
        //                val--;
        //            }
        //            val--;
        //            if ((details.crossingPoints[details.crossingPoints.Count - 1].SafetyPosition - goalSpecifier.CurrentIntermediateGoal(val)).magnitude < 4f)
        //                controller.SetReferencePoint(transform.position + (goalSpecifier.CurrentIntermediateGoal(val+1) - transform.position).normalized * 10);
        //            else
        //                controller.SetReferencePoint(transform.position + (goalSpecifier.CurrentIntermediateGoal(val) - transform.position).normalized * 10);
        //            details.mAgent = null;
        //            controller.IntermediateGoals(details.crossingPoints);                   
        //        }
        //        else
        //        {
        //            Debug.Log("No intersection was found");
        //        }
        //    }
        //    index=1;
        //    yield return new WaitForSeconds(0.1f);
        //}while(roadQuery == false);
        //WorkC =StartCoroutine(Work());
        //yield break;
    }

    IEnumerator Work()
    {
        int val = 0;
        controller.SetProfileOfFunction(profile);
        while (true)
        {
            if ((val=controller.Step())!=0)
            {
                if (val == 2)
                {
                    ///Trigger find new goal.
                    Debug.Log("Looking for new goal");
                    StartCoroutine(GetGoal());
                    yield break;
                }
                if (val == 1)
                {
                    RemoveMeFromRoad();
                    //Debug.Log("End Reached:"+ goalSpecifier.PathStillPending());
                    Debug.Log("Watch me");
                    yield return new WaitUntil(() => !goalSpecifier.PathStillPending());
                    Vector3[] corners = ((NavAgentGoal)goalSpecifier).CopyOfPathCorners();

                    for (int i = 1; i < corners.Length; i++)
                    {
                        Debug.DrawRay(corners[i - 1], corners[i] - corners[i - 1], Color.green, 5);
                    }

                    //yield return new WaitForSeconds(1f);
                    StartCoroutine("QueryRoad");
                    yield break;
                }
            }
            yield return new WaitForEndOfFrame(); 
        }
    }

    /// <summary>
    /// The agent assigns itself to the road datastructure, in order to be considered from cars.
    /// </summary>
    private void AssignMeAsMemberOfTheRoad()
    {
        details.edge.mRoad.AssignAgentOnRoad(controller);
    }
    /// <summary>
    /// The agent removes itself from the datastructure.
    /// </summary>
    private void RemoveMeFromRoad()
    {
        details.edge.mRoad.AssignAgentOnRoad(controller);
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
    
    public Vector3 GetCurrentVelocity()
    {
        return navMeshAgent.velocity;
    }

    public void SetMaxVelocity()
    {
        navMeshAgent.speed = 5;
    }

    public void SetComfortVelocity()
    {
        navMeshAgent.speed = 4;
    }

    Vector3 IEntity.GetGoal()
    {
        throw new System.NotImplementedException();
    }

    public float GetComfortVelocity()
    {
        return profile.ComfortSpeed;
    }

    public float GetMaximumVelocity()
    {
        return profile.MaxSpeed;
    }
}
//public class AgentProfile
//{
//    private float comfortSpeed=2f;
//    private float maxSpeed=4f;

//    public float ComfortSpeed
//    {
//        get
//        {
//            return comfortSpeed;
//        }

//        set
//        {
//            comfortSpeed = value;
//        }
//    }

//    public float MaxSpeed
//    {
//        get
//        {
//            return maxSpeed;
//        }

//        set
//        {
//            maxSpeed = value;
//        }
//    }


//}

public interface IGoalNeeds
{

    bool SetGoal(Vector3 goal);
    bool SetGoalFromTransform(Transform trans);
    bool PathStillPending();
    Vector3 CurrentIntermediateGoal(int i);
}

public interface IAgentOnRoadInfo
{
    CrossingGoal GetIntermediateGoal();
    Vector3 GetAgentPosition();
    Vector3 Velocity();
    int State();
}


public class RoadVisit
{
    Road prev;

    public Road Prev
    {
        get
        {
            return prev;
        }

        set
        {
            prev = value;
        }
    }
    public bool CHeckPrev(Road neo)
    {
        if (prev == neo)
            return true;
        return false;
    }
}