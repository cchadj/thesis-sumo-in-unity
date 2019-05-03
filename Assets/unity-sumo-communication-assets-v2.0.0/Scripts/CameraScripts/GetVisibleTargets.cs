using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(Camera))]
public class GetVisibleTargets : MonoBehaviour
{
    private const int EXPECTED_MAXIMUM_NUMBER_OF_COLLIDERS_IN_FRUSTUM = 50;
    

    [SerializeField] private Camera cam;
    [SerializeField]private float _radius;
    
    [SerializeField] private bool box = true;
    [SerializeField] private LayerMask layerMask;

    [SerializeField] private float refreshEverySeconds;

    // Cache Wait for seconds for performance
    private WaitForSeconds _waitForSeconds;
    
    private float frustumCenterDistance;
    private Vector3 _localFrustumCenter;
    
    
    private Vector3 _boxSize;
    public readonly Collider[] CollidedObjects = new Collider[EXPECTED_MAXIMUM_NUMBER_OF_COLLIDERS_IN_FRUSTUM];
    public int size;

    public event EventHandler<VisibleTargetEventArgs> NewTargetsVisible;
    
    
    private void Awake()
    {
        // Make sure that the local scale is Vector3.One because we want to get localFrustumCenter in worldspace
        // and by using TransformPoint we need the scale to be 1 to find the exact center.
        cam.transform.localScale.Set(1f, 1f, 1f);
        _waitForSeconds = new WaitForSeconds(refreshEverySeconds);
        frustumCenterDistance = cam.farClipPlane / 2;
        _localFrustumCenter = new Vector3(0f,0, cam.farClipPlane/2);
        
        _radius = cam.farClipPlane;
        
        _boxSize = Vector3.one * _radius;
        StartCoroutine(Scan());
    }


    private static readonly WaitForSeconds _waitForSmallAmountOfTime = new WaitForSeconds(1f);
    private IEnumerator Scan()
    {
        while (true)
        {
            if (!cam.transform.hasChanged) yield return _waitForSmallAmountOfTime;

            cam.transform.hasChanged = false;

            yield return _waitForSeconds;

            var frustumCenter = cam.transform.TransformPoint(_localFrustumCenter);
            size = box
                ? GetObjectsInsideBox(CollidedObjects, frustumCenter)
                : GetObjectsInsideSphere(CollidedObjects, frustumCenter);

            NewTargetsVisible?.Invoke(this, new VisibleTargetEventArgs(CollidedObjects, size, frustumCenter));
            Debug.Log(message: "Number of objects retrieved" + size);
            yield return _waitForSmallAmountOfTime;
        }
    }

    private int GetObjectsInsideSphere(Collider[] collidedObjs, Vector3 sphereCenter)
    {
        return Physics.OverlapSphereNonAlloc(sphereCenter, _radius, collidedObjs, layerMask,
            QueryTriggerInteraction.UseGlobal);
    }

    private int GetObjectsInsideBox(Collider[] collidedObjs, Vector3 boxCenter)
    {
        return Physics.OverlapBoxNonAlloc(boxCenter, _boxSize, collidedObjs, Quaternion.identity, layerMask,
            QueryTriggerInteraction.UseGlobal);
    }

    private void OnDrawGizmos()
    {
        _localFrustumCenter = new Vector3(0f, 0f, frustumCenterDistance);
        _boxSize = Vector3.one * _radius;

        var frustumCenter = cam.transform.TransformPoint(_localFrustumCenter);

        // Draw Wirecube center  
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(frustumCenter, 3f);
        if (box)
            Gizmos.DrawWireCube(frustumCenter, _boxSize * 2);
        else
        {
            Gizmos.DrawWireSphere(frustumCenter, _radius);
        }
    }
}

public class VisibleTargetEventArgs : EventArgs
{
    public readonly Collider[] CollidedObjects;
    public readonly int Size;
    public readonly Vector3 FrustumCenter;

    public VisibleTargetEventArgs(Collider[] collidedObjects, int size, Vector3 frustumCenter)
    {
        CollidedObjects = collidedObjects;
        Size = size;
        FrustumCenter = frustumCenter;
    }
}
