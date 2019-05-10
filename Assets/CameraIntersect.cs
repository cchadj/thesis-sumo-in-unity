using System.Collections;
using UnityEngine;

public class CameraIntersect : MonoBehaviour
{
    [SerializeField] private float horizontalPlaneHeight = 0f;
    [SerializeField] private bool debug;
    
    private Camera _cam;
    private float _farClipPlane;
    private float _nearClipPlane;
    
    public  readonly Vector3[] HitPoints = new Vector3[4];
    private int _numberOfPointsHit;
    
    
    // Cache for performance
    private readonly Vector3[] _frustumCorners = new Vector3[4];
    private Transform _transform;
    private Plane _horizontalPlane;
    private static readonly Rect Rect = new Rect(0f, 0f, 1f, 1f);
    

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        _farClipPlane = _cam.farClipPlane;
        _nearClipPlane = _cam.nearClipPlane;
        _horizontalPlane = new Plane(Vector3.up, Vector3.zero); 
        _transform = transform;

        enabled = debug;
    }

    private void Update()
    {
        FindIntersectionsWithPlane(out var _);
    }

    /// <summary>
    /// Finds 0 to 4 corners that intersect the horizontal plane at <see cref="horizontalPlaneHeight"/>
    /// </summary>
    /// <param name="planeHitPoints"> The hit points </param>
    /// <returns> The number of rays hit </returns>
    public int FindIntersectionsWithPlane(out Vector3[] planeHitPoints)
    {
        _cam.CalculateFrustumCorners(Rect, _cam.farClipPlane, Camera.MonoOrStereoscopicEye.Mono, _frustumCorners );

        var numHitPoints = 0;

        for (int i = 0; i < 4; i++)
        {
            var worldSpaceCorner = _cam.transform.TransformVector(_frustumCorners[i]);
            var ray = new Ray(_transform.position, worldSpaceCorner);

            if (_horizontalPlane.Raycast(ray, out float dist))
            {
                HitPoints[numHitPoints++] = ray.GetPoint(dist);
            }
            
            #if UNITY_EDITOR
            Debug.DrawRay(_transform.position, worldSpaceCorner, Color.blue, 3f);
            #endif
            
            planeHitPoints = HitPoints;
        }
        planeHitPoints = HitPoints;
        _numberOfPointsHit = numHitPoints;
        return numHitPoints;
    }
    
    //Debugging 
    private static  readonly Vector3 CubeSize = Vector3.one * 4;
    private void OnDrawGizmos()
    {
        for (int i = 0; i < _numberOfPointsHit; i++)
        {
            Gizmos.DrawCube(HitPoints[i], CubeSize );
            
        }
        
    }
}
