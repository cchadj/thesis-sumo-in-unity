using RiseProject.Tomis.SumoInUnity.SumoTypes;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class HighlightRoute : FunctionalScriptableObjectWithNetworkDataAccess<string>
{
    public override void Run()
    {
        throw new System.NotImplementedException();
    }

    private UnityEngine.Color _startcolor;
    private bool _isRouteHighlighted = false;
    public override void Run(string routeID)
    {
        List<string> edgeIDs = SumoCommands.RouteCommands.GetEdges(routeID).Content;
        foreach(string id in edgeIDs)
        {
            List<Lane> lanes = SumoNetworkData.Edges[id].Lanes;
            foreach (Lane lane in lanes)
            {
                List<GameObject> laneGameObjects = TransformNetworkData.LaneIDGameObjectPairs[lane.ID];
                int objectCount = laneGameObjects.Count;
                int c = 0;
                foreach ( GameObject lanego in laneGameObjects)
                {
                    float normalized = (float)c++/objectCount;
                    MeshRenderer renderer =  lanego.GetComponent<MeshRenderer>();
                    if (!_isRouteHighlighted)
                        _startcolor = renderer.material.color;
                    renderer.material.color = _isRouteHighlighted ? _startcolor : UnityEngine.Color.Lerp(_startcolor, UnityEngine.Color.yellow, normalized);
                }
            }
        }
        _isRouteHighlighted = !_isRouteHighlighted;
    }
}
