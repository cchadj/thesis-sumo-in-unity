using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;

public class CarRayCaster : MonoBehaviour {
    private const float RAY_LENGTH = 90f;
    private const int ROAD_NETWORK_LAYER_MASK = 1 << 9;

    [Tooltip("A place holder used to assist casting ray at the front of the car"), SerializeField]
    /// <summary>
    /// A place holder used to assist casting ray at the front of the car
    /// </summary>
    private Transform _vehicleFrontPlaceholder;

    [Tooltip("A place holder used to assist casting ray at the back of the car"), SerializeField]
    /// <summary>
    /// A place holder used to assist casting ray at the back of the car
    /// </summary>
    private Transform _vehicleBackPlaceholder;

    private Vector3 _frontPosition;
    private Vector3 _backPosition;
    private Ray _carFrontRay;
    private Ray _carBackRay;
    public bool FrontRayDidHit { get; private set; }
    public bool BackRayDidHit { get; private set; }

    public Transform VehicleBackPlaceholder { get => _vehicleBackPlaceholder; set => _vehicleBackPlaceholder = value; }
    public Transform VehicleFrontPlaceholder { get => _vehicleFrontPlaceholder; set => _vehicleFrontPlaceholder = value; }
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
        _frontPosition = VehicleFrontPlaceholder.position;
        _backPosition = VehicleBackPlaceholder.position;
    }

    void FixedUpdate () {
        _frontPosition = VehicleFrontPlaceholder.position;
        _backPosition = VehicleFrontPlaceholder.position;

        Vector3 curPosition = transform.position;
        CarFrontRay = new Ray(_frontPosition - Vector3.up * RAY_LENGTH/2 , Vector3.up );
        CarBackRay = new Ray(_backPosition - Vector3.up * RAY_LENGTH / 2, Vector3.up);

        FrontRayDidHit = Physics.Raycast(CarFrontRay, out FrontRayHitInfo, RAY_LENGTH, ROAD_NETWORK_LAYER_MASK);
        BackRayDidHit = Physics.Raycast(CarBackRay, out BackRayHitInfo, RAY_LENGTH, ROAD_NETWORK_LAYER_MASK);

#if UNITY_EDITOR
        Debug.DrawRay(CarFrontRay.origin, CarFrontRay.direction * RAY_LENGTH, Color.red);
        Debug.DrawRay(CarBackRay.origin, CarBackRay.direction * RAY_LENGTH, Color.red);
#endif
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawRay(CarFrontRay.origin, CarFrontRay.direction * RAY_LENGTH);
        Gizmos.DrawRay(CarBackRay.origin, CarBackRay.direction * RAY_LENGTH);
    }
#endif
}
