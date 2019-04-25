using System;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class HeightBufferCreator : MonoBehaviour
{

    //private Toolbox toolbox;
    //private MeshFilter _meshFilter;
    //private Mesh _mesh; 
    // Use this for initialization
    void Start()
    {
        // toolbox = Toolbox.Instance;
        //_meshFilter = GetComponent<MeshFilter>();
        //_mesh = _meshFilter.sharedMesh;
    }

    public void FillHeightBuffer()
    {
        throw new NotImplementedException();
        //Vector3[] vertices = _mesh.vertices;
        //foreach (Vector3 vertice in vertices)
        //{

        //    Vector3 worldPt = transform.TransformPoint(vertice);
        //    //Toolbox.Instance.HeightBuffer.Add(worldPt);
        //}
        //return;
    }
}

