
using System;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class MeshHeightMatcher : MonoBehaviour {

    /// <summary>
    /// HeightBuffer is some Vector3 locations cached in order 
    /// to be used and match the lane height with the closest point.
    /// </summary>
    private List<Vector3> _heightBuffer;

    /// <summary>
    /// Used to get the center of the mesh
    /// </summary>
    // private Renderer _renderer;

    /// <summary>
    /// The center of the mesh of the lane.
    /// </summary>
    private Vector3 _meshCenter;

    void Start()
    {
        //_renderer = GetComponent<Renderer>();
    }

    /// <summary>
    /// Sets the height of the lane so that it matches the height the closest in xz plane Vector3.
    /// </summary>
    public void MatchHeight()
    {
        throw new NotImplementedException();

        //float minDistance = Mathf.Infinity;
        //Vector2 curLocation = new Vector2(_renderer.bounds.center.x, _renderer.bounds.center.z);

        //Vector3 closest = new Vector3();
        //foreach (Vector3 v in Toolbox.Instance.HeightBuffer)
        //{
        //    Vector2 otherLocation = new Vector2(v.x, v.z);
        //    float dist = Vector2.Distance(curLocation, otherLocation);
        //    if (dist < minDistance)
        //    {
        //        minDistance = dist;
        //        closest = v;
        //    }
        //}
        ///* Change only the height of the lane to that of the closest point. */
        //transform.position = new Vector3(transform.position.x, closest.y, transform.position.z);
    }
}
