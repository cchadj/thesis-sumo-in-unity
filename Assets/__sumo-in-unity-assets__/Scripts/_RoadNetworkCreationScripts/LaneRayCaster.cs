using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class LaneRayCaster : MonoBehaviour
{

    private const float RAY_LENGTH = 100f;
    /// <summary>
    /// Antreas Network Layer 
    /// </summary>
    private const int ROAD_NETWORK_LAYER_MASK = 1 << 10;

    public Vector3 LaneFront;
    public Vector3 LaneBack;


    /// <summary>
    /// Ray that is cast in the center of the hood of the car 
    /// </summary>
    private Ray LaneFrontRay;

    /// <summary>
    /// Ray that is cast in the center of the trunk of the car 
    /// </summary>
    private Ray LaneBackRay;


    /// <summary>
    /// Ray that is cast in the center of the trunk of the car 
    /// </summary>
    private Ray LaneCenterRay;
    //private Ray TestRay;
    //TestRay = new Ray(LaneFront - Vector3.up* RAY_LENGTH / 2, Vector3.up* RAY_LENGTH);
    public bool FrontRayDidHit { get; private set; }
    public bool BackRayDidHit { get; private set; }

    public RaycastHit FrontRayHitInfo;
    public RaycastHit BackRayHitInfo;

    private Renderer _renderer;
    private Vector3 _meshCenter;
    

    private Vector3 _edgeDirection;
    void Start()
    {
        _renderer = GetComponent<Renderer> ();
        _meshCenter = _renderer.bounds.center;
        _edgeDirection = LaneBack - LaneFront;


        Vector3 curPosition = transform.position;
        LaneFrontRay = new Ray(LaneFront - Vector3.up * RAY_LENGTH / 2, Vector3.up);
        LaneCenterRay = new Ray(_meshCenter - Vector3.up * RAY_LENGTH / 2, Vector3.up);
        LaneBackRay = new Ray(LaneBack - Vector3.up * RAY_LENGTH / 2, Vector3.up);

        FrontRayDidHit = Physics.Raycast(LaneFrontRay, out FrontRayHitInfo, RAY_LENGTH, ROAD_NETWORK_LAYER_MASK);
        BackRayDidHit = Physics.Raycast(LaneBackRay, out BackRayHitInfo, RAY_LENGTH, ROAD_NETWORK_LAYER_MASK);
    }
     
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(LaneFrontRay.origin, LaneFrontRay.direction * RAY_LENGTH);
        Gizmos.DrawRay(LaneBackRay.origin, LaneBackRay.direction * RAY_LENGTH);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(LaneCenterRay.origin, LaneCenterRay.direction * RAY_LENGTH);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(LaneBack, _edgeDirection);
    }


}