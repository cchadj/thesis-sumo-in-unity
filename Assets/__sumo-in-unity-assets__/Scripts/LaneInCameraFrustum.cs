﻿using System.Collections;
using System.Reflection;
using RiseProject.Tomis.DataContainers;
using RiseProject.Tomis.SumoInUnity.SumoTypes;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

public class LaneInCameraFrustum : MonoBehaviour
{
    [ReadOnly] public Lane Lane;
    [ReadOnly] public string ID;

    
    private SumoNetworkData networkData;
    private bool _isInsideLanesInsideFrustum = false;
    private Renderer _renderer;

    
    // Cache WaitForSeconds for performance
    private static readonly WaitForSeconds WaitForSeconds = new WaitForSeconds(2f);
    
    
    [Inject]
    private void Construct(SumoNetworkData sumoNetworkData)
    {
        networkData = sumoNetworkData;
    }


    private void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        networkData.RequestVisibleLanes += RequestVisibleLanes_AddLaneToVisibleLanes;
    }
    
    /// <summary>
    /// Replace the mesh of this lane object with a very small mesh that
    /// Does not cast or receive shadows, Does not respond to light and reflection probes
    /// and is fliped so it is not rendered by the camera. When this mesh is inside the camera
    /// frustum then 
    /// </summary>
    public void ReplaceMesh()
    {
        _renderer = GetComponent<MeshRenderer>();
        _renderer.lightProbeUsage = LightProbeUsage.Off;
        _renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        _renderer.shadowCastingMode = ShadowCastingMode.Off;
        _renderer.receiveShadows = false;
        
        var newVertices = new[]
        {
            Lane.centerOfMass + new Vector3(0, 0, 0f),
            Lane.centerOfMass + new Vector3(0, 0, 1f),  
            Lane.centerOfMass + new Vector3(1f, 0, 0f)
        };
        
        var mesh = new Mesh
        {
            vertices = newVertices, 
            triangles = new []{ 2, 1, 0},
        };
        GetComponent<MeshFilter>().sharedMesh = mesh;


        
      // DEPRECATED_WAY:  StartCoroutine(VisibilityTest());
    }

    private void OnDestroy()
    {
        networkData.RequestVisibleLanes -= RequestVisibleLanes_AddLaneToVisibleLanes;
    }

    // Adds self to visible lanes if visible. Happens whenever someones request for visible lanes.
    private void RequestVisibleLanes_AddLaneToVisibleLanes(object sender, LanesInsideFrustumEventArgs e)
    {
        if (_renderer.isVisible)
        {
            e.LanesInsideFrustum.Add(Lane);
        }
    }
    
//    
//    private IEnumerator VisibilityTest()
//    {
//        while (true)
//        {
//            if (_renderer.isVisible)
//            {
//                if (!networkData.LanesInsideFrustum.ContainsKey(ID))
//                {
//                 
//                    networkData.LanesInsideFrustum[ID] = Lane;
//                    _isInsideLanesInsideFrustum = true;
//                }
//            }
//
//            if (!_renderer.isVisible && _isInsideLanesInsideFrustum)
//            {
//                networkData.LanesInsideFrustum.Remove(ID);
//                _isInsideLanesInsideFrustum = false;
//            }
//           
//            yield return WaitForSeconds;
//            
//        }
//    }
//    
//    //Debug
//    private Vector3 _vectorOne = Vector3.one;
//    private void OnDrawGizmos()
//    {
//        if (Application.isPlaying && networkData.showLanesInsideFrustum)
//        {
//            if(_isInsideLanesInsideFrustum)
//                Gizmos.DrawCube(transform.GetChild(0).position, _vectorOne);
//        }
//    }
}
public interface ICallable
{
    string ID { get;  }
    void Call();
    
}