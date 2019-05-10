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
public class LaneContextSubscription : MonoBehaviour
{
    [CallSetter("RefreshSubscriptionEvery"), SerializeField] private float subscribeEverySeconds;

    public float RefreshSubscriptionEvery
    {
        get => subscribeEverySeconds;
        set
        {
            subscribeEverySeconds = value;
            WaitForSeconds = new WaitForSeconds(subscribeEverySeconds);
        }

    }
        
     
    // Debug 
    [Header("For Debug")]
    [SerializeField, ReadOnly] private Lane closestLane;
    [SerializeField, ReadOnly] private string currentlySubscribedLaneID;
    [SerializeField, ReadOnly] private SimulationState simulationState;
    [SerializeField, ReadOnly] private float theContextRange;
    
    // Dependencies
    private Camera _cam;
    private SumoNetworkData _networkData;
    private SumoCommands _sumoCommands;
    private CameraIntersect _cameraIntersect;
    private SumoClient _sumoClient;

    
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
        SumoCommands sumoCommands, SimulationState simState)
    {
        _sumoClient = sumoClient;   
        _networkData = sumoNetworkData;
        _sumoCommands = sumoCommands;
        simulationState = simState;
    }

    private void Start()
    {
                
        _cam = GetComponent<Camera>();

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

        StartCoroutine(SubscriptionCoroutine());
    }


    private IEnumerator SubscriptionCoroutine()
    {
        
        while (true)
        {
            Assert.AreEqual(_sumoClient.SubscriptionType, SubscriptionType.Context);
            
            ContextSubscribeToLaneInFrustum();
            yield return WaitForSeconds;

        }
    }

    /// <summary>
    /// Find and subscribe to the lane that is closest to the center of the frustum.
    /// The center of the frustum is calculated at Start.
    /// The test happens in the xz plane.
    /// </summary>
    private void ContextSubscribeToLaneInFrustum()
    {
        var numOfIntersectionsWithPlane = _cameraIntersect.FindIntersectionsWithPlane(out _hitPoints);
        
        var positionToGetClosestLaneFrom = Vector3.one;
        float contextRange = 0f;
        
        
        //if frustum intersects with the xz plane then we know the optimal context range and the optimal centre
        if (numOfIntersectionsWithPlane == 4)
        {
            // Calculate the minimum enclosing circle of the four intersection points
            var enclosingCircle = SmallestEnclosingCircle.MakeUnityCircle(_hitPoints);

            positionToGetClosestLaneFrom = new Vector3(enclosingCircle.c.x, 0f, enclosingCircle.c.y);
            contextRange = enclosingCircle.r;
        }
        else
        {
            positionToGetClosestLaneFrom = centerOfFrustum.position;
            contextRange = _cam.farClipPlane / 2f;
        }
        theContextRange = contextRange;

        
        // Get all the lanes that are inside the camera frustum.
        var lanesInsideFrustum = _networkData.RequestForVisibleLanes();
        
        if (lanesInsideFrustum.Count == 0)
            return;
       
        
        // Find the closest lane from the position To Get Closest Lane From 
        closestLane = null;
        var closestDistanceSqr = Mathf.Infinity;
        foreach (var lane in lanesInsideFrustum)
        {
            var distX = lane.centerOfMass.x - positionToGetClosestLaneFrom.x;
            var distZ = lane.centerOfMass.z - positionToGetClosestLaneFrom.z;
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
        if (closestLane.ID == currentlySubscribedLaneID)
            return;
        if (closestDistanceSqr < 10f)
            return;
        
        
        // Subscribe to new lane and unsubscribe from old one
        _sumoCommands.LaneCommands.SubscribeContext(closestLane.ID, 0f, 1000f, Vehicle.ContextDomain, contextRange, ListOfVariablesToSubscribeTo);
        _sumoCommands.LaneCommands.UnsubscribeContext(currentlySubscribedLaneID, TraCIConstants.CMD_GET_VEHICLE_VARIABLE);
        
        
        // Update currently subscribed lane       
        currentlySubscribedLaneID = closestLane.ID;
        simulationState.currentContextSubscribedLaneID = currentlySubscribedLaneID;
        simulationState.currentContextSubscribedLane = closestLane;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && closestLane)
        {
            GUI.color = Color.green;
            UnityEditor.Handles.DrawWireDisc(closestLane.centerOfMass, Vector3.up, theContextRange);    
        }
    }
#endif
    
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