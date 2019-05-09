using UnityEngine.EventSystems;
using UnityEngine;
using RiseProject.Tomis.DataContainers;

public class RouteDetailMouseEvents : MonoBehaviour, IPointerClickHandler {

    [SerializeField]
    public HighlightRoute OnPointerClickRunableScript;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnPointerClickRunableScript.Run(GetComponent<RouteData>().routeID);
    }
}
