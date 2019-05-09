using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RiseProject.Kirlitsias;
using UnityEngine;

public class CarLogic : MovingEntity {

    private SimpleGraphRoadNetwork.Node currentJunction;
    private SimpleGraphRoadNetwork.Edge currentEdge;
    private SimpleGraphRoadNetwork.Lane currentLane;
    private List<Vector3> lane;//maybe it should be current path.
    private Vector3 lastDirection;
    private bool initialized = false;
    bool stop = false;

    public List<Vector3> Lane
    {
        get
        {
            return lane;
        }
    }

    public delegate void MoveAction();
    public event MoveAction moveAction;
    

    public void InitializeCar(SimpleGraphRoadNetwork.Node spawnedAtNode)
    {
        currentJunction = spawnedAtNode;
        NextGoal();
        initialized = true;
    }

    private void NextGoal()
    {
        if (currentEdge != null)
        {
            currentEdge.MovingEntitiesOnEdge.Remove(this);
            currentLane.MovingEntitiesOnLane.Remove(this);
        }
        ChooseRandomEdgeFromNode();
        GetRandomLane();
        if (lane == null)
            return;
        transform.LookAt(lane[0], Vector3.up);
    }
    private void ChooseRandomEdgeFromNode()
    {
        if (currentJunction.OutEdges.Count == 0)
        {
            stop = true;
            Destroy(transform.gameObject);
            //Debug.Log("No edges outgoing");
            return;
        }
        float dot = -1;float threshold = -0.2f;
        if (currentJunction.OutEdges.Count == 1)
        {
            currentEdge = currentJunction.OutEdges[0];
        }
        else
            do
            {
                threshold -= 0.1f;
                currentEdge = currentJunction.OutEdges[Random.Range(0, currentJunction.OutEdges.Count)];
                dot = Vector3.Dot(lastDirection, currentEdge.GetFirstPartDirection());
                //Debug.Log("Dot val=" + dot);
            } while (dot<threshold);
        if (currentEdge.Lanes.Count== 0)
        {
            Debug.Log("SHIITTT");
        }
        currentEdge.MovingEntitiesOnEdge.Add(this);
        currentJunction = currentEdge.EndNode;        
    }

    private void GetRandomLane()
    {
        int index = 0;
        if (currentEdge == null)
        {
            return;
        }
        if (currentEdge.Lanes.Count == 0)
        {
            Debug.Log("oopppssss");
        }
        lane = new List<Vector3>( 
             currentEdge.Lanes[index=Random.Range(0, currentEdge.Lanes.Count - 1)].EdgeParts);
        IntersectingLanes(currentLane, currentEdge.Lanes[index]);

        currentLane = currentEdge.Lanes[index];
        currentLane.MovingEntitiesOnLane.Add(this);
    }


    private void IntersectingLanes(SimpleGraphRoadNetwork.Lane prev, SimpleGraphRoadNetwork.Lane next)
    {
        if (prev == null)
            return;
        Vector3 intersect = Vector3.zero;int prevLen = prev.EdgeParts.Count;
        bool checkLanes = Intersection.IntersectionOfLines(prev.EdgeParts[prevLen - 1], next.EdgeParts[0],
            prev.EdgeParts[prevLen - 1] - prev.EdgeParts[prevLen - 2], next.EdgeParts[0] - next.EdgeParts[1],out intersect);

        Debug.DrawRay(prev.EdgeParts[prevLen - 1], (prev.EdgeParts[prevLen - 1] - prev.EdgeParts[prevLen - 2]).normalized * 5, Color.blue, 10);
        Debug.DrawRay(next.EdgeParts[0], (next.EdgeParts[0] - next.EdgeParts[1]).normalized * 5, Color.blue, 10);

        if (checkLanes)
        {
            if ((intersect - transform.position).magnitude > 10)
                return;
            lane.Insert(0, intersect);
            GameObject o = new GameObject();
            o.name = "intersetciop";
            o.transform.position = intersect;
        }
    }

    private IEnumerator RayBasedAvoidance()
    {
        RaycastHit hit;
        while(true)
        {
            //Debug.DrawRay(transform.position, Velocity.normalized * 10, Color.white);
            if (Physics.Raycast(transform.position, Velocity.normalized,out hit, 6, 1 << LayerMask.NameToLayer("car")))
            {
                NewMethod(hit);
            }
            else
            {
                moveAction =  NormalMove;
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void NewMethod(RaycastHit hit)
    {
        if (hit.distance < 1.5f)
        {
            Velocity *= 0.22f;

        }
        else if (hit.distance < 2)
            Velocity *= 0.52f;
        else Velocity *= 0.82f;
        moveAction = Decelerate;
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.gameObject.layer != LayerMask.NameToLayer("car"))
    //        return;
    //    Debug.Log("Enter trigger");
    //    moveAction = null;

    //    if (Vector3.Dot((other.transform.position - transform.position).normalized, (lastDirection).normalized) > 0)
    //    {
    //        Velocity = Vector3.zero;

    //        StopAllCoroutines();
    //    }
    //    moveAction = CollideMove;
    //}
    //private void OnTriggerExit(Collider other)
    //{
    //    moveAction = null;
    //    moveAction = NormalMove;
    //    StartCoroutine(RayBasedAvoidance());

    //}

    private void OnApplicationQuit()
    {
        StopAllCoroutines();
    }
    private void Start()
    {
        moveAction += NormalMove;
        //StartCoroutine(RayBasedAvoidance());
    }

    private void CollideMove()
    {
        transform.position += Velocity* Time.deltaTime;
        lastDirection = Velocity.normalized;
    }
    
    private void Decelerate()
    {
        transform.position +=Velocity * Time.deltaTime;
        lastDirection = Velocity.normalized;
    }
    private void NormalMove()
    {
        transform.position += (lane[0] - transform.position).normalized * 5 * Time.deltaTime;
        lastDirection = (lane[0] - transform.position).normalized;
        Velocity = lastDirection.normalized * 5;
    }

    // Update is called once per frame
    void Update () {
        if (stop)
            return;
        moveAction?.Invoke();
        if ((transform.position - lane[0]).magnitude < 0.5f)
        {
            lane.RemoveAt(0);
            if (lane.Count == 0)
            {
                NextGoal();
            }
            transform.LookAt(lane[0], Vector3.up);
        }
	}
}
public static class Extensions1
{
    public static IList<T> Clone<T>(this IList<T> listToClone) where T : System.ICloneable
    {
        return listToClone.Select(item => (T)item.Clone()).ToList();
    }
}