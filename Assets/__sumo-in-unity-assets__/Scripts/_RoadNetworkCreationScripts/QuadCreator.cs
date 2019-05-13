using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadCreator : MonoBehaviour {

    /// <summary>
    /// Creates a quad based on two points. The quad is created along the line that connects 
    /// the two points. The quad created is rendered in the x,z plane.
    /// </summary>
    /// <param name="startPoint"> Start of the line </param>
    /// <param name="endPoint"> The end of the line </param>
    /// <param name="width"> The width the quad will have arround the line </param>
    /// <param name="createNormals"> Create normals or not. False if used only for mesh collider </param>
    /// <returns></returns>
    public static GameObject CreateQuad(Vector2 startPoint, Vector2 endPoint, float width, bool createNormals,  bool addBoxCollider, float minWidth=0f)
    {
        if (Vector2.Distance(startPoint, endPoint) < minWidth)
            return null;
        GameObject go = new GameObject();
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[4];

        Vector2 direction1 = endPoint - startPoint;
        Vector2 newVector1 = Vector3.Cross(direction1, Vector3.forward);
        newVector1.Normalize();
        Vector2 p0 = width/2 * newVector1 + startPoint ;
        Vector2 p1 = -width/2 * newVector1 + startPoint;

        Vector2 p2 =  width / 2 * newVector1 + endPoint;
        Vector2 p3 =  -width / 2 * newVector1 + endPoint;

        vertices[0] = new Vector3(p0.x, 0f, p0.y);
        vertices[1] = new Vector3(p1.x, 0f, p1.y);
        vertices[2] = new Vector3(p2.x, 0f, p2.y);
        vertices[3] = new Vector3(p3.x, 0f, p3.y);

        mesh.vertices = vertices;

        int[] tri = new int[6];

        //  Lower left triangle.
        tri[0] = 1;
        tri[1] = 2;
        tri[2] = 0;

        //  Upper right triangle.   
        tri[3] = 1;
        tri[4] = 3;
        tri[5] = 2;

        mesh.triangles = tri;
        if (createNormals)
        {
            Vector3[] normals = new Vector3[4];

            normals[0] = new Vector3(0, 1, 0);
            normals[1] = new Vector3(0, 1, 0);
            normals[2] = new Vector3(0, 1, 0);
            normals[3] = new Vector3(0, 1, 0);

            mesh.normals = normals;
        }
        else
            mesh.normals = null;


        Vector2[] uv = new Vector2[4];

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 1);
        uv[3] = new Vector2(1, 1);

        mesh.uv = uv;
        go.AddComponent<MeshRenderer>();

        (go.AddComponent<MeshFilter>()).sharedMesh = mesh;

        if (addBoxCollider)
        {
            Vector3 startPoint3d = new Vector3(startPoint.x, 0f, startPoint.y);
            Vector3 endPoint3d = new Vector3(endPoint.x, 0f, endPoint.y);
            Vector3 edgeDirection = (endPoint3d - startPoint3d).normalized;
            Renderer edgeRenderer = go.GetComponent<Renderer>();
            Mesh edgeMesh = (go.GetComponent<MeshFilter>().sharedMesh);
            GameObject edgeBoxCollider = new GameObject()
            {
                name = "boxcollider"

            };
            edgeBoxCollider.transform.tag = "boxcollider";
            edgeBoxCollider.transform.parent = go.transform;
            edgeBoxCollider.transform.rotation = Quaternion.Euler(Vector3.zero);
            edgeBoxCollider.transform.localRotation = Quaternion.Euler(Vector3.zero);
            edgeBoxCollider.layer = go.layer;

            BoxCollider col = edgeBoxCollider.AddComponent<BoxCollider>();
            col.size = new Vector3(1f, 1f, 1f);
            edgeBoxCollider.transform.position = edgeRenderer.bounds.center;


            edgeBoxCollider.transform.rotation = Quaternion.LookRotation(edgeDirection);
            edgeBoxCollider.transform.localScale = new Vector3(width, 1f, Vector3.Distance(startPoint3d, endPoint3d));
            // Quaternion.FromToRotation(edgeBoxCollider.transform.right, laneDirection);
        }
        return go;
    }

    /// <summary>
    /// Creates a quad based on two points. The quad is created along the line that connects 
    /// the two points. If the distance bettween startPoint and endPoint is bigger than lengthThreshold
    /// then it creates more quads recursively. The quads created are rendered in the x,z plane.
    /// </summary>
    /// <param name="startPoint"> Start of the line </param>
    /// <param name="endPoint"> The end of the line </param>
    /// <param name="width"> The width the quad will have arround the line </param>
    /// <param name="createNormals"> Create normals or not. False if used only for mesh collider </param>
    /// <param name="lengthThreshold">
    ///     If quad has more length than that then it creates two quads with smaller length recursively.
    ///     If lengthThreshold is 0 or less then no segmentation occurs.
    /// </param>
    /// 
    /// <returns> A list of created quads </returns>
    public static List<GameObject> CreateQuad(Vector2 startPoint, Vector2 endPoint, float width, bool createNormals=false, bool addBoxCollider=true, float lengthThreshold=Mathf.Infinity, float minimumLaneLength=0f)
    {
        if (lengthThreshold <= 0)
            lengthThreshold = 10000f;
        List<GameObject> createdQuads = new List<GameObject>();

        var dist = Vector2.Distance(startPoint, endPoint);
        if( dist > lengthThreshold && dist/2f > minimumLaneLength)
        {
            Vector2 halfPoint = Vector2.Lerp(startPoint, endPoint, 0.5f);
            createdQuads.AddRange(CreateQuad( startPoint, halfPoint,  width,  createNormals, addBoxCollider, lengthThreshold, minimumLaneLength));
            createdQuads.AddRange(CreateQuad(halfPoint, endPoint, width, createNormals, addBoxCollider, lengthThreshold ,minimumLaneLength));
        }
        else
        {
            createdQuads.Add(CreateQuad(startPoint, endPoint, width, createNormals, addBoxCollider, minimumLaneLength));
        }
        
        return createdQuads;
    }
}
