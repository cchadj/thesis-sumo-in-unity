using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;

public class CarRayCaster : MonoBehaviour
{
    private const float RayLength = 20f;
    private const int RoadNetworkLayerMask = 1 << 9;
    [SerializeField] private bool showSingleRayCast;

    [Tooltip("A place holder used to assist casting ray at the front of the car"), SerializeField]
    private Transform vehicleFrontPlaceholder;

    [Tooltip("A place holder used to assist casting ray at the back of the car"), SerializeField]
    private Transform vehicleBackPlaceholder;

    private Vector3 _frontPosition;
    private Vector3 _backPosition;
    private Ray _carFrontRay;
    private Ray _carBackRay;
    public bool FrontRayDidHit { get; private set; }
    public bool BackRayDidHit { get; private set; }

    /// <summary>
    /// Ray that is cast in the center of the hood of the car 
    /// </summary>
    public Ray CarFrontRay { get => _carFrontRay; set => _carFrontRay = value; }
    /// <summary>
    /// Ray that is cast in the center of the trunk of the car 
    /// </summary>
    public Ray CarBackRay { get => _carBackRay; set => _carBackRay = value; }

    public RaycastHit FrontRayHitInfo;
    public RaycastHit BackRayHitInfo;


    // Use this for initialization
    void Start()
    {
        _frontPosition = vehicleFrontPlaceholder.position;
        _backPosition = vehicleBackPlaceholder.position;
    }

    void FixedUpdate () {
        _frontPosition = vehicleFrontPlaceholder.position;
        _backPosition = vehicleBackPlaceholder.position;

        Vector3 curPosition = transform.position;
        CarFrontRay = new Ray(_frontPosition - Vector3.up * RayLength/2 , Vector3.up );
        CarBackRay = new Ray(_backPosition - Vector3.up * RayLength / 2, Vector3.up);

        FrontRayDidHit = Physics.Raycast(CarFrontRay, out FrontRayHitInfo, RayLength, RoadNetworkLayerMask);
        BackRayDidHit = Physics.Raycast(CarBackRay, out BackRayHitInfo, RayLength, RoadNetworkLayerMask);

#if UNITY_EDITOR
        if (showSingleRayCast)
        {
            Debug.DrawRay(Vector3.Lerp(CarFrontRay.origin, CarBackRay.origin, 0.5f ), CarFrontRay.direction * RayLength, Color.red);
        }
        else
        {
            Debug.DrawRay(CarFrontRay.origin, CarFrontRay.direction * RayLength, Color.red);
            Debug.DrawRay(CarBackRay.origin, CarBackRay.direction * RayLength, Color.red);     
        }
       
#endif
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawRay(CarFrontRay.origin, CarFrontRay.direction * RayLength);
        Gizmos.DrawRay(CarBackRay.origin, CarBackRay.direction * RayLength);
    }
#endif
}
