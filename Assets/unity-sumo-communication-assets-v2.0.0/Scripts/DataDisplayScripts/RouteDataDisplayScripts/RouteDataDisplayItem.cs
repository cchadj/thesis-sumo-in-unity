using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RouteDataDisplayItem : MonoBehaviour
{
    [SerializeField]
    private Text _routeID;
    [SerializeField]
    private Text _templateEdgesIDText;

    public Text RouteIDText
    {
        get
        {
            return _routeID;
        }
        private set
        {
            _routeID = value;
        }
    }
    public Text TemplateEdgesIDText
    {
        get
        {
            return _templateEdgesIDText;
        }
        private set
        {
            _templateEdgesIDText = value;
        }
    }
}
