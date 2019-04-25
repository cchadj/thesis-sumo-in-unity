using RiseProject.Tomis.DataHolders;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RouteDataDisplay : MonoBehaviour
{
    [SerializeField]
    private SumoCommands _sumoCommands;

    [SerializeField]
    private Transform _templateRouteDetailsDisplay;
    [SerializeField]
    private Transform _showRoutesPanel;

    public SumoCommands SumoCommands
    {
        get
        {
            return _sumoCommands;
        }
        private set
        {
            _sumoCommands = value;
        }
    }
    
    public Transform ShowRoutesPanel
    {
        get
        {
            return _showRoutesPanel;
        }
        private set
        {
            _showRoutesPanel = value;
        }
    }
    public Transform TemplateRouteDetailsDisplay
    {
        get
        {
            return _templateRouteDetailsDisplay;
        }
        set
        {
            _templateRouteDetailsDisplay = value;
        }
    }


    private List<Transform> _instansiatedRouteDetails;
    private List<Transform> InstansiatedRouteDetails
    {
        get
        {
            return _instansiatedRouteDetails;
        }

        set
        {
            _instansiatedRouteDetails = value;
        }
    }

    bool firstUpdate = true;
    void Awake()
    {
        firstUpdate = true;
    }
    // Use this for initialization
    void Start()
    {
        InstansiatedRouteDetails = new List<Transform>();
    }

    void Update()
    {
        if(firstUpdate)
        {
            enabled = false;
            firstUpdate = !firstUpdate;
            return;
        }

        List<string> routIDs = SumoCommands.RouteCommands.GetIdList().Content;
        foreach (string id in routIDs)
        {
            Transform routeDetails = GameObject.Instantiate(TemplateRouteDetailsDisplay);
            routeDetails.gameObject.SetActive(true);

            RouteDataDisplayItem routeDataDisplayItem = routeDetails.GetComponent<RouteDataDisplayItem>();
            routeDataDisplayItem.RouteIDText.text = $"Route ID: {id}";
            routeDataDisplayItem.TemplateEdgesIDText.text = $"Edges ID list: {string.Join(", ", SumoCommands.RouteCommands.GetEdges(id).Content.ToArray())}";
            routeDetails.parent = ShowRoutesPanel;
            InstansiatedRouteDetails.Add(routeDetails);

            RouteData routeData = routeDetails.GetComponent<RouteData>();
            routeData.routeID = id;
        }
    }

    void OnDisable()
    {
        foreach (Transform t in InstansiatedRouteDetails)
            GameObject.Destroy(t);
        InstansiatedRouteDetails.Clear();
    }
}
