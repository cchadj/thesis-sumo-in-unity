using System.Collections;
using System.Reflection;
using RiseProject.Tomis.DataHolders;
using RiseProject.Tomis.SumoInUnity.SumoTypes;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

public class LaneInCameraFrustum : MonoBehaviour
{

    public Lane Lane;
    private bool _isInsideLanesInsideFrustum = false;
    [SerializeField] private SumoNetworkData networkData;
    private Renderer _renderer;


    [Inject]
    private void Construct(SumoNetworkData sumoNetworkData)
    {
        networkData = sumoNetworkData;
    }
    
    /// <summary>
    /// Replace the mesh of this lane object with a very small mesh that
    /// Does not cast or receive shadows, Does not respond to light and reflection probes
    /// and is fliped so it is not rendered by the camera. When this mesh is inside the camera
    /// frustum then 
    /// </summary>
    private void Start()
    {
        if (Random.value > 0.33f)
        {
            enabled = false;
            GameObject.Destroy(gameObject);
            
        }
         
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

        GetComponent<MeshFilter>().mesh = mesh;
        StartCoroutine(VisibilityTest());
    }

    private static readonly WaitForSeconds  _waitForSeconds = new WaitForSeconds(1f);
    private static int i;

    [ReadOnly] public string ID;

    private IEnumerator VisibilityTest()
    {
        while (true)
        {
            if (_renderer.isVisible)
            {
                Debug.Log(Lane.ID + "I am visible");
                if (!networkData.LanesInsideFrustum.ContainsKey(ID))
                {
                 
                    networkData.LanesInsideFrustum[ID] = Lane;
                    _isInsideLanesInsideFrustum = true;
                }
            }

            if (!_renderer.isVisible && _isInsideLanesInsideFrustum)
            {
                networkData.LanesInsideFrustum.Remove(ID);
                _isInsideLanesInsideFrustum = false;
            }

            yield return _waitForSeconds;
            
        }
    }
        
//    private void OnWillRenderObject()
//    {
//        Debug.Log(Lane.ID  + " Lane is rendered");
//        if (!networkData.LanesInsideFrustum.ContainsKey(ID))
//        {
//            
//            networkData.LanesInsideFrustum[ID] = Lane;
//            _isInsideLanesInsideFrustum = true;
//
//        }
//       
//    }

    public void Call()
    {
        Debug.Log("Sup babe? my ID : " + ID);
    }
}

public interface ICallable
{
    string ID { get;  }
    void Call();
    
}