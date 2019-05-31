using System;
using System.Collections;
using System.Collections.Generic;
using RiseProject.Tomis.DataContainers;
using UnityEngine;
using Zenject;
using System.Reflection;
using CodingConnected.TraCI.NET;

using RiseProject.Tomis.SumoInUnity;
using RiseProject.Tomis.SumoInUnity.SumoTypes;
using UnityEngine.Assertions;
using Debug = UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Camera), typeof(CameraIntersect))]
public class CameraContextSubscription : MonoBehaviour
{
    [CallSetter("RefreshSubscriptionEvery"), SerializeField] private float subscribeEverySeconds;
    [SerializeField] private  float vehicleContextSubscriptionRange;
    
    public float RefreshSubscriptionEvery
    {
        get => subscribeEverySeconds;
        set
        {
            subscribeEverySeconds = value;
            WaitForSeconds = new WaitForSeconds(subscribeEverySeconds);
        }

    }

    private Matrix4x4 _roadLocalToWorldMatrix;
    
    // Debug 
    [Header("--- Debug ---")]
    [SerializeField, ReadOnly] private SimulationState simulationState;  
    [SerializeField, ReadOnly] private Transform roadNetwork;
    [SerializeField, ReadOnly] private float roadHeight;
    [SerializeField, ReadOnly] private string currentlySubscribedVehicleID;
    [SerializeField, ReadOnly] private ContextSubscriptionState subscriptionState = ContextSubscriptionState.NoSubscription;
    
    [Space(3)]
    [Header("Subscription Circle")]
    [SerializeField] private Color subscriptionCircleColor = Color.red;
    [SerializeField, ReadOnly] private Lane closestLane;
    [SerializeField, ReadOnly] private Vehicle subscribedVehicle;
    [SerializeField, ReadOnly] private string currentlySubscribedLaneID;
    [SerializeField, ReadOnly, Tooltip( "The Current actual Subscription Range that")] 
    private float curContextRange;

    [Header("Enclosing Circle")]
    [SerializeField] private Color enclosingCircleColor = Color.white;
    [SerializeField, ReadOnly] private Vector3 _positionToGetClosestLaneFrom = Vector3.one;
    [SerializeField, ReadOnly, Tooltip( "The new subscription range that will be used for the next subscritpion " )] 
    private float newContextRange;

    
    // Dependencies
    private Camera _cam;
    private SumoNetworkData _networkData;
    private SumoCommands _sumoCommands;
    private CameraIntersect _cameraIntersect;
    private SumoClient _sumoClient;
    private CurrentlySelectedTargets _selectedTargets;

    
    // Cache
    private Transform centerOfFrustum;
    private static readonly List<byte> ListOfVariablesToSubscribeTo = new List<byte>
        {TraCIConstants.VAR_POSITION, TraCIConstants.VAR_ANGLE};
    private static WaitForSeconds WaitForSeconds = new WaitForSeconds(2f); // cache to reduce gc time
    private Vector3[] _hitPoints = new Vector3[4]; // The intersection points of the camera frustum with the xz plane

    [Inject]
    private void Construct(
        SumoClient sumoClient,
        SumoNetworkData sumoNetworkData, 
        SumoCommands sumoCommands, 
        SimulationState simState,
        CurrentlySelectedTargets selectedTargets
        )
    {
        _sumoClient = sumoClient;   
        _networkData = sumoNetworkData;
        _sumoCommands = sumoCommands;
        simulationState = simState;
        _selectedTargets = selectedTargets;
    }

    private void Start()
    {
        if (!roadNetwork)
        {
            var go = GameObject.FindWithTag("GeneratedRoadNetwork");
            if (go)
                roadNetwork = go.transform; 
        }

        roadHeight = roadNetwork.transform.position.y;
        _roadLocalToWorldMatrix = roadNetwork.transform.localToWorldMatrix;
        
        _cam = GetComponent<Camera>();
        _cam.transform.localScale = Vector3.one;
        
        _cameraIntersect = GetComponent<CameraIntersect>();
        _cameraIntersect.maxDist = _cam.farClipPlane;
        
        if (_sumoClient.SubscriptionType != SubscriptionType.Context)
        {
            Destroy(this);
            _cameraIntersect.enabled = false;
            Destroy(_cameraIntersect);
            enabled = false;
            return;
        }


        transform.localScale.Set(1f, 1f, 1f);
        
        // We need to keep track of the center of the frustum when the camera frustum doesn't intersect xz plane
        var frustumCenterLocalPosition = new Vector3(0, 0, _cam.farClipPlane/2f);
        centerOfFrustum = new GameObject().transform;
        centerOfFrustum.gameObject.SetIcon(IconManager.Icon.CircleGreen);
        centerOfFrustum.parent = transform;
        centerOfFrustum.localPosition = frustumCenterLocalPosition;
        centerOfFrustum.name = "Center Of Camera Frustum";
        
        _selectedTargets.VehicleSelected += SelectedTargets_OnVehicleSelected;
        _selectedTargets.VehicleDeselected += SelectedTargets_OnVehicleDeselected;

        StartCoroutine(LaneSubscriptionCoroutine());
    }
    

    private void UnsubscribeFromPreviouslySubscribedEgoObject()
    {
        if(subscriptionState == ContextSubscriptionState.LaneSubscription)
            _sumoCommands.LaneCommands.UnsubscribeContext(currentlySubscribedLaneID, Vehicle.ContextDomain);

        if (subscriptionState == ContextSubscriptionState.VehicleSubscription)
        {
            _sumoCommands.VehicleCommands.UnsubscribeContext(currentlySubscribedVehicleID, Vehicle.ContextDomain);

        }
    }

    
    private void SelectedTargets_OnVehicleDeselected(object sender, EventArgs e)
    {
        StopAllCoroutines();
        
        UnsubscribeFromPreviouslySubscribedEgoObject();

        StartCoroutine(LaneSubscriptionCoroutine());
    }
    
    
    private void SelectedTargets_OnVehicleSelected(object sender, SelectedVehicleEventArgs e)
    {
        StopAllCoroutines();
        
        UnsubscribeFromPreviouslySubscribedEgoObject();
        
        StartCoroutine(VehicleSubscriptionCoroutine(e.SelectedVehicle));
    }

    private IEnumerator VehicleSubscriptionCoroutine(Vehicle vehicle)
    {
        
        while (true)
        {
            Assert.AreEqual(_sumoClient.SubscriptionType, SubscriptionType.Context);
            
            ContextSubscribeToVehicle(vehicle);
            yield return WaitForSeconds;
        }
    }


    private IEnumerator LaneSubscriptionCoroutine()
    {
        
        while (true)
        {
            Assert.AreEqual(_sumoClient.SubscriptionType, SubscriptionType.Context);
            
            ContextSubscribeToLaneInFrustum();
            yield return WaitForSeconds;
        }
    }

    private void ContextSubscribeToVehicle(Vehicle vehicle)
    {
        if(vehicle.ID != currentlySubscribedVehicleID)
            UnsubscribeFromPreviouslySubscribedEgoObject();
        
        var contextRange = vehicleContextSubscriptionRange;

        _sumoCommands.VehicleCommands.SubscribeContext(
            vehicle.ID, 
            0f, 
            1000f,
            Vehicle.ContextDomain,
            contextRange,
            ListOfVariablesToSubscribeTo);
        
        curContextRange = contextRange;
        
        
        subscriptionState = ContextSubscriptionState.VehicleSubscription;
        subscribedVehicle = vehicle;
        currentlySubscribedVehicleID = vehicle.ID;
        
        
        simulationState.subscriptionState = subscriptionState;
        simulationState.currentContextSubscribedObjectID = currentlySubscribedVehicleID;
        simulationState.currentContextSubcribedTraCIVariable = vehicle;
    }
    
    
    /// <summary>
    /// Find and subscribe to the lane that is closest to the center of the frustum.
    /// The center of the frustum is calculated at Start.
    /// The test happens in the xz plane.
    /// </summary>
    private void ContextSubscribeToLaneInFrustum()
    {
        var numOfIntersectionsWithPlane = _cameraIntersect.FindIntersectionsWithPlane(out _hitPoints);
        var contextRange = 0f;
        
        
        //if frustum intersects with the xz plane then we know the optimal context range and the optimal centre
        if (numOfIntersectionsWithPlane == 4)
        {
            // Calculate the minimum enclosing circle of the four intersection points
            var enclosingCircle = SmallestEnclosingCircle.MakeUnityCircle(_hitPoints);

            _positionToGetClosestLaneFrom = new Vector3(enclosingCircle.c.x, 0f, enclosingCircle.c.y);
            contextRange = enclosingCircle.r;
        }
        else
        {
            _positionToGetClosestLaneFrom = centerOfFrustum.position;
            contextRange = _cam.farClipPlane / 2f;
        }
        newContextRange = contextRange;

        
        // Get all the lanes that are inside the camera frustum.
        var lanesInsideFrustum = _networkData.RequestForVisibleLanes();
        
        if (lanesInsideFrustum.Count == 0)
            return;
       
        
        // Find the closest lane from the position To Get Closest Lane From 
        closestLane = null;
        var closestDistanceSqr = Mathf.Infinity;
        foreach (var lane in lanesInsideFrustum)
        {
            var lanePosition = _roadLocalToWorldMatrix.MultiplyPoint3x4(lane.centerOfMass);
            var distX = lanePosition.x - _positionToGetClosestLaneFrom.x;
            var distZ = lanePosition.z - _positionToGetClosestLaneFrom.z;
            var dSqrToTarget = distX * distX + distZ * distZ;
         
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                closestLane = lane;
            }
        }
        
        
        // Assert because if a lane is visible and no closest lane found then something is wrong
        UnityEngine.Debug.Assert(closestLane != null, nameof(closestLane) + " != null");

        
        // No need to subscribe to a a new lane if it close or the old one
//        if (closestLane.ID == currentlySubscribedLaneID)
//        {
//            if(Mathf.Abs(newContextRange - curContextRange) < 10f)
//                return;
//        }


//        if (closestDistanceSqr < 10f)
//        {
//            if(Mathf.Abs(newContextRange - curContextRange) < 10f)
//                return;
//        }

        _sumoCommands.LaneCommands.UnsubscribeContext(currentlySubscribedLaneID, TraCIConstants.CMD_GET_VEHICLE_VARIABLE);
        _sumoCommands.LaneCommands.SubscribeContext(closestLane.ID, 0f, 1000f, Vehicle.ContextDomain, contextRange, ListOfVariablesToSubscribeTo);
        
        curContextRange = newContextRange;
        
        // Update currently subscribed lane
        subscriptionState = ContextSubscriptionState.LaneSubscription;
        currentlySubscribedLaneID = closestLane.ID;

        simulationState.subscriptionState = subscriptionState;
        simulationState.currentContextSubscribedObjectID = currentlySubscribedLaneID;
        simulationState.currentContextSubcribedTraCIVariable = closestLane;
    }
    

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        switch (subscriptionState)
        {
            case ContextSubscriptionState.VehicleSubscription:
                Handles.color = subscriptionCircleColor;
                Handles.DrawWireDisc(_roadLocalToWorldMatrix.MultiplyPoint3x4(subscribedVehicle.Position), Vector3.up, curContextRange);
                break;
            case ContextSubscriptionState.LaneSubscription:
                Handles.color = enclosingCircleColor;
                Handles.DrawWireDisc(_positionToGetClosestLaneFrom + new Vector3(0f, roadNetwork.transform.position.y, 0f), Vector3.up, newContextRange);   
                Handles.color = subscriptionCircleColor;
                Handles.DrawWireDisc(_roadLocalToWorldMatrix.MultiplyPoint3x4(closestLane.centerOfMass), Vector3.up, curContextRange);
                break;
            case ContextSubscriptionState.NoSubscription:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
#endif
    
}


public enum ContextSubscriptionState
{
    NoSubscription,
    VehicleSubscription,
    LaneSubscription
}


public class IconManager {

    public enum LabelIcon {
        None = -1,
        Gray,
        Blue,
        Teal,
        Green,
        Yellow,
        Orange,
        Red,
        Purple
    }

    public enum Icon {
        None = -1,
        CircleGray,
        CircleBlue,
        CircleTeal,
        CircleGreen,
        CircleYellow,
        CircleOrange,
        CircleRed,
        CirclePurple,
        DiamondGray,
        DiamondBlue,
        DiamondTeal,
        DiamondGreen,
        DiamondYellow,
        DiamondOrange,
        DiamondRed,
        DiamondPurple
    }

    private static GUIContent[] labelIcons;
    private static GUIContent[] largeIcons;

    public static void SetIcon( GameObject gObj, LabelIcon icon ) {
        if ( labelIcons == null ) {
            labelIcons = GetTextures( "sv_label_", string.Empty, 0, 8 );
        }

        if ( icon == LabelIcon.None )
            RemoveIcon( gObj );
        else
            Internal_SetIcon( gObj, labelIcons[(int)icon].image as Texture2D );
    }

    public static void SetIcon( GameObject gObj, Icon icon ) {
        if ( largeIcons == null ) {
            largeIcons = GetTextures( "sv_icon_dot", "_pix16_gizmo", 0, 16 );
        }

        if ( icon == Icon.None )
            RemoveIcon( gObj );
        else
            Internal_SetIcon( gObj, largeIcons[(int)icon].image as Texture2D );
    }

    public static void SetIcon( GameObject gObj, Texture2D texture ) {
        Internal_SetIcon( gObj, texture );
    }

    public static void RemoveIcon( GameObject gObj ) {
        Internal_SetIcon( gObj, null );
    }

    private static void Internal_SetIcon( GameObject gObj, Texture2D texture ) {
#if UNITY_EDITOR
        var ty = typeof( EditorGUIUtility );
        var mi = ty.GetMethod( "SetIconForObject", BindingFlags.NonPublic | BindingFlags.Static );
        mi.Invoke( null, new object[] { gObj, texture } );
#endif
    }

    private static GUIContent[] GetTextures( string baseName, string postFix, int startIndex, int count ) {
        GUIContent[] guiContentArray = new GUIContent[count];
#if UNITY_EDITOR
#if UNITY_5_3_OR_NEWER
        for ( int index = 0; index < count; index++ ) {
            guiContentArray[index] = EditorGUIUtility.IconContent( baseName + ( startIndex + index ) + postFix );
        }
#else
        var t = typeof( EditorGUIUtility );
        var mi = t.GetMethod( "IconContent", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof( string ) }, null );

        for ( int index = 0; index < count; ++index ) {
            guiContentArray[index] = mi.Invoke( null, new object[] { baseName + (object)( startIndex + index ) + postFix } ) as GUIContent;
        }
#endif
#endif
        return guiContentArray;
    }
}

public static class IconManagerExtension {

    public static void SetIcon( this GameObject gObj, IconManager.LabelIcon icon ) {
        if ( icon == IconManager.LabelIcon.None )
            IconManager.RemoveIcon( gObj );
        else
            IconManager.SetIcon( gObj, icon );
    }

    public static void SetIcon( this GameObject gObj, IconManager.Icon icon ) {
        if ( icon == IconManager.Icon.None )
            IconManager.RemoveIcon( gObj );
        else
            IconManager.SetIcon( gObj, icon );
    }

    public static void SetIcon( this GameObject gObj, Texture2D texture ) {
        IconManager.SetIcon( gObj, texture );
    }

    public static void RemoveIcon( this GameObject gObj ) {
        IconManager.RemoveIcon( gObj );
    }
}