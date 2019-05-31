using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;

public class CarRayCaster : MonoBehaviour
{
    [SerializeField] private float rayLength = 20f;
    [Tooltip("A place holder used to cast a ray at the front of the car"), SerializeField]
    private Transform vehicleFrontPlaceholder;
    [Tooltip("A place holder used to cast a ray at the back of the car"), SerializeField]
    private Transform vehicleBackPlaceholder;
    
    [Header("Debuggin")]
    [SerializeField] private bool showSingleRayCast;
    
    private const int RoadNetworkLayerMask = 1 << 9;

    private Vector3 _frontPosition;
    private Vector3 _backPosition;

    public bool FrontRayDidHit { get; private set; }
    public bool BackRayDidHit { get; private set; }

    /// <summary>
    /// Ray that is cast in the center of the hood of the car 
    /// </summary>
    public Ray CarFrontRay { get; set; }

    /// <summary>
    /// Ray that is cast in the center of the trunk of the car 
    /// </summary>
    public Ray CarRearRay { get; set; }

    public RaycastHit frontRayHitInfo;
    public RaycastHit backRayHitInfo;


    // Use this for initialization
    private void Start()
    {
        _frontPosition = vehicleFrontPlaceholder.position;
        _backPosition = vehicleBackPlaceholder.position;
    }

    private void Update () {
        _frontPosition = vehicleFrontPlaceholder.position;
        _backPosition = vehicleBackPlaceholder.position;

        Vector3 curPosition = transform.position;
        CarFrontRay = new Ray(_frontPosition - Vector3.up * rayLength/2 , Vector3.up );
        CarRearRay = new Ray(_backPosition - Vector3.up * rayLength / 2, Vector3.up);

        FrontRayDidHit = Physics.Raycast(CarFrontRay, out frontRayHitInfo, rayLength, RoadNetworkLayerMask);
        BackRayDidHit = Physics.Raycast(CarRearRay, out backRayHitInfo, rayLength, RoadNetworkLayerMask);

#if UNITY_EDITOR
        if (showSingleRayCast)
        {
            Debug.DrawRay(Vector3.Lerp(CarFrontRay.origin, CarRearRay.origin, 0.5f ), CarFrontRay.direction * rayLength, Color.red);
        }
        else
        {
            Debug.DrawRay(CarFrontRay.origin, CarFrontRay.direction * rayLength, Color.red);
            Debug.DrawRay(CarRearRay.origin, CarRearRay.direction * rayLength, Color.red);     
        }
       
#endif
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawRay(CarFrontRay.origin, CarFrontRay.direction * rayLength);
        Gizmos.DrawRay(CarRearRay.origin, CarRearRay.direction * rayLength);
    }
#endif
}
