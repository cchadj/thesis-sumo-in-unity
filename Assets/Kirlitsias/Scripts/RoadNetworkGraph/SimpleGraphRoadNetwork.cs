using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
using KdTree;
using KdTree.Math;
using UnityQuery;

namespace RiseProject.Kirlitsias
{

    public class SimpleGraphRoadNetwork : MonoBehaviour
    {

        public static string xmlName;

        [System.Serializable]
        public class Graph
        {
            private List<Node> nodes = new List<Node>();
            private List<Edge> edges = new List<Edge>();

            private Dictionary<string, Node> nodeDictionary = new Dictionary<string, Node>();

            public List<Node> Nodes
            {
                get { return nodes; }

            }

            /// <summary>
            /// Finds the shortest node graph from the agent
            /// Maybe do one for the edges as well.
            /// Should probably have overloads or ...
            /// </summary>
            /// <returns></returns>
            public Node FindNearestNode()
            {
                return null;
            }

            public void DrawGraphWithRays()
            {
                foreach (Node node in nodes)
                {
                    foreach (Edge edge in node.OutEdges)
                    {
                        foreach (List<Vector3> list in edge.ReturnStartToEndPoints())
                        {
                            DrawRaysFromList(list);
                        }
                    }
                }
            }

            public void CreateRoadDataStructures()
            {

                foreach (var item in edges)
                {
                    if (item.mRoad == null)
                    {
                        item.mRoad = new Road(item);
                    }
                }
            }

            public void CreateRoadsWithPlanes(GameObject prefab)
            {
                Vector3 position, point1, point2, point3, point4;
                Vector3[] fpoints = new Vector3[4];
                List<Vector3> points = new List<Vector3>();
                Vector3 direction;
                Vector3 scale = new Vector3(2, 1, 1);
                Vector3 extraY = new Vector3(0, 0.1f, 0);
                Vector3 perpendicular;
                Mesh newMesh = new Mesh();
                Vector2[] uvs = new Vector2[4];

                foreach (var node in nodes)
                {
                    foreach (Edge edge in node.OutEdges)
                    {
                        foreach (List<Vector3> list in edge.ReturnStartToEndPoints())
                        {

                            for (int i = 0; i < list.Count - 1; i++)
                            {
                                if (list.Count != edge.sidewalk.Count) continue;
                                Debug.Log(list.Count + " | " + edge.sidewalk.Count);
                                perpendicular = edge.sidewalk[i] - list[i];
                                fpoints[0] = list[i] + perpendicular.normalized * 2;
                                fpoints[1] = list[i] - perpendicular.normalized * 2;
                                perpendicular = edge.sidewalk[i + 1] - list[i + 1];
                                fpoints[2] = list[i + 1] + perpendicular.normalized * 2;
                                fpoints[3] = list[i + 1] - perpendicular.normalized * 2;
                                uvs[0] = fpoints[0].XZ();
                                uvs[1] = fpoints[1].XZ();
                                uvs[2] = fpoints[2].XZ();
                                uvs[3] = fpoints[3].XZ();
                                newMesh.vertices = fpoints;
                                newMesh.triangles = new int[] {0, 2, 1, 2, 3, 1};
                                newMesh.uv = uvs;
                                GameObject o = Instantiate(prefab);

                                o.GetComponent<MeshFilter>().mesh = newMesh;

                                //position = (list[i] + list[i + 1]) / 2f+ extraY;
                                //direction = (list[i + 1] - list[i]);
                                //scale.z *= direction.magnitude;

                                //scale.z =1;
                                newMesh = new Mesh();
                                fpoints = new Vector3[4];
                                uvs = new Vector2[4];
                            }
                        }
                    }
                }
            }

            public void CreateWholeRoad(GameObject prefab)
            {
                Vector3 position, point1, point2, point3, point4;

                Vector3[] fpoints = new Vector3[4];
                List<Vector3> points = new List<Vector3>();
                Vector3 direction;
                Vector3 scale = new Vector3(2, 1, 1);
                Vector3 extraY = new Vector3(0, 0.1f, 0);
                Vector3 perpendicular;
                Mesh newMesh = new Mesh();
                Vector2[] uvs = new Vector2[4];

                foreach (var node in nodes)
                {
                    foreach (Edge edge in node.OutEdges)
                    {
                        List<List<Vector3>> sidewalks = edge.ReturnBothSidewalks();
                        Debug.Log("sidewalk number: " + sidewalks.Count);
                        if (sidewalks.Count != 2)
                            continue;
                        for (int i = 0; i < sidewalks[0].Count - 1; i++)
                        {
                            if (sidewalks[0].Count != sidewalks[1].Count)
                                continue;
                            //fpoints[0] = sidewalks[0][i];
                            //fpoints[1] = sidewalks[1][i];
                            //fpoints[2] = sidewalks[0][i+1];
                            //fpoints[3] = sidewalks[1][i+1];
                            //uvs[0] = fpoints[0].XZ();
                            //uvs[1] = fpoints[1].XZ();
                            //uvs[2] = fpoints[2].XZ();
                            //uvs[3] = fpoints[3].XZ();
                            //newMesh.vertices = fpoints;
                            //newMesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
                            //newMesh.uv = uvs;
                            Vector3 center = (sidewalks[0][i] + sidewalks[1][i] + sidewalks[0][i + 1] +
                                              sidewalks[1][i + 1]) / 4f;

                            Vector3 sc = new Vector3(sidewalks[0][i].x - sidewalks[1][i].x, 1,
                                sidewalks[0][i + 1].z - sidewalks[0][i].z);
                            Vector3 dir = sidewalks[0][i + 1] - sidewalks[0][i];
                            GameObject o = Instantiate(prefab, center, Quaternion.LookRotation(dir, Vector3.up));
                            o.transform.localScale = sc;

                        }
                    }
                }
            }

            private void DrawRaysFromList(List<Vector3> list)
            {
                int counter = list.Count - 2;

                for (; counter >= 0; counter--)
                {
                    Debug.DrawRay(list[counter], list[counter + 1] - list[counter], Color.red);
                }
            }

            public void PopulateGraphFromXml(string xmlFile)
            {
                xmlName = Directory.GetFiles(@"Assets\RoadNet", "*.net.xml", SearchOption.AllDirectories)[0];
                Debug.Log(xmlName);
                XDocument doc = XDocument.Load(xmlName);
                ReadJunctions(doc);
                ReadEdges(doc);
            }

            public bool MeshInitializationFinished = false;

            public IEnumerator InitializeFromMesh()
            {
                MeshInitializationFinished = false;
                MeshToGraph meshToGraph = null;
                yield return new WaitUntil(() => (meshToGraph = FindObjectOfType<MeshToGraph>()) != null);

                yield return new WaitUntil(() => meshToGraph.IsReady());
                Node newNode;
                foreach (var item in meshToGraph.Junctions)
                {
                    nodes.Add(GetJunctionFromMeshToGraphObj(item));
                }

                Debug.Log("Nodes added");
                foreach (var item in meshToGraph.Edges)
                {
                    edges.Add(GetEdgeFromMeshToGraphObj(item));
                    Debug.Log("edge added");
                }

                MeshInitializationFinished = true;

                //yield return new WaitUntil()
            }

            private static int JunctionId = 0;

            private Node GetJunctionFromMeshToGraphObj(Junction junc)
            {
                Node newNode;
                newNode = new Node();
                newNode.SetPositionXFromString(junc.position.x.ToString());
                newNode.SetPositionZFromString(junc.position.z.ToString());
                newNode.SetPositionY(junc.position.y);
                newNode.Id = JunctionId + "";
                junc.id_Same_With_Node = newNode.Id;
                JunctionId++;
                nodeDictionary.Add(newNode.Id, newNode);

                return newNode;
            }

            private Edge GetEdgeFromMeshToGraphObj(TmpEdhe edge)
            {
                Edge newEdge = new Edge();
                Node node;
                Lane lane = new Lane();
                newEdge.AddLane(lane);
                foreach (var item in edge.s)
                {
                    lane.AddLanePart(item);
                }

                newEdge.Length = lane.Length;
                if (!nodeDictionary.TryGetValue(edge.Start.id_Same_With_Node, out node)) ;
                newEdge.StartNode = node;
                if (!nodeDictionary.TryGetValue(edge.End.id_Same_With_Node, out node)) ;
                newEdge.EndNode = node;
                newEdge.StartNode.OutEdges.Add(newEdge);
                newEdge.EndNode.InEdges.Add(newEdge);
                //edges.Add(newEdge);
                return newEdge;
            }


            private void ReadJunctionsFromMeshToGraph()
            {

            }

            private void ReadJunctions(XDocument doc)
            {
                var junctionElements = doc.Root
                    .Elements("junction");
                Node newNode;
                foreach (XElement junctionElement in junctionElements)
                {
                    if (junctionElement.Attribute("type").Value.Equals("internal") ||
                        junctionElement.Attribute("type").Value.Equals("dead_end"))
                    {
                        continue;
                    }

                    //Debug.Log("Hey");
                    nodes.Add(newNode = new Node());
                    newNode.SetPositionXFromString(junctionElement.Attribute("x").Value);
                    newNode.SetPositionZFromString(junctionElement.Attribute("y").Value);
                    newNode.Id = junctionElement.Attribute("id").Value;
                    nodeDictionary.Add(newNode.Id, newNode);
                }
            }

            private void ReadEdges(XDocument doc)
            {
                var edgeElements = doc.Root
                    .Elements("edge");
                Edge newEdge;
                Node node;
                Lane newLane;
                string[] shapePositions;
                //string attr = null;
                foreach (XElement edgeElement in edgeElements)
                {
                    if (edgeElement.Attribute("function") != null &&
                        edgeElement.Attribute("function").Value.Equals("internal"))
                    {
                        //Debug.Log("Internal Edge");
                        continue;
                    }

                    newEdge = new Edge();
                    newEdge.Id = edgeElement.Attribute("id").Value;
                    if (!nodeDictionary.TryGetValue((string) edgeElement.Attribute("from"), out node))
                    {

                        //Debug.Log("Juncition id missing:" + (string)edgeElement.Attribute("from"));
                        continue;
                    }

                    newEdge.StartNode = node;
                    if (newEdge.StartNode.Id == "35612088")
                    {
                        Debug.Log("12345");
                    }

                    if (!nodeDictionary.TryGetValue((string) edgeElement.Attribute("to"), out node))
                    {
                        //Debug.Log("Juncition id missing:" + (string)edgeElement.Attribute("to"));
                        continue;
                    }

                    newEdge.EndNode = node;
                    edges.Add(newEdge);

                    //List<Lane> curEdgeLanes = new List<Lane>();
                    /* http://sumo.sourceforge.net/userdoc/Networks/SUMO_Road_Networks.html */
                    /* <lane id="<ID>_1" index="1" speed="<SPEED>" length="<LENGTH>" shape="0.00,498.35,2.00 248.50,498.35,3.00"/> */
                    var laneElements = edgeElement.Elements("lane");
                    foreach (XElement laneElement in laneElements)
                    {
                        newLane = new Lane();
                        //newLane.i = (string)laneElement.Attribute("id");
                        newLane.index = (int) laneElement.Attribute("index");
                        newLane.Speed = (float) laneElement.Attribute("speed");
                        newLane.Length = (float) laneElement.Attribute("length");
                        //newLane.EdgeID = edgeID;
                        //newLane.Edge = newEdge;
                        newLane.XAttribute = laneElement.Attribute("shape");
                        shapePositions = ((string) laneElement.Attribute("shape")).Split(null);
                        for (int i = 0; i < shapePositions.Length; i++)
                        {
                            string[] point = shapePositions[i].Split(',');
                            newLane.AddLanePart(new Vector3(
                                float.Parse(point[0], CultureInfo.InvariantCulture.NumberFormat), 0,
                                float.Parse(point[1], CultureInfo.InvariantCulture.NumberFormat)));

                            /* UseCultureInfo.InvariantCulture.NumberFormat for . decimal mark */
                        }

                        newLane.edge = newEdge;
                        newEdge.AddLane(newLane);
                        newEdge.Length = newLane.Length;
                    }

                    newEdge.EndNode.InEdges.Add(newEdge);
                    newEdge.StartNode.AddEdge(newEdge);
                }
            }

            public void PopulateJunctionTree(KdTree<float, Node> kdtree)
            {
                foreach (Node node in nodes)
                {
                    kdtree.Add(new[] {node.GetPositionX(), node.GetPositionZ()}, node);
                }
            }

            public void PopulateEdgeTree(KdTree<float, Edge> kdtree)
            {
                foreach (Node node in nodes)
                {
                    foreach (Edge edge in node.OutEdges)
                    {
                        foreach (List<Vector3> list in edge.ReturnStartToEndPoints())
                        {
                            foreach (Vector3 point in list)
                            {
                                kdtree.Add(new[] {point.x, point.z}, edge);
                            }
                        }
                    }
                }
            }

            public void GenerateSidewalks()
            {
                foreach (Edge edge in edges)
                {
                    edge.GenerateSidewalk(3.5f);
                }
            }

            public void VisualizeSidewalksWithRays()
            {

                foreach (Edge edge in edges)
                {
                    for (int i = 1; i < edge.sidewalk.Count; i++)
                    {
                        Debug.DrawRay(edge.sidewalk[i - 1], edge.sidewalk[i] - edge.sidewalk[i - 1], Color.yellow);
                    }

                    if (edge.sidewalkMonodromos == null)
                        continue;
                    for (int i = 1; i < edge.sidewalkMonodromos.Count; i++)
                    {
                        Debug.DrawRay(edge.sidewalkMonodromos[i - 1],
                            edge.sidewalkMonodromos[i] - edge.sidewalkMonodromos[i - 1], Color.yellow);
                    }
                }
            }

            public void FindOneWayRoads()
            {
                foreach (Node node in nodes)
                {
                    node.FindOneWaysThatStartFromMe();
                }
            }

            //public void PopulateEdgeTree
        }

        public class Road
        {
            private List<Edge> edges = new List<Edge>();
            private int numberOfLanes;
            private int speedLimit;
            private List<IAgentOnRoadInfo> agentsOnRoad = new List<IAgentOnRoadInfo>();

            //private List<vec>


            /// <summary>
            /// Ambitious retarted writing
            /// </summary>
            private int[] agentIndexes = new int[100];

            /// <summary>
            /// Assign
            /// </summary>
            /// <param name="agent"></param>
            public void AssignAgentOnRoad(IAgentOnRoadInfo agent)
            {
                agentsOnRoad.Add(agent);
            }

            public void UnassignAgentFromRoad(IAgentOnRoadInfo agent)
            {
                agentsOnRoad.Remove(agent);
            }

            private IndexFinder indexFinder;

            public Road(Edge current)
            {
                if (current.mRoad != null)
                    return;
                current.mRoad = this;
                edges.Add(current);
                if (current.sidewalkMonodromos == null)
                {
                    edges.Add(current.GetOppositeEdge());
                    current.GetOppositeEdge().mRoad = this;
                }

                numberOfLanes = 0;
                foreach (var item in edges)
                {
                    numberOfLanes += item.Lanes.Count;
                }

                int type;
                if ((type = CanCheckIndexes()) != -1)
                {
                    if (type == 1)
                    {
                        indexFinder = new IndexFinder(edges[0].sidewalk, edges[0].sidewalkMonodromos);
                    }
                    else if (type == 2)
                    {
                        indexFinder = new IndexFinder(edges[0].sidewalk, edges[1].sidewalk);
                    }
                }
            }

            private bool ShouldReverse(Edge edge)
            {
                if (edges.Count == 1)
                    return false;
                if (edges[1] == edge)
                    return true;
                return false;
            }

            public Road(Edge current, int speedLimit) : this(current)
            {
                this.speedLimit = speedLimit;
            }

            public void GetPedestrianDangersForCar(IEntity carEntity, Lane lane)
            {
                int carIndex = 1;

                foreach (var item in agentsOnRoad)
                {

                }
            }

            private int CanCheckIndexes()
            {
                if (edges.Count == 2)
                    return 2;
                if (edges.Count == 1)
                {
                    if (edges[0].sidewalkMonodromos == null)
                        return -1;
                    else return 1;
                }

                return -1;
            }

            /// <summary>
            /// points of sidewalk- used to determine where cars and agents are on the graph.
            /// </summary>
            private List<Vector3> side1 = new List<Vector3>();

            /// <summary>
            /// points of sidewalk 2- used to determine where cars and agents are on the graph.
            /// </summary>
            private List<Vector3> side2 = new List<Vector3>();



            private void CreateList(int type)
            {
                switch (type)
                {
                    case 1: //Means Monodromos
                        side1 = new List<Vector3>(edges[0].sidewalk.ToArray());
                        side2 = new List<Vector3>(edges[0].sidewalkMonodromos.ToArray());
                        side2.Reverse();
                        break;
                    case 2: //Means 2plis
                        side1 = new List<Vector3>(edges[0].sidewalk.ToArray());
                        side2 = new List<Vector3>(edges[1].sidewalk.ToArray());
                        side2.Reverse();
                        break;
                    default:
                        Debug.LogError("Kati prp na kammw polla lathos gia na fkalei lathos dame");
                        break;
                }
            }

            public int NumberOfLanes
            {
                get { return numberOfLanes; }

                set { numberOfLanes = value; }
            }

            public int SpeedLimit
            {
                get { return speedLimit; }

                set { speedLimit = value; }
            }

            public class IndexFinder
            {
                /// <summary>
                /// points of sidewalk- used to determine where cars and agents are on the graph.
                /// </summary>
                private List<Vector3> side1 = new List<Vector3>();

                /// <summary>
                /// points of sidewalk 2- used to determine where cars and agents are on the graph.
                /// </summary>
                private List<Vector3> side2 = new List<Vector3>();

                private Vector3 indexZEROpoint;
                private Vector3 indexLastpoint;

                public IndexFinder(List<Vector3> s1, List<Vector3> s2)
                {
                    this.side1 = s1;
                    this.side2 = s2;
                    indexZEROpoint = (s1[0] + s2[0]) / 2f;
                    int count = side1.Count;
                    indexLastpoint = (s1[count - 1] + s2[count - 1]) / 2f;
                }

                private void Reverse()
                {
                    side1.Reverse();
                    side2.Reverse();
                }

                public int IndexOfPoint(Vector3 pointInQuestion, bool reverse)
                {
                    if (reverse)
                    {
                        Reverse();
                    }

                    for (int i = 1; i < side1.Count; i++)
                    {
                        if (Intersection.CheckIfLeft(pointInQuestion, side2[i], side1[i]))
                        {
                            if (reverse)
                                Reverse();
                            return i;
                        }
                    }

                    if (reverse)
                        Reverse();
                    return -1;
                }

            }

        }

        [System.Serializable]
        public class Node
        {
            private List<Edge> outEdges = new List<Edge>();
            private List<Edge> inEdges = new List<Edge>();
            private Vector3 position = new Vector3(0, 0, 0);
            public string Id { get; set; }
            public string Type { get; set; }

            public List<Edge> OutEdges
            {
                get { return outEdges; }
            }

            public Vector3 Position
            {
                get { return position; }
            }

            public List<Edge> InEdges
            {
                get { return inEdges; }

                set { inEdges = value; }
            }

            public float GetPositionX()
            {
                return position.x;
            }

            public float GetPositionZ()
            {
                return position.z;
            }

            public void SetPositionY(float y)
            {
                position.y = y;
            }

            public void SetPositionXFromString(string val)
            {
                position.x = float.Parse(val);
            }

            public void SetPositionZFromString(string val)
            {
                position.z = float.Parse(val);
            }

            public void AddEdge(Edge edge)
            {
                ///Should probably look whether the edge already exists.
                outEdges.Add(edge);
            }

            public void AddEdgeList(List<Edge> edges)
            {
                outEdges = edges;
            }

            /// <summary>
            /// The current implementation is from lidl.Will change it later if it works;p
            /// </summary>
            /// <returns></returns>
            public Edge GetShortestEdge()
            {
                if (outEdges == null || outEdges.Count == 0)
                    return null;
                outEdges.Sort();
                return outEdges[0];
            }

            private void DrawRaysFromList(List<Vector3> list)
            {
                int counter = list.Count - 2;

                for (; counter >= 0; counter--)
                {
                    Debug.DrawRay(list[counter], list[counter + 1] - list[counter], Color.red);
                }
            }

            public void DrawRaysForMe()
            {
                foreach (Edge edge in OutEdges)
                {
                    foreach (List<Vector3> list in edge.ReturnStartToEndPoints())
                    {
                        DrawRaysFromList(list);
                    }
                }
            }

            public void FindOneWaysThatStartFromMe()
            {
                bool oneway = true;
                foreach (Edge outedge in outEdges)
                {
                    foreach (Edge edge in outedge.EndNode.outEdges)
                    {
                        if (edge.EndNode == this)
                        {
                            oneway = false;
                            break;
                        }
                    }

                    outedge.IsMonodromos = oneway;
                    oneway = true;
                }
            }

            public List<AngleVector3> pointsAroundNode = null;

            //public void GetReducedSize()
            public void CreateABoundedArea()
            {
                try
                {
                    Vector3 centerPoint = position;
                    List<AngleVector3> vectors = new List<AngleVector3>();
                    pointsAroundNode = new List<AngleVector3>();
                    foreach (Edge edge in OutEdges)
                    {
                        pointsAroundNode.Add(new AngleVector3(edge.sidewalk[0], 0, 0, edge));
                        if (edge.IsMonodromos)
                        {
                            pointsAroundNode.Add(new AngleVector3(edge.sidewalkMonodromos
                                    [edge.sidewalkMonodromos.Count - 1], 1, edge.sidewalkMonodromos.Count - 1, edge));
                        }
                    }

                    foreach (var edge in InEdges)
                    {
                        pointsAroundNode.Add(new AngleVector3(edge.sidewalk[edge.sidewalk.Count - 1]
                            , 0, edge.sidewalk.Count - 1, edge));
                        if (edge.IsMonodromos)
                        {
                            pointsAroundNode.Add(new AngleVector3(edge.sidewalkMonodromos[0], 1, 0, edge));
                        }
                    }

                    //centerPoint = Vector3.zero;
                    //foreach (var item in pointsAroundNode)
                    //{
                    //    centerPoint += item;
                    //}
                    //centerPoint /= pointsAroundNode.Count;
                    foreach (var item in pointsAroundNode)
                    {
                        //vectors.Add(new AngleVector3(item));
                        //vectors[vectors.Count - 1].CaclulateAngle(centerPoint);
                        item.CaclulateAngle(centerPoint);
                    }

                    pointsAroundNode.Sort();
                    pointsAroundNode.Add(pointsAroundNode[0]); ///For easiest traversal
                }
                catch (System.Exception ex)
                {

                }
            }
        }

        [System.Serializable]
        public class Lane
        {
            public float Length { get; set; }
            public float Speed { get; set; }
            public XAttribute XAttribute { get; set; }
            public List<MovingEntity> MovingEntitiesOnLane { get; private set; }
            public Edge edge { get; set; }
            public int index;

            public Lane()
            {
                MovingEntitiesOnLane = new List<MovingEntity>();
            }

            public List<Vector3> EdgeParts
            {
                get { return edgeParts; }
            }

            public Vector3 GetDirection()
            {
                return EdgeParts[1] - EdgeParts[0];
            }

            private List<Vector3> edgeParts = new List<Vector3>();

            public void SaveLaneShape()
            {
                XAttribute.Value = EdgePartsToString();
                XAttribute.Document.Save(SimpleGraphRoadNetwork.xmlName);
            }

            private string EdgePartsToString()
            {
                string toRet = "";
                int count = 0;
                int num = edgeParts.Count;
                foreach (Vector3 part in edgeParts)
                {
                    count++;
                    toRet += part.x + ",";
                    if (count != num)
                        toRet += part.z + " ";
                    else
                        toRet += part.z;
                }

                return toRet;
            }

            public void AddLanePart(Vector3 part)
            {
                EdgeParts.Add(part);
            }

            public void SetEdgeParts(List<Vector3> edgeParts)
            {
                this.edgeParts = edgeParts;
            }
        }

        [System.Serializable]
        public class Edge : System.IComparable<Edge>
        {
            private Vector3 start;
            private Vector3 end;
            private List<MovingEntity> movingEntitiesOnEdge = new List<MovingEntity>();
            private bool isMonodromos = false;
            private List<Lane> lanes = new List<Lane>();

            public Road mRoad { get; set; }

            //private List<Vector3> edgeParts;
            public Edge()
            {

            }

            public Edge(Vector3 start, Vector3 end)
            {
                this.start = start;
                this.end = end;

            }

            public Edge(Vector3 start, Vector3 end, float length)
            {
                this.start = start;
                this.end = end;
                this.Length = length;
            }

            public float Length { get; set; }
            public string Id { get; set; }
            [SerializeField] public Node StartNode { get; set; }
            [SerializeField] public Node EndNode { get; set; }

            [SerializeField]
            public List<Lane> Lanes
            {
                get { return lanes; }
            }

            public int CompareTo(Edge other)
            {
                if (Length < other.Length)
                    return -1;
                if (Length == other.Length)
                    return 0;
                return 1;
            }

            public void AddLane(Lane lane)
            {
                lanes.Add(lane);
            }

            public int GetNumberOFSidewalks()
            {
                if (IsMonodromos)
                {
                    return 2;
                }

                return 1;
            }

            public List<List<Vector3>> ReturnStartToEndPoints()
            {
                List<List<Vector3>> allLanes = new List<List<Vector3>>();
                foreach (Lane lane in lanes)
                {
                    allLanes.Add(lane.EdgeParts);
                }

                return allLanes;
            }

            public List<List<Vector3>> ReturnBothSidewalks()
            {
                List<List<Vector3>> bothSidewalks = new List<List<Vector3>>();

                bothSidewalks.Add(sidewalk);
                if (sidewalkMonodromos != null)
                {
                    List<Vector3> opposite = new List<Vector3>(sidewalkMonodromos.ToArray());
                    opposite.Reverse();
                    bothSidewalks.Add(opposite);
                    return bothSidewalks;
                }

                if (GetOppositeEdge() != null)
                {
                    List<Vector3> opposite = new List<Vector3>(GetOppositeEdge().sidewalk.ToArray());
                    opposite.Reverse();
                    bothSidewalks.Add(opposite);
                    return bothSidewalks;
                }

                return null;
            }

            private List<Vector3> enhancedEdgePoints;

            public List<Vector3> GetEdgePointsForLineIntersection()
            {
                if (enhancedEdgePoints != null)
                    return enhancedEdgePoints;

                enhancedEdgePoints = new List<Vector3>();
                enhancedEdgePoints.Add(StartNode.Position);
                foreach (Vector3 point in Lanes[0].EdgeParts)
                {
                    enhancedEdgePoints.Add(point);
                }

                enhancedEdgePoints.Add(EndNode.Position);
                return enhancedEdgePoints;
            }

            public Vector3 GetFullDirection()
            {
                return (EndNode.Position - StartNode.Position).normalized;
            }

            public Vector3 GetFirstPartDirection()
            {
                return (Lanes[0].EdgeParts[1] - Lanes[0].EdgeParts[0]).normalized;
            }

            public List<Vector3> sidewalk { get; private set; }
            public List<Vector3> sidewalkMonodromos { get; private set; }

            public bool IsMonodromos
            {
                get { return isMonodromos; }

                set { isMonodromos = value; }
            }

            public List<MovingEntity> MovingEntitiesOnEdge
            {
                get { return movingEntitiesOnEdge; }

                set { movingEntitiesOnEdge = value; }
            }

            /// <summary>
            /// Sidewalks should use both in and out lines to determine the point.
            /// This is a bad solution. 
            /// </summary>
            /// <param name="width"></param>
            public void GenerateSideWalkBad(float width)
            {
                sidewalk = new List<Vector3>();
                List<Vector3> parts = lanes[0].EdgeParts;
                Vector2 perpendicular;
                Vector3 direction;

                perpendicular = Vector2.Perpendicular((parts[1] - parts[0]).normalized.XZ()).normalized;
                sidewalk.Add(parts[0] + new Vector3(perpendicular.x, 0, perpendicular.y) * (width / 2f));

                for (int i = 1; i < parts.Count; i++)
                {
                    direction = parts[i] - parts[i - 1];
                    sidewalk.Add(sidewalk[i - 1] + direction);
                }
            }

            public void GenerateSidewalk(float width)
            {

                sidewalk = new List<Vector3>();
                List<Vector3> parts = lanes[0].EdgeParts;
                sidewalk.Add(FindPoint1(parts[0], parts[1] - parts[0], width));

                for (int i = 1; i < parts.Count - 1; i++)
                {
                    sidewalk.Add(FindPoint2(parts[i], parts[i] - parts[i - 1], parts[i + 1] - parts[i], width));
                }

                if (parts.Count == 1)
                    return;
                sidewalk.Add(FindPoint1(parts[parts.Count - 1], parts[parts.Count - 1] - parts[parts.Count - 2],
                    width));

                if (isMonodromos && lanes.Count > 1)
                {
                    sidewalkMonodromos = new List<Vector3>();

                    parts = lanes[1].EdgeParts;
                    parts.Reverse();
                    sidewalkMonodromos.Add(FindPoint1(parts[0], parts[1] - parts[0], width));

                    for (int i = 1; i < parts.Count - 1; i++)
                    {

                        sidewalkMonodromos.Add(FindPoint2(parts[i], parts[i] - parts[i - 1], parts[i + 1] - parts[i],
                            width));
                    }

                    if (parts.Count == 1)
                        return;
                    sidewalkMonodromos.Add(FindPoint1(parts[parts.Count - 1],
                        parts[parts.Count - 1] - parts[parts.Count - 2], width));
                    parts.Reverse();
                }
                else if (isMonodromos && lanes.Count == 1)
                {
                    sidewalkMonodromos = new List<Vector3>();

                    parts = lanes[0].EdgeParts;
                    parts.Reverse();
                    sidewalkMonodromos.Add(FindPoint1(parts[0], parts[1] - parts[0], width));

                    for (int i = 1; i < parts.Count - 1; i++)
                    {

                        sidewalkMonodromos.Add(FindPoint2(parts[i], parts[i] - parts[i - 1], parts[i + 1] - parts[i],
                            width));
                    }

                    if (parts.Count == 1)
                        return;
                    sidewalkMonodromos.Add(FindPoint1(parts[parts.Count - 1],
                        parts[parts.Count - 1] - parts[parts.Count - 2], width));
                    parts.Reverse();
                }
            }

            /// <summary>
            /// This works only for points that have only in or out line, not both.
            /// </summary>
            public Vector3 FindPoint1(Vector3 relativePoint, Vector3 direction, float width)
            {
                Vector2 perpendicular = Vector2.Perpendicular(direction.normalized.XZ()).normalized;
                return relativePoint + new Vector3(perpendicular.x, 0, perpendicular.y) * width / 2f;
            }

            /// <summary>
            /// This works only for points with both in and out lines.
            /// </summary>
            public Vector3 FindPoint2(Vector3 relativePoint, Vector3 directionIn, Vector3 directionOut, float width)
            {
                Vector2 perpendicular1 = Vector2.Perpendicular(directionIn.normalized.XZ()).normalized;
                Vector2 perpendicular2 = Vector2.Perpendicular(directionOut.normalized.XZ()).normalized;
                Vector2 perpendicular = (perpendicular1 + perpendicular2).normalized;
                return relativePoint + new Vector3(perpendicular.x, 0, perpendicular.y) * width / 2f;
            }



            /// <summary>
            /// This function should provide direction of movemnt. For now the direction is going to be 
            /// pre calculated.
            /// If this road has two sidewalks the direction should be reversed for the monodromos sidewalk.
            /// The agent should have the information about the sidewalk he approaches.
            /// Halting this function.Maybe it is inefficient to use this one.
            /// </summary>
            private void GenerateMovingDirectionForAgent()
            {

            }

            private Edge opposite;

            public Edge GetOppositeEdge()
            {
                if (opposite != null)
                    return opposite;
                foreach (Edge edge in EndNode.OutEdges)
                {
                    if (StartNode == edge.EndNode)
                    {
                        opposite = edge;
                        return opposite;
                    }
                }

                return null;
            }
        }


        private Graph graph;
        private KdTree<float, Node> jTree = new KdTree<float, Node>(2, new FloatMath());

        public Graph Graph1
        {
            get { return graph; }
        }

        public KdTree<float, Node> JTree
        {
            get { return jTree; }

            set { jTree = value; }
        }

        public GameObject cubePrefab;
        public List<GameObject> cubes = new List<GameObject>();

        /// <summary>
        /// Not used currently.
        /// </summary>
        private bool saveGraph = false;

        // Use this for initialization
        void Start()
        {
            StartCoroutine(InitializeGraph());
            //graph = new Graph();
            //graph.PopulateGraphFromXml("LeftNicosia.net");
            //graph.PopulateJunctionTree(Jtree);
            //foreach(Node junction in graph.Nodes)
            //{
            //    cubes.Add(Instantiate(cubePrefab, junction.Position, new Quaternion()));
            //    cubes[cubes.Count - 1].name = junction.Id;
            //    cubes[cubes.Count - 1].GetComponent<NodeHolder>().MYNODEREEE = junction;
            //}
            //graph.CreateRoadsWithPlanes(quad);
            //graph.CreateWholeRoad(fullroad);
        }

        IEnumerator InitializeGraph()
        {
            //graph=GetComponent<Serializer>().DeSerializeObject<Graph>( "Traffic.xml");

            if (graph == null)
            {
                graph = new Graph();
                //graph.PopulateGraphFromXml("test.net");
                StartCoroutine(graph.InitializeFromMesh());
                yield return new WaitUntil(() => graph.MeshInitializationFinished);
                Debug.Log("Waiting is finitoo");
                graph.PopulateJunctionTree(jTree);
                graph.FindOneWayRoads();
                graph.GenerateSidewalks();
                graph.CreateRoadDataStructures();
            }

            foreach (Node junction in graph.Nodes)
            {
                cubes.Add(Instantiate(cubePrefab, junction.Position, new Quaternion()));
                cubes[cubes.Count - 1].name = junction.Id;
                cubes[cubes.Count - 1].GetComponent<NodeHolder>().MYNODEREEE = junction;
                junction.CreateABoundedArea();
            }
        }

        public GameObject quad;

        public GameObject fullroad;

        // Update is called once per frame
        void Update()
        {
            graph.DrawGraphWithRays();
            graph.VisualizeSidewalksWithRays();
            // SaveGraph();
        }

        /// <summary>
        /// Should not be used anymore
        /// </summary>
        void SaveGraph()
        {
            if (!saveGraph) return;
            GetComponent<Serializer>().SerializeObject<Graph>(graph, "Traffic.xml");
            saveGraph = false;
        }

        //public class Vector2Math : ITypeMath<Vector2> {
        //}
    }
}