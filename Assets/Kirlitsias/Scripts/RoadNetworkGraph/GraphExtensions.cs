using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityAI;
using UnityQuery;
using static RiseProject.Kirlitsias.SimpleGraphRoadNetwork;

namespace RiseProject.Kirlitsias
{
    

public static class GraphExtensions {

    private static float[] tmpPositionForKdTree=new float[2];
    private static KdTree.KdTreeNode<float, SimpleGraphRoadNetwork.Node>[] nearestNeighbours;  
    private static void SetTmpPosition(Transform agent)
    {
        tmpPositionForKdTree[0] = agent.position.x;
        tmpPositionForKdTree[1] = agent.position.z;
    }
    private static IntersectionDetails mainDetails = new IntersectionDetails();
    private static IntersectionDetails junctionAreaDetails = new IntersectionDetails();
    private static Agent theAgent;
    public static bool FindIntersectionOfPathWithRoads(this SimpleGraphRoadNetwork graphRoadNetwork,
        Transform agent,out Vector3 intersectionPoint,out SimpleGraphRoadNetwork.Edge outedge
        ,out IntersectionDetails details)
    {
        SetTmpPosition(agent);
        nearestNeighbours= graphRoadNetwork.JTree.GetNearestNeighbours(tmpPositionForKdTree, 10);
        SimpleGraphRoadNetwork.Node node;
        Vector3 tmpIntersection, intersection = Vector3.zero ;bool intersected = false;bool junctionIttersected=false;
        SimpleGraphRoadNetwork.Edge tmpEdge;
        outedge = null;
        //Debug.DrawRay(agent.position, ((HumanContext)agent.GetComponent<Agent>().GetContext()).goal - agent.position, Color.blue,10);
        float distance = float.MaxValue;
        //mainDetails.Initialize();
        mainDetails = new IntersectionDetails();mainDetails.Initialize();junctionAreaDetails = new IntersectionDetails();
        junctionAreaDetails.Initialize();
        details = null;
        theAgent = agent.GetComponent<Agent>();
        foreach(KdTree.KdTreeNode<float,SimpleGraphRoadNetwork.Node> treeNode in nearestNeighbours)
        {
            node = treeNode.Value;
            junctionIttersected=node.CheckIntersectingNode(agent.position, ((HumanContext)agent.GetComponent<Agent>().GetContext()).goal,
                out junctionAreaDetails);
            if (junctionIttersected)
            {
                junctionIttersected = node.CheckIntersectingNode(agent.position, ((HumanContext)agent.GetComponent<Agent>().GetContext()).goal,
                out junctionAreaDetails);
            }
            if (junctionIttersected && distance > (junctionAreaDetails.intersection - agent.position).magnitude)
            {
                details = (IntersectionDetails)junctionAreaDetails.Clone();
                distance = (junctionAreaDetails.intersection - agent.position).magnitude;
                intersection = junctionAreaDetails.intersection;
                outedge = junctionAreaDetails.edge;
            }
            intersected =QueryIntersectionDetailed(node, agent.position, ((HumanContext)agent.GetComponent<Agent>().GetContext()).goal
               , out mainDetails, out tmpIntersection,out tmpEdge);
            if(intersected && distance >(tmpIntersection - agent.position).magnitude)
            {
                details = (IntersectionDetails)mainDetails.Clone();
                distance = (tmpIntersection - agent.position).magnitude;
                intersection = tmpIntersection;
                outedge = tmpEdge;
            }
        }
        if(distance == float.MaxValue)
        {
            intersectionPoint = Vector3.zero;
            return false;
        }
        //Debug.Log("Start node:" + outedge.StartNode + " | End node:" + outedge.EndNode);
        intersectionPoint = intersection;
        return true;
    }

    public static bool FindIntersectionOfPathWithRoads(this SimpleGraphRoadNetwork graphRoadNetwork,
        Transform agent, out Vector3 intersectionPoint, out SimpleGraphRoadNetwork.Edge outedge
        , out IntersectionDetails details,Vector3 goal,RoadVisit prevVisit)
    {
        SetTmpPosition(agent);
        nearestNeighbours = graphRoadNetwork.JTree.GetNearestNeighbours(tmpPositionForKdTree, 7);
        SimpleGraphRoadNetwork.Node node;
        Vector3 tmpIntersection, intersection = Vector3.zero; bool intersected = false; bool junctionIttersected = false;
        SimpleGraphRoadNetwork.Edge tmpEdge;
        outedge = null;
        //Debug.DrawRay(agent.position, ((HumanContext)agent.GetComponent<Agent>().GetContext()).goal - agent.position, Color.blue,10);
        float distance = float.MaxValue;
        //mainDetails.Initialize();
        mainDetails = new IntersectionDetails(); mainDetails.Initialize(); junctionAreaDetails = new IntersectionDetails();
        junctionAreaDetails.Initialize();
        details = null;
        theAgent = agent.GetComponent<Agent>();
        
        foreach (KdTree.KdTreeNode<float, SimpleGraphRoadNetwork.Node> treeNode in nearestNeighbours)
        {   
            node = treeNode.Value;
            //I modified here agents position to be a bit behind the agent.
            //junctionIttersected = node.CheckIntersectingNode(agent.position-(goal-agent.position).normalized,goal,
            //    out junctionAreaDetails);
            junctionIttersected = node.CheckIntersectingNode(agent.position, goal,
                out junctionAreaDetails);
            if (junctionIttersected && distance > (junctionAreaDetails.intersection - agent.position).magnitude&&!prevVisit.CHeckPrev(junctionAreaDetails.edge.mRoad))
            {
                GameObject g = new GameObject();
                g.name = "Intersected";
                Debug.Log("Junction intersected");
                g.transform.position = junctionAreaDetails.intersection;
                details = (IntersectionDetails)junctionAreaDetails.Clone();
                distance = (junctionAreaDetails.intersection - agent.position).magnitude;
                intersection = junctionAreaDetails.intersection;
                outedge = junctionAreaDetails.edge;
                Debug.Log("Junction was intersected");

            }
            intersected = QueryIntersectionDetailed(node, agent.position,goal
               , out mainDetails, out tmpIntersection, out tmpEdge);
            tmpIntersection.y = agent.position.y;
            if (intersected)
            {
                Debug.Log("Road was intersected:" + (tmpIntersection - agent.position).magnitude);
                
            }
            if (intersected && distance > (tmpIntersection - agent.position).magnitude)
            {
                details = (IntersectionDetails)mainDetails.Clone();
                distance = (tmpIntersection - agent.position).magnitude;
                intersection = tmpIntersection;
                outedge = tmpEdge;
            }
        }

        if (distance == float.MaxValue)
        {
            intersectionPoint = Vector3.zero;
            return false;
        }
        //Debug.Log("Start node:" + outedge.StartNode + " | End node:" + outedge.EndNode);
        intersectionPoint = intersection;
        return true;
    }



    private static float disFind;
    public static void FindNearestPoint(this SimpleGraphRoadNetwork.Edge edge,Vector3 position,out Vector3 pointFound)
    {
        disFind = float.MaxValue;
        pointFound = Vector3.zero;
        foreach(Vector3 point in edge.ReturnStartToEndPoints()[0])
        {
            if ((position - point).magnitude < disFind)
            {
                disFind = (position - point).magnitude;
                pointFound = point;
            }
        }
    }

    private static bool IsPointLeftOfLine(Vector3 point,Vector3 a,Vector3 b)
    {
        if (((point.x - a.x) * (b.y - a.y) - (point.y - a.y) * (b.x - a.x))<0){
            return true;
        }
        return false;
    }


    private static float dis;
    private static bool QueryIntersection(SimpleGraphRoadNetwork.Node node,Vector3 agentPosition, Vector3 goal,out Vector3 intersection,out SimpleGraphRoadNetwork.Edge edgeFOund)
    {
        dis = float.MaxValue;
        Vector3 tmpInter = Vector3.zero;
        edgeFOund = null;intersection = Vector3.zero;
        foreach (SimpleGraphRoadNetwork.Edge edge in node.OutEdges)
        {
            Debug.DrawRay(edge.StartNode.Position, edge.EndNode.Position - edge.StartNode.Position,Color.yellow);
            if (Intersection.LineSegmentsIntersection(edge.StartNode.Position, edge.EndNode.Position, agentPosition, goal,out tmpInter))
            {
                if ((tmpInter - agentPosition).magnitude < dis)
                {
                    if (!edge.IsMonodromos && IsPointLeftOfLine(agentPosition, edge.StartNode.Position, edge.EndNode.Position))
                    {
                        dis = (tmpInter - agentPosition).magnitude;
                        intersection = tmpInter;

                        edgeFOund = edge;
                    }
                    if (edge.IsMonodromos)
                    {
                        dis = (tmpInter - agentPosition).magnitude;
                        intersection = tmpInter;

                        edgeFOund = edge;
                    }
                }
            }
        }
        foreach (SimpleGraphRoadNetwork.Edge edge in node.InEdges)
        {
            Debug.DrawRay(edge.StartNode.Position, edge.EndNode.Position - edge.StartNode.Position, Color.cyan);
            if (Intersection.LineSegmentsIntersection(edge.StartNode.Position, edge.EndNode.Position, agentPosition, goal, out tmpInter))
            {
                if ((tmpInter - agentPosition).magnitude < dis)
                {
                    if (!edge.IsMonodromos && IsPointLeftOfLine(agentPosition, edge.StartNode.Position, edge.EndNode.Position))
                    {
                        intersection = tmpInter;
                        dis = (tmpInter - agentPosition).magnitude;
                        edgeFOund = edge;
                    }
                    if (edge.IsMonodromos)
                    {
                        intersection = tmpInter;
                        dis = (tmpInter - agentPosition).magnitude;
                        edgeFOund = edge;
                    }
                }
            }
        }
        if (edgeFOund != null)
        {
            return true;
        }
        return false;
    }



    ///I tried a simplified search, but lets say it sas a retarted idea. QueryIntersections should not be used.
    ///There are a lot of times that it will not work as expected.
    ///THe new implementation will query the first lane of each road(the leftmost one).
    private static IntersectionDetails IntersectionDetailsFQ = new IntersectionDetails();
    private static bool QueryIntersectionDetailed(SimpleGraphRoadNetwork.Node node,
        Vector3 agentPosition, Vector3 goal,out IntersectionDetails details, out Vector3 intersection, 
        out SimpleGraphRoadNetwork.Edge edgeFOund)
    {
        agentPosition.y = 0;
        float dis = float.MaxValue;edgeFOund = null;
        intersection = Vector3.zero;
        IntersectionDetailsFQ.Initialize();
        details = null;
        foreach (SimpleGraphRoadNetwork.Edge edge in node.OutEdges)
        {
            if(DetailedSearchOnEdgeSidewalk(edge,agentPosition,goal,out IntersectionDetailsFQ))
            {
                if(dis> (IntersectionDetailsFQ.intersection - agentPosition).magnitude)
                {
                    details = (IntersectionDetails)IntersectionDetailsFQ.Clone();
                    dis = (IntersectionDetailsFQ.intersection - agentPosition).magnitude;
                    intersection = IntersectionDetailsFQ.intersection;
                    edgeFOund = edge;
                }
            }
        }
        foreach (SimpleGraphRoadNetwork.Edge edge in node.InEdges)
        {
            if(DetailedSearchOnEdgeSidewalk(edge, agentPosition, goal, out IntersectionDetailsFQ))
            {
                if (dis > (IntersectionDetailsFQ.intersection - agentPosition).magnitude)
                {
                    details = (IntersectionDetails)IntersectionDetailsFQ.Clone();
                    dis = (IntersectionDetailsFQ.intersection - agentPosition).magnitude;
                    intersection = IntersectionDetailsFQ.intersection;
                    edgeFOund = edge;
                }
            }
        }
        if (dis == float.MaxValue)
        {
            return false;
        }
        Debug.Log(dis);

        return true;
    }


    private static bool DetailedSearchOnEdge(SimpleGraphRoadNetwork.Edge edge,
        Vector3 agentPosition, Vector3 goal, out Vector3 intersection)
    {
        float dis = float.MaxValue;
        Vector3 tmpInter = Vector3.zero;
        List<Vector3> points = edge.GetEdgePointsForLineIntersection();
        intersection = Vector3.zero;
        for (int i = 0; i < points.Count - 1; i++)
        {
            if (Intersection.LineSegmentsIntersection(points[i], points[i + 1], agentPosition, goal, out tmpInter))
            {
                if ((tmpInter - agentPosition).magnitude < dis)
                {
                    intersection = tmpInter;
                    dis = (tmpInter - agentPosition).magnitude;
                    return true;///We return here because it should not be possible to get two intersections
                                /// in the same lane :P.
                }
            }
        }
        if (dis == float.MaxValue)
            return false;

        return true;
    }
    private static bool DetailedSearchOnEdgeSidewalk(SimpleGraphRoadNetwork.Edge edge,
        Vector3 agentPosition, Vector3 goal, out IntersectionDetails details)
    {
        float dis = float.MaxValue;
        Vector3 tmpInter = Vector3.zero;
        int numberOfSidewalks = edge.GetNumberOFSidewalks();
        List<Vector3> points = edge.GetEdgePointsForLineIntersection();
        details = new IntersectionDetails();
        goal.y = 0;
        for (int j = 0; j < numberOfSidewalks; j++)
        {
            points = GetProperSidewalkPoints(edge, j);
            for (int i = 0; i < points.Count - 1; i++)
            {

                //if (!Intersection.CheckIfLeft(agentPosition, points[i + 1], points[i]))///Checks whether the agent is not in the road.
                //{
                //    continue;
                //}
                //Debug.Log("Goal position:" + goal +" and agent position: "+agentPosition);

                Debug.DrawRay(points[i], Vector2.Perpendicular(new Vector2((points[i + 1] - points[i]).x, (points[i + 1] - points[i]).z)).YTOZ().normalized*3,Color.red,2f);
                Debug.DrawRay(points[i], (goal - agentPosition).normalized*3, Color.red, 2f);
                //Debug.DrawRay(points[i], (goal - agentPosition).normalized * 3, Color.red, 2f);
                
                if (Vector3.Dot(Vector2.Perpendicular(new Vector2((points[i + 1] - points[i]).x, (points[i + 1] - points[i]).z)).YTOZ().normalized, (goal - agentPosition).normalized)
                    > 0) continue;

                if (Intersection.LineSegmentsIntersection(points[i], points[i + 1], agentPosition, goal, out tmpInter))
                {
                    if((tmpInter - agentPosition).magnitude<4)
                        Debug.Log((tmpInter - agentPosition).magnitude);
                    if ((tmpInter - agentPosition).magnitude < dis)
                    {
                        details.Initialize();
                        details.intersection = tmpInter;
                        details.sidewalkIndex = j;
                        details.sidewalkPartIndex = i;
                        details.sideWalkDirection = points[i + 1] - points[i];
                        
                        //Debug.Log(details.sideWalkDirection);
                        details.edge = edge;
                        dis = (tmpInter - agentPosition).magnitude;
                        continue;
                    }
                }
            }
        }
        if (dis == float.MaxValue)
            return false;
        return true;
    }
    public static bool CheckIntersectingNode(this SimpleGraphRoadNetwork.Node node,
        Vector3 agentPosition, Vector3 goal, out IntersectionDetails suggestedPoint)
    {
        agentPosition.y = 0;
        Vector3 tmpInter = Vector3.zero;
        IntersectionDetails intersection = new IntersectionDetails();
        float dis = float.PositiveInfinity;
        AngleVector3 best = null;
        for (int i = 0; i < node.pointsAroundNode.Count; i++)
        {
            Debug.DrawRay(node.Position+new Vector3(0,node.pointsAroundNode[i].vector.y,0),
                node.pointsAroundNode[i].vector - node.Position - new Vector3(0, node.pointsAroundNode[i].vector.y, 0), Color.cyan, 5);
            if (Intersection.LineSegmentsIntersection
                (node.Position, node.pointsAroundNode[i].vector, agentPosition, goal, out tmpInter))
                //&& Intersection.CheckIfLeft(agentPosition, node.pointsAroundNode[i + 1].vector, node.pointsAroundNode[i].vector))
            {   
                if (dis > (tmpInter - agentPosition).magnitude)
                {
                    intersection.Initialize();
                    dis = (tmpInter - agentPosition).magnitude;
                    best = (node.pointsAroundNode[i]);
                    intersection.intersection = best.vector+(best.vector-tmpInter).normalized*0f;
                    intersection.sidewalkPartIndex = best.sidewalkIndexPart;
                    intersection.sidewalkIndex = best.sidewalk;
                    intersection.sideWalkDirection =Vector3.zero;
                    intersection.edge = best.edge;
                    Debug.Log("DIs" + dis);
                }
            }
        }

        if (intersection.edge == null)
        {
            suggestedPoint = null;
            return false;
        }
        suggestedPoint =(IntersectionDetails) intersection.Clone();
        return true;
    }
    private static AngleVector3 Nearest(AngleVector3 v1,AngleVector3 v2,Vector3 interPoint,Vector3 nearest)
    {
        Vector3 d1 = interPoint - v1.vector;
        Vector3 d2 = interPoint - v2.vector;
        Vector3 d3 = nearest - interPoint;

        Debug.DrawRay(interPoint, d1, Color.white, 5);
        Debug.DrawRay(interPoint, d2, Color.white, 5);
        Debug.DrawRay(interPoint, d3, Color.white, 5);


        if (Vector3.Dot(d1.normalized, d3.normalized) > Vector3.Dot(d2.normalized, d3.normalized))
            return v1;
        return v2;
        //if ((v1.vector - interPoint).magnitude < (v2.vector - interPoint).magnitude)
        //    return v1;
        //return v2;
    }
    public static List<Vector3> GetProperSidewalkPoints(SimpleGraphRoadNetwork.Edge edge,
        int index)
    {
        if (index == 0)
        {
            return edge.sidewalk;
        }
        return edge.sidewalkMonodromos;
    }
    /// <summary>
    /// Should return across the road point.
    /// Should save lane points.
    /// </summary>
    public static void PerpendicularPoint()
    {

    }
}

public class IntersectionDetails:System.ICloneable
{
    /// <summary>
    /// Shows whether the sidewalk is the main one, or the one due to a one way road.
    /// </summary>
    public int sidewalkIndex=-1;
    /// <summary>
    /// Info about the index in the parts list of the sidewalk. 
    /// </summary>
    public int sidewalkPartIndex = -1;
    /// <summary>
    /// Which edge should we query
    /// </summary>
    public SimpleGraphRoadNetwork.Edge edge;
    public Vector3 sideWalkDirection { get; set; }
    private static Vector2 forPerpendicularCalculation = new Vector2();
    /// <summary>
    /// The exact point where the edge is intersected.
    /// </summary>
    public Vector3 intersection;
    public int currentPoint = 0;
    public Agent mAgent;

    public List<CrossingPoint> crossingPoints = new List<CrossingPoint>();
    public object Clone()
    {
        IntersectionDetails details = new IntersectionDetails();
        details.sidewalkIndex = this.sidewalkIndex;
        details.sidewalkPartIndex = sidewalkPartIndex;
        details.edge = edge;
        details.intersection = intersection;
        details.sideWalkDirection = sideWalkDirection;
        return details;
    }

    public bool HasValues()
    {
        if (edge == null)
            return false;
        return true;
    }
    public void Initialize()
    {
        sidewalkIndex = -1;
        sidewalkPartIndex = -1;
        edge = null;
        intersection = Vector3.zero;
    }

    private Vector2 perpendicular = Vector2.zero;
    public Vector2 GetPerpendicular()
    {
        if (perpendicular == Vector2.zero)
            CalculatePerpendicular();
        return perpendicular;
    }
    public Vector3 GetPoint1()
    {
        //Debug.Log(sideWalkDirection);
        return intersection - sideWalkDirection;
    }
    public Vector3 GetPoint2()
    {
        return intersection + sideWalkDirection;

    }
    private void CalculatePerpendicular()
    {
        if((sidewalkPartIndex + 1)>= GraphExtensions.GetProperSidewalkPoints(edge, sidewalkIndex).Count)
        {
            perpendicular = Vector2.Perpendicular((GraphExtensions.GetProperSidewalkPoints(edge, sidewalkIndex)[sidewalkPartIndex-1] -
            GraphExtensions.GetProperSidewalkPoints(edge, sidewalkIndex)[(sidewalkPartIndex)]).XZ());
            return;
        }
        perpendicular = Vector2.Perpendicular((GraphExtensions.GetProperSidewalkPoints(edge, sidewalkIndex)[sidewalkPartIndex]-
             GraphExtensions.GetProperSidewalkPoints(edge, sidewalkIndex)[(sidewalkPartIndex + 1)]).XZ());
    }

    /// <summary>
    /// ENas apo tous xeiroterous kwdikes ever. Enna thelei na anevasoume nakko t standart ;p
    /// 
    /// 
    /// </summary>
    /// Crossign points should not be null and should be cleared as well
    public void FindInBetweenNodes()
    {   
        currentPoint = 0;        
        List<Vector3> tmp;
        float distanceBetweenPoints = 0;
        Vector3 tmpPoint;
        SimpleGraphRoadNetwork.Edge oppositeEdge;
        CrossingPoint.intersectionSidewalk = GraphExtensions.GetProperSidewalkPoints(edge, sidewalkIndex);
        if (edge.IsMonodromos&& sidewalkIndex==1)
        {
            crossingPoints.Add(new CrossingPoint(intersection, CrossingPoint.CrossingPointType.START,
                edge.Lanes[edge.Lanes.Count-1],null));
        }
        else
            crossingPoints.Add(new CrossingPoint(intersection, CrossingPoint.CrossingPointType.START,edge.Lanes[0],null));
        int tmpSidewalkIndex = sidewalkPartIndex;
        if(edge.IsMonodromos)
        {
            for (int i = 1; i < edge.Lanes.Count; i++)
            {
                if (sidewalkIndex == 1)
                {
                    sidewalkPartIndex = (edge.Lanes[i].EdgeParts.Count - 1) - tmpSidewalkIndex;//DOnt tocuch
                }
                   tmpPoint = (edge.Lanes[i].EdgeParts[sidewalkPartIndex] +//without thinking
                  edge.Lanes[i - 1].EdgeParts[sidewalkPartIndex])/2f;//
                sidewalkPartIndex = tmpSidewalkIndex;
                distanceBetweenPoints = (-GraphExtensions.GetProperSidewalkPoints(edge, sidewalkIndex)[sidewalkPartIndex]
               + tmpPoint).magnitude;
                if (sidewalkIndex == 1)
                {
                    crossingPoints.Add(new CrossingPoint(intersection +
                                        GetPerpendicular().YTOZ().normalized * distanceBetweenPoints, CrossingPoint.CrossingPointType.MIDDLE
                                        , edge.Lanes[edge.Lanes.Count - 1-i], edge.Lanes[edge.Lanes.Count - i]));
                }
                else
                {
                    crossingPoints.Add(new CrossingPoint(intersection +
                                        GetPerpendicular().YTOZ().normalized * distanceBetweenPoints, CrossingPoint.CrossingPointType.MIDDLE
                                        , edge.Lanes[i], edge.Lanes[i-1]));
                }
                //new GameObject().transform.position = intersection + GetPerpendicular().YTOZ().normalized * distanceBetweenPoints;
            }
            ///This lines determine the sidewalk safe point.
            tmp = GraphExtensions.GetProperSidewalkPoints(edge, (sidewalkIndex+1) % 2);
            distanceBetweenPoints = (-GraphExtensions.GetProperSidewalkPoints(edge, sidewalkIndex)[sidewalkPartIndex]
               + tmp[tmp.Count - 1 - sidewalkPartIndex]).magnitude;
            //No need to correct this one because we dont care about the ending lane(there is none, we need this point
            //only for the goal point).
            CrossingPoint.intersectionSidewalk = tmp;
            crossingPoints.Add(new CrossingPoint(intersection +
    GetPerpendicular().YTOZ().normalized * distanceBetweenPoints, CrossingPoint.CrossingPointType.END,
            edge.Lanes[edge.Lanes.Count - 1],null));
            //new GameObject().transform.position =intersection+ GetPerpendicular().YTOZ().normalized * distanceBetweenPoints;

            ///This was another solution that was not working as well as needed.
            //opposite = tmp[tmp.Count - 1 - sidewalkPartIndex] - tmp[tmp.Count - 2 - sidewalkPartIndex];
            //new GameObject().transform.position = intersection +(
            //    (-GraphExtensions.GetProperSidewalkPoints(edge, sidewalkIndex)[sidewalkPartIndex]
            //    + tmp[tmp.Count - 1 - sidewalkPartIndex])+(-GraphExtensions.GetProperSidewalkPoints
            //    (edge, sidewalkIndex)[sidewalkPartIndex+1]
            //    + tmp[tmp.Count - 2 - sidewalkPartIndex]) )/2f;
        }
        else
        {
            for (int i = 1; i < edge.Lanes.Count; i++)
            {
                tmpPoint = (edge.Lanes[i].EdgeParts[sidewalkPartIndex] +
                  edge.Lanes[i - 1].EdgeParts[sidewalkPartIndex]) / 2f;
                distanceBetweenPoints = (-GraphExtensions.GetProperSidewalkPoints(edge, sidewalkIndex)[sidewalkPartIndex]
               + tmpPoint).magnitude;
                crossingPoints.Add(new CrossingPoint(intersection +
    GetPerpendicular().YTOZ().normalized * distanceBetweenPoints, CrossingPoint.CrossingPointType.MIDDLE,edge.Lanes[i], edge.Lanes[i-1]));
                //new GameObject().transform.position = intersection + GetPerpendicular().YTOZ().normalized * distanceBetweenPoints;
            }
            oppositeEdge = edge.GetOppositeEdge();
            tmpPoint = edge.Lanes[edge.Lanes.Count - 1].EdgeParts[sidewalkPartIndex];
            oppositeEdge.Lanes.Reverse();
            for (int i = 0; i < oppositeEdge.Lanes.Count; i++)
            {
                if(i == 0)
                {
                    //new GameObject().transform.position = tmpPoint;
                    //new GameObject().transform.position = oppositeEdge.Lanes[i].EdgeParts[oppositeEdge.Lanes[i].EdgeParts.Count - 1 - sidewalkPartIndex];

                    tmpPoint = (oppositeEdge.Lanes[i].EdgeParts[oppositeEdge.Lanes[i].EdgeParts.Count-1-sidewalkPartIndex] +
                        tmpPoint) / 2f;
                    //new GameObject().transform.position = oppositeEdge.Lanes[i].EdgeParts[oppositeEdge.Lanes[i].EdgeParts.Count - 1 - sidewalkPartIndex];
                    //new GameObject().transform.position = tmpPoint;

                    distanceBetweenPoints = (-GraphExtensions.GetProperSidewalkPoints(edge, sidewalkIndex)[sidewalkPartIndex]
                   + tmpPoint).magnitude;
                    crossingPoints.Add(new CrossingPoint(intersection +
    GetPerpendicular().YTOZ().normalized * distanceBetweenPoints, CrossingPoint.CrossingPointType.MIDDLE,oppositeEdge.Lanes[i]
    , edge.Lanes[edge.Lanes.Count - 1]));
                    //new GameObject().transform.position = intersection + GetPerpendicular().YTOZ().normalized * distanceBetweenPoints;
                }
                else
                {
                    tmpPoint = (oppositeEdge.Lanes[i].EdgeParts[oppositeEdge.Lanes[i].EdgeParts.Count - 1 - sidewalkPartIndex] +
                  oppositeEdge.Lanes[i-1].EdgeParts[oppositeEdge.Lanes[i].EdgeParts.Count - 1 - sidewalkPartIndex]) / 2f;
                    distanceBetweenPoints = (-GraphExtensions.GetProperSidewalkPoints(edge, sidewalkIndex)[sidewalkPartIndex]
                   + tmpPoint).magnitude;
                    crossingPoints.Add(new CrossingPoint(intersection +
    GetPerpendicular().YTOZ().normalized * distanceBetweenPoints, CrossingPoint.CrossingPointType.MIDDLE, oppositeEdge.Lanes[i]
    , oppositeEdge.Lanes[i-1]));
                    //new GameObject().transform.position = intersection + GetPerpendicular().YTOZ().normalized * distanceBetweenPoints;
                }
            }
            oppositeEdge.Lanes.Reverse();
            tmp = GraphExtensions.GetProperSidewalkPoints(oppositeEdge, sidewalkIndex);
            distanceBetweenPoints = (-GraphExtensions.GetProperSidewalkPoints(edge, sidewalkIndex)[sidewalkPartIndex]
               + tmp[tmp.Count - 1 - sidewalkPartIndex]).magnitude;
            CrossingPoint.intersectionSidewalk = tmp;
            crossingPoints.Add(new CrossingPoint(intersection +
    GetPerpendicular().YTOZ().normalized * distanceBetweenPoints, CrossingPoint.CrossingPointType.END,
    oppositeEdge.Lanes[oppositeEdge.Lanes.Count-1],null));
            //new GameObject().transform.position = intersection + GetPerpendicular().YTOZ().normalized * distanceBetweenPoints;
        }
        for (int i = 0; i < crossingPoints.Count; i++)
        {
            if (i == 0)
                crossingPoints[i].Type = CrossingPoint.CrossingPointType.START;
            else if (i == crossingPoints.Count - 1)
                crossingPoints[i].Type = CrossingPoint.CrossingPointType.END;
            else crossingPoints[i].Type = CrossingPoint.CrossingPointType.MIDDLE;


            //crossingPoints[i].SafetyPosition.y = intersection.y;
        }

        for (int i = 1;  i < crossingPoints.Count; i++)
        {
            crossingPoints[i].previousCrossingPoint = crossingPoints[i - 1];
            crossingPoints[i].CorrectDirectionOfCrossingPoint();
        }

    }



    //////////////CODE ABOUT CARS///////////////////////////////////////////
    ///
    
    public MovingEntity GetMostImportantMovingEntity()
    {
        if (GetCarsOnLane().Count == 0)
            return null;
        int index = -1; float distance = float.MaxValue;float dot = 0;
        for (int i = 0; i < GetCarsOnLane().Count; i++)
        {
            dot = Vector3.Dot((mAgent.transform.position- GetCarsOnLane()[i].transform.position).normalized, GetCarsOnLane()[i].Velocity.normalized);
            Debug.Log("Dot of I:"+i+ " is :"+dot);
            if (dot >0 && distance > (GetCarsOnLane()[i].transform.position - GetPointOfInterest()).magnitude)
            {
                distance = (GetCarsOnLane()[i].transform.position - GetPointOfInterest()).magnitude;
                index = i;
            }
        }
        if (index == -1)
            return null;
        return GetCarsOnLane()[index];
    }
    private List<MovingEntity> GetCarsOnLane()
    {
        return crossingPoints[mAgent.Cache.GetIndexOfCurrentIntermediateGoal
            (crossingPoints)].laneToObserveForCars.MovingEntitiesOnLane;
    }
    private Vector3 GetPointOfInterest()
    {
        return crossingPoints[mAgent.Cache.GetIndexOfCurrentIntermediateGoal
            (crossingPoints)].SafetyPosition;
    }
   
    private float distanceToSafety()
    {
        return 0;
    }

    //////////////CODE ABOUT CARS END///////////////////////////////////////////
    ///////////////////////--------------------------------/////////////////////
    //////////////CODE ABOUT SIDEWALKMOVEMENT///////////////////////////////////
    
    public Vector3 GetSidewalkDir()
    {
        Vector3 dir1,dir2, dir = Vector3.zero;
        dir = ((HumanContext)mAgent.GetContext()).originalGoal - mAgent.transform.position;
        SimpleGraphRoadNetwork.Lane lane = crossingPoints[mAgent.Cache.GetIndexOfCurrentIntermediateGoal(crossingPoints)]
                .laneToObserveForCars;
        try
        {
            dir1 = lane.EdgeParts[sidewalkPartIndex - 1] - lane.EdgeParts[sidewalkPartIndex];
        } catch (System.Exception ex) {
            dir1 = Vector3.zero;
        }
        try
        {
            dir2 = lane.EdgeParts[sidewalkPartIndex + 1] - lane.EdgeParts[sidewalkPartIndex];
        }
        catch (System.Exception ex)
        {
            dir2 = Vector3.zero;
        }
        if (dir1 == Vector3.zero && dir2 == Vector3.zero)
            return dir1;
        //Debug.Log
        if (dir1 == Vector3.zero&& Vector3.Dot(dir.normalized, dir2.normalized)>0.01)
            return dir2;
        if (dir2== Vector3.zero && Vector3.Dot(dir.normalized, dir1.normalized) > 0.01)
            return dir1;
      
        if (dir1!= Vector3.zero && dir2!= Vector3.zero)
        {
            if (Vector3.Dot(dir.normalized, dir1.normalized) > Vector3.Dot(dir.normalized, dir2.normalized))
            {
                return dir1;
            }
            else
                return dir2;
        }
        return Vector3.zero;

        ///There is a chance that we need to have a zero direction 
        ///not being able t follow the proper direction.
    }

    public Vector3 GetSidewalkDirNew()
    {
        CrossingPoint current = crossingPoints[mAgent.Cache.GetIndexOfCurrentIntermediateGoal(crossingPoints)];
        int index=current.FindAtWhichPart(mAgent.transform.position);
        Vector3 dir,dir1, dir2;
        dir1 = current.movesOn[index] - current.movesOn[index + 1];
        dir2 = current.movesOn[index+1] - current.movesOn[index ];
        dir = ((HumanContext)mAgent.GetContext()).originalGoal - mAgent.transform.position;

        if (Vector3.Dot(dir, dir1) > Vector3.Dot(dir, dir2))
            return dir1;
        return dir2;
    }
}

public class CrossingPoint
{
    public static List<Vector3> intersectionSidewalk;
    public enum CrossingPointType {START,MIDDLE,END}
    public CrossingPoint previousCrossingPoint { get; set; }
    private Vector3 safetyPosition;
    private CrossingPointType type;
    /// <summary>
    /// I have the edge from this as well. We may need to use the edge when we want 
    /// to query for the whole road, and not only the lane.
    /// </summary>
    public SimpleGraphRoadNetwork.Lane laneToObserveForCars { get; private set; }
    
    public SimpleGraphRoadNetwork.Lane previousLane { get;private set; }

    public List<Vector3> movesOn = new List<Vector3>();
    public CrossingPointType Type
    {
        get
        {
            return type;
        }

        set
        {
            type = value;
        }
    }
    private int index = -1;
    public Vector3 SafetyPosition
    {
        get
        {
            return safetyPosition;
        }
        set
        {
            safetyPosition = value;
        }
    }
    public int FindAtWhichPart(Vector3 position)
    {
        float dis1;float dis2,dis3;float difference = float.MaxValue;int index = -1;
        for (int i = 0; i < movesOn.Count-1; i++)
        {
            dis1 = (position - movesOn[i]).magnitude;
            dis2 = (position - movesOn[i+1]).magnitude;
            dis3 = (movesOn[i] - movesOn[i + 1]).magnitude;

            if (difference > Mathf.Abs(dis3 - dis1 - dis2))
            {
                index = i;
                difference = Mathf.Abs(dis3 - dis1 - dis2);
            }
        }
        return index;
    }

    //I idea einai oti theorw t swsto direction na einai to prwto pezodromio p vriskw kai kinoume panw
    // diladi to moveOn.

    private List<Vector3> GetFirstCrossingPointList()
    {
        CrossingPoint tmp=previousCrossingPoint;
        if (tmp == null)
        {
            Debug.Log("No val reee");
            return null;
        }
        while (tmp.previousCrossingPoint != null)
        {
            tmp = tmp.previousCrossingPoint;
        }
        
        return tmp.movesOn;
    }

    private void ProperDirection(List<Vector3> CheckIfReverseNeeded)
    {
        List<Vector3> reference = GetFirstCrossingPointList();

        for (int i = 0; i < reference.Count-1; i++)
        {
           if( Vector3.Dot((reference[i] - reference[i + 1]).normalized, (CheckIfReverseNeeded[i] - CheckIfReverseNeeded[i + 1]).normalized)<0.99f)
            {
                CheckIfReverseNeeded.Reverse();
                return;
            }
        }
    }
    public void CorrectDirectionOfCrossingPoint()
    {
        if (previousCrossingPoint == null)
            return;
        ProperDirection(movesOn);
    }
    //public CrossingPoint(Vector3 pos, CrossingPointType type,SimpleGraphRoadNetwork.Lane lane,SimpleGraphRoadNetwork.Lane prev)
    //{
    //    this.laneToObserveForCars = lane;
    //    safetyPosition = pos;
    //    this.type = type;
    //    this.previousLane = prev;
    //    if(type==CrossingPointType.START || type == CrossingPointType.END)
    //    {
    //            movesOn = intersectionSidewalk;///THe end crossing point will have wrong assignemnt. :(
    //    }
    //    else
    //    {
    //        if (lane.edge != prev.edge)
    //        {
    //            lane.EdgeParts.Reverse();
    //        }
    //        for (int i = 0; i < lane.EdgeParts.Count; i++)
    //        {
    //            movesOn.Add((lane.EdgeParts[i] + prev.EdgeParts[i]) / 2f);
    //        }
    //        if(lane.edge != prev.edge)
    //        {
    //            lane.EdgeParts.Reverse();
    //        }
    //    }
    //    foreach (var item in movesOn)
    //    {
    //        //new GameObject().transform.position = item;
    //    }
    //    index = FindAtWhichPart(safetyPosition);
    //}
    public CrossingPoint(Vector3 pos, CrossingPointType type, SimpleGraphRoadNetwork.Lane lane, SimpleGraphRoadNetwork.Lane prev)
    {
        this.laneToObserveForCars = lane;
        safetyPosition = pos;
        this.type = type;
        this.previousLane = prev;
        if (type == CrossingPointType.START || type == CrossingPointType.END)
        {
            movesOn = new List<Vector3>();
            foreach (var item in intersectionSidewalk)
            {
                movesOn.Add(item);
            }
        }
        else
        {
            bool getItNormal = false;
            if(Vector3.Dot(lane.EdgeParts[0]-lane.EdgeParts[1], prev.EdgeParts[0]- prev.EdgeParts[1]) < 0.99)
            {
                lane.EdgeParts.Reverse();
                getItNormal = true;
            }
            for (int i = 0; i < lane.EdgeParts.Count; i++)
            {               
                movesOn.Add((lane.EdgeParts[i] + prev.EdgeParts[i]) / 2f);
            }
            if (getItNormal)
            {
                lane.EdgeParts.Reverse();
            }
        }
    }
    /// <summary>
    /// Vriskw optimal point kai kala.
    /// This function should update the index.
    /// </summary>
    /// <param name="agent"></param>
    /// <param name="goal"></param>
    public void FindIntersectionFreely(Vector3 agent,Vector3 goal)
    {
        Vector3 intersection;
        for (int i = 0; i < movesOn.Count-1; i++)
        {
            if(Intersection.LineSegmentsIntersection(agent, goal, movesOn[i], movesOn[i + 1], out intersection))
                safetyPosition = intersection;
        }
    }
    
    public void FindPerpendicularIntersection()
    {
        Vector3 perp = Vector2.Perpendicular((movesOn[index + 1] - movesOn[index]).XZ()).YTOZ();
    }
    private Vector3 perpendicular,roadDirection;

    public void FindCurrentIndex(Vector3 position)
    {
        int i = 0;
        while (Intersection.CheckIfLeft(position, movesOn[i], previousCrossingPoint.movesOn[i]))
            i++;
        index = i;
    }
    public bool CheckPositionOutsideRoad(Vector3 newPosition)
    {
        int len = movesOn.Count;
        if (previousCrossingPoint != null)
        {
            if (Intersection.CheckIfLeft(newPosition, movesOn[0], previousCrossingPoint.movesOn[0]) == Intersection.CheckIfLeft(newPosition, movesOn[len - 1], previousCrossingPoint.movesOn[len - 1]))
                return true;
        }
        else
        {
            if (Intersection.CheckIfLeft(newPosition, movesOn[0], previousCrossingPoint.movesOn[0]) == Intersection.CheckIfLeft(newPosition, movesOn[len - 1], previousCrossingPoint.movesOn[len - 1]))
                return true;
        }

        return false;
    }

    public void FindNewSafetyPosition(Vector3 position)
    {
        GameObject tmp;
        Vector3 interPoint;
        int i = 0;
        Vector3 dir1 = movesOn[0] - movesOn[1];Vector3 dir2 = previousCrossingPoint.movesOn[0] - previousCrossingPoint.movesOn[1];
        //Debug.Log("Dot" + Vector3.Dot(dir1.normalized, dir2.normalized));

        if (Vector3.Dot(dir1.normalized, dir2.normalized) < 0.99f)
        {
            previousCrossingPoint.movesOn.Reverse();
        }
        dir1 = movesOn[0] - movesOn[1]; dir2 = previousCrossingPoint.movesOn[0] - previousCrossingPoint.movesOn[1];
        //Debug.Log("Dot" + Vector3.Dot(dir1.normalized, dir2.normalized));
        //Debug.Log("movesOn len:" + movesOn.Count);
        //Debug.Log("PrevOn len:" + previousCrossingPoint.movesOn.Count);
        GameObject g;
        //for (int ind = 0; ind < movesOn.Count; ind++)
        //{
        //    //Debug.DrawRay(previousCrossingPoint.movesOn[ind], movesOn[ind] - previousCrossingPoint.movesOn[ind, Color.cyan, 10);
        //    g=new GameObject();
        //    g.name = "This:" + ind;
        //    g.transform.position = movesOn[ind];
        //    g = new GameObject();
        //    g.name = "Prev:" + ind;
        //    g.transform.position = previousCrossingPoint.movesOn[ind];
        //}

        while (Intersection.CheckIfLeft(position, movesOn[i], previousCrossingPoint.movesOn[i]))
        {
            //new GameObject().transform.position = previousCrossingPoint.movesOn[i];
            //Debug.DrawRay(previousCrossingPoint.movesOn[i], movesOn[i]- previousCrossingPoint.movesOn[i], Color.cyan,10);
            i++;
            if (i == movesOn.Count)
            {
                i = 0;
                break;
            }
        }
        index = i;
        perpendicular = (movesOn[i] - previousCrossingPoint.movesOn[i])*3;
        if (i == 0)
        {
            i += 1;
        }
        roadDirection = movesOn[i] - movesOn[i - 1];
        Debug.DrawRay(position, perpendicular, Color.black, 5);
        //Debug.DrawRay(movesOn[i - 1], movesOn[i]- movesOn[i - 1], Color.blue, 5);
        if (Intersection.LineSegmentsIntersection(position, position + perpendicular,
            movesOn[i - 1], movesOn[i],out interPoint)|| Intersection.LineSegmentsIntersection(position, position - perpendicular,
            movesOn[i - 1], movesOn[i], out interPoint))
        safetyPosition = interPoint;
        previousCrossingPoint.movesOn.Reverse();
    }
    public void FindNewSafetyPosition(Vector3 position, CrossingPoint next)
    {
        GameObject tmp;
        Vector3 interPoint;
        int i = 0;
        Vector3 dir1 = movesOn[0] - movesOn[1]; Vector3 dir2 = next.movesOn[0] - next.movesOn[1];
        if (Vector3.Dot(dir1.normalized, dir2.normalized) < 0.99f)
        {
            next.movesOn.Reverse();
            Debug.Log("Reverse");
        }
        Debug.Log("Dot"+ Vector3.Dot(dir1.normalized, dir2.normalized));

        while (Intersection.CheckIfLeft(position,next.movesOn[i], movesOn[i]))
        {
            i++;
            if (i == movesOn.Count-1)
            {
                i = 1;
                break;
            }
        }
        if (i == 0)
            i = 1;
        Debug.Log("inedx" + i);
        index = i;
        perpendicular = (next.movesOn[i]- movesOn[i]) * 3;
        roadDirection = movesOn[i] - movesOn[i - 1];
        Debug.DrawRay(position, perpendicular, Color.black, 5);
        Debug.DrawRay(movesOn[i - 1], movesOn[i] - movesOn[i - 1], Color.blue, 5);
        if (Intersection.LineSegmentsIntersection(position, position + perpendicular,
            movesOn[i - 1], movesOn[i], out interPoint) || Intersection.LineSegmentsIntersection(position, position - perpendicular,
            movesOn[i - 1], movesOn[i], out interPoint))
            safetyPosition = interPoint;
    }
    private CrossingPoint FirstCrossingPoint()
    {
        if (previousCrossingPoint == null)
            return this;
        else
            return FirstCrossingPoint();
    }
    
    public Vector3 GetSidewalkDirNew(Vector3 position,Vector3 original)
    {   
        CrossingPoint current = this;
        int index = current.FindAtWhichPart(position);
        Vector3 dir, dir1, dir2;
        dir1 = current.movesOn[index] - current.movesOn[index + 1];
        dir2 = current.movesOn[index + 1] - current.movesOn[index];
        dir = original - position;
        Debug.DrawRay(position, dir, Color.green, 1.125f);
        if (Vector3.Dot(dir.normalized, dir1.normalized) > Vector3.Dot(dir.normalized, dir2.normalized))
            return dir1;
        return dir2;
    }

    //public float FindNearestJunctionDanger()
    //{

    //    Node start = laneToObserveForCars.edge.StartNode;
    //    Node end = laneToObserveForCars.edge.EndNode;

        
    //}

    


}
}
