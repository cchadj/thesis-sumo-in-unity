﻿using System;
using System.Xml.Linq;
using System.Globalization;
using System.Collections.Generic;

using Tomis.UnityEditor.Utilities;
using cakeslice;

using RiseProject.Tomis.SumoInUnity.SumoTypes;
using RiseProject.Tomis.Util.Serializable;
using RiseProject.Tomis.DataHolders;
using RiseProject.Tomis.SumoInUnity.MVC;
using UnityEngine;

[ExecuteInEditMode]
public class RoadNetworkRenderer : MonoBehaviour
{
    [Header("Rendered Road Network Settings")]
    [Tooltip("What name do you want the generated mesh to be")]
    public string meshName;
    [FileSelect(
        OpenAtPath = @"/StreamingAssets/sumo-scenarios/",
        AssetRelativePath = true,
        SelectMode =  SelectionMode.File,
        ButtonName = "Select Sumo Net File",
        FileExtensions = "net.xml",
        Tooltip = "Select .net.xml file to create road network from")]

    public string sumoNetXmlFile;
    public Material roadMaterial;
    public Material junctionMaterial;
    public Material junctionTrafficLightMaterial;
    [Tooltip(" Each quad that has length more than Length Threshold is segmented recursively to smaller quads.\n" +
             " if Length Threshold is <=0 then no segmentation occurs. ")]
    public float lengthThreshold;
    public float roadWidth = 4f;

    private CurrentlySelectedTargets CurrentlySelectedTargets { get; set; }
    private TransformNetworkData TransformNetworkData { get; set; }
    private SumoNetworkData NetworkData { get; set; }
    
    [field: SerializeField, Tooltip("These events get raised whenever a lane is clicked."),  Rename("On Click Lane Events")]
    private List<GameEvent> OnClickLaneEvents { get; set; }

    [field: Space(8), Header("UI Related"), SerializeField, Rename("Make Clickable")]
    private bool MakeClickable { get;}

    //[Tooltip("Disable Normals for optimisation")]
    /* Optimization tip: If a Mesh is used only by a Mesh Collider, you can disable Normals
    * in Import Settings, because the physics system doesn’t need them.
    */
    private bool _disableNormals = false;
    [Tooltip("Adds box collider. If unchecked adds convex mesh collider")]
    public bool addBoxCollider = true;

    public Canvas laneCanvas;

    [Space(8), Header("Height Match Settings")]

    [Tooltip("Attaches a script used to match height with Andreas Roads.")] public bool attachHeightMatcher = true;
    [Tooltip("List of game object used to fill height buffer")] public List<GameObject> heightBufferObjects;

    [Space(8), Header("Debug Settings")]

    [Tooltip("Draw Normals for debug")] public bool drawNormals = true;
    [Tooltip("Red rays show the two edges of the mesh. Blue is the center of the mesh. Green shows direction of the mesh")]
    public bool showDebugRays;

    private int _laneCount;
    private string _netXmlFile;
    private readonly Dictionary<string, Junction> _junctionDict;
    private GameObject _generatedRoadNetwork;
    private GameObject _generatedLanes;
    private GameObject _generatedJunctions;
    private Dictionary<Junction.JunctionType, GameObject> _gameObjectParentsByJunctionType = new Dictionary<Junction.JunctionType, GameObject>();

    private int _junctionCount;
    private const int GENERATED_NETWORK_LAYER = 9;

    void Update()
    {
        AddHeightBufferScript();
    }

    /// <summary>
    /// Adds HeightBufferCreator script to all HeightBufferObjects that have a mesh.
    /// Also add it to all it's children. Each object can have this script only once.
    /// </summary>
    private void AddHeightBufferScript()
    {
        if (heightBufferObjects != null)
            foreach (var o in heightBufferObjects)
            {
                if (o == null)
                {
                    continue;
                }

                if (o.GetComponent<MeshRenderer>() != null)
                {
                    o.AddComponent<HeightBufferCreator>();
                }

                Transform[] allChildren = o.GetComponentsInChildren<Transform>();
                foreach (Transform c in allChildren)
                {
                    if (c.GetComponent<MeshRenderer>() != null && c.gameObject.GetComponent<HeightBufferCreator>() == null)
                        c.gameObject.AddComponent<HeightBufferCreator>();
                }
            }
    }


    /// <summary>
    /// Creates the road network in X,Z plane in unity based on this .net.xml file
    /// </summary>
    /// <returns> The created rendered Network </returns>
    public GameObject RenderNetwork()
    {
        NetworkData = SumoNetworkData.Instance;
        TransformNetworkData = TransformNetworkData.Instance;
        CurrentlySelectedTargets = CurrentlySelectedTargets.Instance;
        
        _generatedRoadNetwork = new GameObject()
        {
            name = meshName,
            layer = GENERATED_NETWORK_LAYER,
            tag = "GeneratedRoadNetwork"
        };
        _generatedRoadNetwork.AddComponent<DisplayRenderer>();

        _generatedLanes = new GameObject()
        {
            name = "Lanes",
            layer = GENERATED_NETWORK_LAYER
        };
        _generatedLanes.transform.parent = _generatedRoadNetwork.transform;

        _generatedJunctions = new GameObject()
        {
            name = "Junctions",
            layer = GENERATED_NETWORK_LAYER,
           
        };
        _generatedJunctions.transform.parent = _generatedRoadNetwork.transform;

        Debug.Log("Folder opened: " + sumoNetXmlFile);
        /* We must render the Road*/
        _netXmlFile = sumoNetXmlFile; // Directory.GetFiles(SumoNetXmlFile, "*.net.xml", SearchOption.AllDirectories)[0];
        var lanes = GetLaneDataFromNetXml().ToArray();
        var junctions = GetJunctionDataFromNetXml().ToArray();

        var laneGameObjectDict = new IDtoGameObjectsDictionary();
        /* RENDER LANES */
        foreach (var lane in lanes)
        {
            _laneCount++;
            var curLane = lane;
            laneGameObjectDict.Add(curLane.ID, RenderLane(curLane, roadWidth));
        }

        var junctionGameObjectDict = new IDtoGameObjectDictionary();
        /* RENDER JUNCTIONS */
        foreach (var junction in junctions)
        {
            _junctionCount++;
            var curJunction = junction;
            junctionGameObjectDict.Add(curJunction.ID, RenderShapedVariable(curJunction));
        }

        TransformNetworkData.LaneIDGameObjectPairs = laneGameObjectDict;

        return _generatedRoadNetwork;
    }

    /// <summary>
    /// Render a Lane object that is represented by set of points. Each consecutive two points are connected
    /// via a straight line. The thickness of the lane is not specified in the net.xml file so it needs to be 
    /// provided (as roadWidth).
    /// 
    /// For each consecutive two points a quad will be created from the first point to the second point in the x,z
    /// plane with roadWidth thickness. If a quad is greater than this LengthTheshold then it breaks recursively into
    /// 
    /// 
    /// </summary>
    /// <param name="lane"> The lane to render in unity </param>
    /// <param name="roadWidth"></param>
    /// <returns> List of quads created for the lane </returns>
    private List<GameObject> RenderLane(Lane lane, float roadWidth)
    {
        /* each lane is representated by two points 
         e.g (23.3, 0,23) -------------------------- (31,23)
         */
        List<GameObject> lanesCreated = new List<GameObject>();
        for (int i = 0; i < lane.ShapeVertexPoints.Count - 1; i++)
        {
            List<GameObject> laneObjects = null;
            Vector2 startPoint = (lane.ShapeVertexPoints[i]);
            Vector2 endPoint = lane.ShapeVertexPoints[i + 1];

            Vector3 startPoint3D = new Vector3(startPoint.x, 0f, startPoint.y);
            Vector3 endPoint3D = new Vector3(endPoint.x, 0f, endPoint.y);
            laneObjects = QuadCreator.CreateQuad(startPoint, endPoint, roadWidth, !_disableNormals, addBoxCollider, lengthThreshold);

            int edgeCount = 0;
            foreach (GameObject laneQuad in laneObjects)
            {
                laneQuad.name = lane.GetType().Name + "_id=_" + lane.ID + i + "_quadcount=_" + edgeCount++;
                laneQuad.transform.parent = _generatedLanes.transform;
                laneQuad.layer = GENERATED_NETWORK_LAYER;
                (laneQuad.GetComponent<MeshRenderer>()).material = roadMaterial;
                lanesCreated.Add(laneQuad);

                Transform[] children = laneQuad.transform.GetComponentsInChildren<Transform>();
                foreach (Transform c in children)
                    c.gameObject.layer = laneQuad.layer;

                if (_disableNormals)
                {
                    DestroyImmediate(laneQuad.GetComponent<MeshRenderer>(), false);
                    DestroyImmediate(laneQuad.GetComponent<MeshFilter>(), false);
                }

                if (showDebugRays)
                {
                    LaneRayCaster rayCaster = laneQuad.AddComponent<LaneRayCaster>();
                    rayCaster.LaneFront = new Vector3(startPoint.x, 0f, startPoint.y);
                    rayCaster.LaneBack = new Vector3(endPoint.x, 0f, endPoint.y);
                }

                if (attachHeightMatcher)
                {
                    laneQuad.AddComponent<MeshHeightMatcher>();
                }

                if (drawNormals)
                {
                   laneQuad.AddComponent<DrawNormals>();
                }

                var laneDataScript = laneQuad.AddComponent<LaneData>();
                laneDataScript.TraciVariable = lane;

                var laneController = laneQuad.AddComponent<LaneController>();
                laneController.TraCIVariable = lane;
                
                if (MakeClickable && addBoxCollider)
                {

                    Outline highlightLine = laneQuad.gameObject.AddComponent<Outline>();
                    highlightLine.color = 1;
                    highlightLine.enabled = false;

                    SelectableObjectEvent selectableObjectEvent = laneQuad.gameObject.AddComponent<SelectableObjectEvent>();
                    selectableObjectEvent.Transform = laneQuad.transform.GetChild(0);
                    selectableObjectEvent.OnHoverOutline = highlightLine;
                    selectableObjectEvent.SelectedTargets = CurrentlySelectedTargets;
                }
            }
        }

        return lanesCreated;
    }

    private GameObject RenderShapedVariable<T>(T traciVar) where T : TraCIVariableShaped
    {
        /* Mesh     https://docs.unity3d.com/ScriptReference/Mesh.html  */
        /* Triangle http://wiki.unity3d.com/index.php/Triangulator      */

        GameObject newGameObject = PolygonMesher.CreateShapedObject(traciVar.ShapeVertexPoints);
        newGameObject.name = typeof(T).Name + "_id=_" + traciVar.ID;
        newGameObject.transform.parent = _generatedRoadNetwork.transform;
        
        /* Do not cache MeshRenderer */
        //MeshRenderer renderer = newGameObject.GetComponent<MeshRenderer>();

        if (traciVar is Junction)
        {
            Junction j = (Junction)(object)traciVar;
            Junction.JunctionType junctionType = j.GetJunctionType();

            GameObject junctionParent;
            bool parentExist = _gameObjectParentsByJunctionType.TryGetValue(junctionType,  out junctionParent);
            if (!parentExist || !junctionParent)
            {
                junctionParent = new GameObject()
                {
                    name = j.GetJunctionTypeAsString(),
                    layer = GENERATED_NETWORK_LAYER
                };
                junctionParent.transform.parent = _generatedJunctions.transform;
                _gameObjectParentsByJunctionType.Add(junctionType, junctionParent);
            }

            newGameObject.transform.parent = junctionParent.transform;
            JunctionData jd = newGameObject.AddComponent<JunctionData>();
            jd.TraciVariable = j;
            switch (junctionType)
            {
                case Junction.JunctionType.TrafficLight:
                case Junction.JunctionType.TrafficLightUnregulated:
                case Junction.JunctionType.TrafficLightRightOnRed:
                    newGameObject.GetComponent<MeshRenderer>().material = junctionTrafficLightMaterial;
                    break;
                default:
                    newGameObject.GetComponent<MeshRenderer>().material = junctionMaterial;
                    break;
            }
        }
        else
        {
            newGameObject.GetComponent<MeshRenderer>().material = roadMaterial;
        }
        return newGameObject;
    }

    /// <summary>
    /// Read the this .net.Xml document provided to collect data for the shape of the lanes.
    /// Also collect additional data such as parent edge id.
    /// </summary>
    /// <returns></returns>
    private List<Lane> GetLaneDataFromNetXml()
    {
        List<Lane> lanes = new List<Lane>();
        List<Edge> edges = new List<Edge>();
        XDocument doc = XDocument.Load(_netXmlFile);
        var edgesElements = doc.Root
                          .Elements("edge");
        foreach (XElement edgeElement in edgesElements)
        {
            var id = (string)edgeElement.Attribute("id");
            Edge newEdge = ScriptableObject.CreateInstance<Edge>();
            newEdge.Instantiate(id);
            newEdge.FromJunctionID = (string)edgeElement.Attribute("from");
            newEdge.ToJunctionID = (string)edgeElement.Attribute("to");

            edges.Add(newEdge);
            List<Lane> curEdgeLanes = new List<Lane>();
            /* http://sumo.sourceforge.net/userdoc/Networks/SUMO_Road_Networks.html */
            /* <lane id="<ID>_1" index="1" speed="<SPEED>" length="<LENGTH>" shape="0.00,498.35,2.00 248.50,498.35,3.00"/> */
            var laneElements = edgeElement.Elements("lane");
            foreach (XElement laneElement in laneElements)
            {
                Lane newLane = ScriptableObject.CreateInstance<Lane>();
                newLane.Instantiate((string)laneElement.Attribute("id"));
                newLane.Index = (uint)laneElement.Attribute("index");
                newLane.Speed = (float)laneElement.Attribute("speed");
                newLane.Length = (float)laneElement.Attribute("length");
                newLane.EdgeID = id;
                newLane.Edge = newEdge;

                List<Vector2> vectorPoints = new List<Vector2>();
                string[] shapePositions = ((string)laneElement.Attribute("shape")).Split(null);
                for (int i = 0; i < shapePositions.Length; i++)
                {
                    string[] point = shapePositions[i].Split(',');
                    vectorPoints.Add(new Vector2(
                        float.Parse(point[0], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(point[1], CultureInfo.InvariantCulture.NumberFormat)));
                    newLane.ShapeVertexPoints = vectorPoints;
                    /* UseCultureInfo.InvariantCulture.NumberFormat for . decimal mark */
                }
                lanes.Add(newLane);
                curEdgeLanes.Add(newLane);
            }
            newEdge.Lanes = curEdgeLanes;
        }

        GenerateDictionaries(lanes);
        GenerateDictionaries(edges);
        return lanes;
    }

    private List<Junction> GetJunctionDataFromNetXml()
    {
        /* <junction id="5" type="priority" x="100.00" y="400.00" incLanes="10to5_0 6to5_0 1to5_0" intLanes=":5_0_0 :5_9_0 :5_10_0 :5_3_0 :5_4_0 :5_5_0 :5_6_0 :5_7_0 :5_11_0" shape="104.75,403.25 104.75,396.75 103.25,395.25 96.75,395.25 95.25,396.75 95.25,403.25"> */
        List<Junction> junctions = new List<Junction>();
        XDocument doc = XDocument.Load(_netXmlFile);
        var junctionElements = doc.Root.Elements("junction");

        foreach (XElement junctionElement in junctionElements)
        {
            List<Vector2> vectorPoints = new List<Vector2>();
            string[] shapePositions = ((string)junctionElement.Attribute("shape"))?.Split(null);
            if(shapePositions != null)
                for (int i = 0; i < shapePositions.Length; i++)
                {
                    string[] point = shapePositions[i].Split(',');
                    vectorPoints.Add(new Vector2(
                        float.Parse(point[0], CultureInfo.InvariantCulture.NumberFormat),
                        float.Parse(point[1], CultureInfo.InvariantCulture.NumberFormat)));
                    /* UseCultureInfo.InvariantCulture.NumberFormat for . decimal mark */
                }

            string[] intLanes = ((string)junctionElement.Attribute("intLanes")).Split(null);
            string[] incLanes = ((string)junctionElement.Attribute("incLanes")).Split(null);
            Junction j = ScriptableObject.CreateInstance<Junction>();
            j.Instantiate((string)junctionElement.Attribute("id"));
            j.Raw2DPosition = new Vector2((float)junctionElement.Attribute("x"), (float)junctionElement.Attribute("y"));
            j.ShapeVertexPoints = vectorPoints;
            j.IntLanes = intLanes;
            j.IncLanes = incLanes;

            GenerateDictionaries(junctions);
            junctions.Add(j);
        }

        return junctions;
    }

    /// <summary>
    /// Set this NetworkData scriptable object with lane dictionary.
    /// </summary>
    /// <param name="lanes"></param>
    /// <returns></returns>
    public ScriptableObject GenerateDictionaries(List<Lane> lanes)
    {
        LaneDictionary ld = new LaneDictionary();
        foreach (Lane l in lanes)
            ld.Add(l.ID, l);

        NetworkData.Lanes = ld;
        return NetworkData;
    }

    public ScriptableObject GenerateDictionaries(List<Edge> edges)
    {
        EdgeDictionary ed = new EdgeDictionary();
        foreach (Edge v in edges)
            ed.Add(v.ID, v);

        NetworkData.Edges = ed;
        return NetworkData;
    }

    public ScriptableObject GenerateDictionaries(List<Junction> junctions)
    {
        JunctionDictionary jd = new JunctionDictionary();
        NetworkData.Junctions = jd;
        return NetworkData;
    }

    private List<Route> GetRouteDataFromRouXml()
    {
        throw new NotImplementedException();
    }
}
