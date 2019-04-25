using System.Collections.Generic;
using UnityEngine;

public class PolygonMesher : MonoBehaviour
{
    /*
     * Create a GameObject with mesh that lays on the x, z plane based on the
     * vertices given.
     */
    public static GameObject CreateShapedObject(List<Vector2> vertices2D)
    {
        GameObject newGameObj = new GameObject();

        if (vertices2D == null)
        {
            return null;
        }
        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(vertices2D.ToArray());
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices. Vertices lay on the x,z plane
        Vector3[] vertices = new Vector3[vertices2D.Count];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices2D[i].x, 0.01f, vertices2D[i].y);
        }

        // Create the mesh
        Mesh newMesh = new Mesh
        {
            vertices = vertices,
            triangles = indices
        };
        newMesh.RecalculateNormals();
        newMesh.RecalculateBounds();
        //InvertNormals(newMesh);

        // Set up game object with mesh;
        newGameObj.AddComponent<MeshRenderer>();
        MeshFilter filter = newGameObj.AddComponent<MeshFilter>();
        filter.mesh = newMesh;
        return newGameObj;
    }

    public static void InvertNormals(Mesh mesh)
    {
        if (mesh != null)
        {
            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++)
                normals[i] = -normals[i];
            mesh.normals = normals;

            for (int m = 0; m < mesh.subMeshCount; m++)
            {
                int[] triangles = mesh.GetTriangles(m);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int temp = triangles[i + 0];
                    triangles[i + 0] = triangles[i + 1];
                    triangles[i + 1] = temp;
                }
                mesh.SetTriangles(triangles, m);
            }
        }
    }
}