using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using KdTree;
using KdTree.Math;

[ExecuteInEditMode]
public class RescaleBuilding : MonoBehaviour {

    public List<IndependentBuilding> buildings = new List<IndependentBuilding>();
    public float scale = 1;

    public bool shouldScale = false;
    public Vector3[] originaVertices;

    Mesh mesh;
    private KdTree<float, IndependentBuilding> buildingTree = new KdTree<float, IndependentBuilding>(2, new FloatMath());
    // Use this for initialization
    void Start() {
        
    }
	
    private int FindBuilding(int indexOfTriangleMember, int indexOfTriangleMember2, int indexOfTriangleMember3,
        Vector3 VectorOfTriangleMember, Vector3 VectorOfTriangleMember2, Vector3 VectorOfTriangleMember3)
    {
        int indexItem = 0;
        foreach (var item in buildings)
        {
            foreach (var item2 in item.indexes)
            {
                if (item2 == indexOfTriangleMember || item2 == indexOfTriangleMember2 || item2 == indexOfTriangleMember3||
                    (item.points[indexItem]-VectorOfTriangleMember).magnitude<0.5f || (item.points[indexItem] - VectorOfTriangleMember2).magnitude < 0.5f ||
                    (item.points[indexItem] - VectorOfTriangleMember3).magnitude < 0.5f
                    )
                {
                    return buildings.IndexOf(item);
                }
                indexItem++;
            }
            indexItem = 0;
        }
        return buildings.Count;///Means not found.
    }

    private int FindBuildingWithKdTree(int indexOfTriangleMember, int indexOfTriangleMember2, int indexOfTriangleMember3,
        Vector3 VectorOfTriangleMember, Vector3 VectorOfTriangleMember2, Vector3 VectorOfTriangleMember3)
    {
        float[] member = new float[2];Vector3 tmpVector=new Vector3();
        KdTreeNode<float, IndependentBuilding>[] node;
        if(buildingTree.Count==0)
            return buildings.Count;
        member[0] = VectorOfTriangleMember.x;member[1] = VectorOfTriangleMember.z;
        node=buildingTree.GetNearestNeighbours(member, 1);
        tmpVector.x = node[0].Point[0];
        tmpVector.z = node[0].Point[1];
        tmpVector.y = VectorOfTriangleMember.y;
        if((VectorOfTriangleMember - tmpVector).magnitude < 0.25f)
        {
            return buildings.IndexOf(node[0].Value);
        }
        member[0] = VectorOfTriangleMember2.x; member[1] = VectorOfTriangleMember2.z;
        node = buildingTree.GetNearestNeighbours(member, 1);
        tmpVector.x = node[0].Point[0];
        tmpVector.z = node[0].Point[1];
        tmpVector.y = VectorOfTriangleMember.y;
        if ((VectorOfTriangleMember - tmpVector).magnitude < 0.25f)
        {
            return buildings.IndexOf(node[0].Value);
        }
        member[0] = VectorOfTriangleMember3.x; member[1] = VectorOfTriangleMember3.z;
        node = buildingTree.GetNearestNeighbours(member, 1);
        tmpVector.x = node[0].Point[0];
        tmpVector.z = node[0].Point[1];
        tmpVector.y = VectorOfTriangleMember.y;
        if ((VectorOfTriangleMember - tmpVector).magnitude < 0.25f)
        {
            return buildings.IndexOf(node[0].Value);
        }
        return buildings.Count;
    }
    public GameObject gameObjectParadigm;
    public void Initialize()
    {
        mesh = GetComponent<MeshFilter>().sharedMesh;
        int index;
        Debug.Log(mesh.triangles.Length);
        if (buildingTree.Count != 0)
            return;
        //Debug.DrawRay(transform.TransformPoint(mesh.vertices[0]), Vector3.up*1000, Color.blue, 500);
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            if ((index = FindBuildingWithKdTree(mesh.triangles[i], mesh.triangles[i + 1], mesh.triangles[i + 2],
                mesh.vertices[mesh.triangles[i]], mesh.vertices[mesh.triangles[i + 1]],
                mesh.vertices[mesh.triangles[i + 2]])) == buildings.Count
                )
            {
                buildings.Add(new IndependentBuilding(mesh, transform));
            }
            buildings[index].AddAndKdTree(mesh.triangles[i], mesh.vertices[mesh.triangles[i]],buildingTree);
            buildings[index].AddAndKdTree(mesh.triangles[i + 1], mesh.vertices[mesh.triangles[i + 1]], buildingTree);
            buildings[index].AddAndKdTree(mesh.triangles[i + 2], mesh.vertices[mesh.triangles[i + 2]], buildingTree);
        }
        originaVertices = mesh.vertices ;
        CheckCombinableMeshes();
        CheckCombinableMeshes();
        CheckCombinableMeshes();

        StartCoroutine(CreateBuildings());
        
    }

    IEnumerator CreateBuildings()
    {
        if (transform.childCount != 0)
            yield break;
        Mesh nMesh;
        GameObject obj;
        foreach (var item in buildings)
        {
            nMesh = new Mesh();
            nMesh.vertices = item.points.ToArray();
            nMesh.triangles = FindTrianglesOfIndependentBuilding(item, mesh);
            obj = Instantiate(gameObjectParadigm);
            obj.transform.parent = transform;
            obj.GetComponent<MeshFilter>().mesh = nMesh;
            yield return new WaitForSeconds(0.02f);
        }
    }

    public int[] FindTrianglesOfIndependentBuilding(IndependentBuilding building,Mesh original)
    {
        int index = -1;
        List<int> triangles = new List<int>();
        for (int i = 0; i < original.triangles.Length; i += 3)
        {
            if ((index = building.indexes.IndexOf(original.triangles[i])) > -1)
            {
                triangles.Add(index);
                triangles.Add(building.indexes.IndexOf(original.triangles[i + 1]));
                triangles.Add(building.indexes.IndexOf(original.triangles[i + 2]));               
            }
        }
        return triangles.ToArray();
 
    }

    public void CheckCombinableMeshes()
    {
        for (int i = 0; i < buildings.Count; i++)
        {
            for (int j = i+1; j < buildings.Count; j++)
            {
                if (IndependentBuilding.SameVertices(buildings[i], buildings[j]))
                {
                    buildings[i].GibData(buildings[j]);
                }
            }
        }
        for (int i = buildings.Count-1; i >=0 ; i--)
        {
            if (buildings[i].indexes.Count == 0)
                buildings.RemoveAt(i);
        }
    }

    public void ResetVertices()
    {
        if (mesh == null)
            return;
        mesh.vertices = originaVertices;
        foreach (var item in buildings)
        {
            for (int i = 0; i < item.indexes.Count; i++)
            {
                item.points[i] = originaVertices[item.indexes[i]];
            }
            item.rotation = Quaternion.LookRotation(Vector3.up);
        }
        foreach (var item in buildings)
        {
            item.FindCenter();
        }
    }

    Vector3[] vertices;

    public KdTree<float, IndependentBuilding> BuildingTree
    {
        get
        {
            return buildingTree;
        }

        set
        {
            buildingTree = value;
        }
    }

    // Update is called once per frame

    private void OnGUI()
    {
        
    }
    void Update () {

        //Debug.Log("Hiiiiiiiiii");
        //if (shouldScale)
        //{
        //    vertices = new Vector3[mesh.vertices.Length];
        //    foreach (var item in buildings)
        //    {
        //        item.Scale(scale, vertices);
        //    }
        //}
        //mesh.vertices = vertices;
        //mesh.RecalculateBounds();
        //mesh.RecalculateNormals();
        //shouldScale = false;
    }

    public void MoveBuilding(IndependentBuilding building,Vector3 change)
    {
        vertices = new Vector3[mesh.vertices.Length];
        foreach (var item in buildings)
        {
            item.Move(new Vector3(), vertices);
        }
        building.Move(change, vertices); 
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();        
    }
    public void RotateBuilding(IndependentBuilding building, Quaternion change)
    {
        //building.rotation = change;
        vertices = new Vector3[mesh.vertices.Length];
        foreach (var item in buildings)
        {
            item.Move(new Vector3(), vertices);
        }
        building.Rotate(change, vertices);
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
    public void ScaleIndependent(IndependentBuilding building,Vector3 scaleFactor)
    {
        vertices = new Vector3[mesh.vertices.Length];
        foreach (var item in buildings)
        {
            item.Move(new Vector3(), vertices);
        }
        building.ScaleIndependently(scaleFactor, vertices);
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    public void Scale()
    {
        if (buildings.Count == 0)
        {
            Initialize();
        }
        Debug.Log("Hiiiiiiiiii");
        vertices = new Vector3[mesh.vertices.Length];
        foreach (var item in buildings)
        {
            item.Scale(scale, vertices);
        }
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        shouldScale = false;
    }


    //public void BuildingGizmos()
    //{
    //    if (buildings.Count == 0)
    //    {
    //        Initialize();
    //    }
        
    //}
}

public class IndependentBuilding {

    public List<Vector3> points = new List<Vector3>();
    public List<int> indexes = new List<int>();
    public Quaternion rotation = Quaternion.LookRotation(Vector3.up);
    public Vector3 currentScale = new Vector3(1, 1, 1);

    public Dictionary<int, Vector3> dictionaryPoints = new Dictionary<int, Vector3>();

    private Vector3 center;
    private Mesh mesh;
    private Transform transform;

    public Vector3 Center
    {
        get
        {
            return center;
        }

        set
        {
            center = value;
        }
    }

    public IndependentBuilding(Mesh mesh,Transform transform)
    {
        this.mesh = mesh;
        this.transform = transform;
    }

    public void FindCenter()
    {
        center = Vector3.zero;
        foreach (var item in points)
        {
            center += item;
        }
        center /= points.Count;
        center.y = transform.position.y;
    }
    Vector3 throwaway;
    public void Add(int index,Vector3 point)
    {

        if (dictionaryPoints.ContainsKey(index))
            return;

        //for (int i = 0; i < indexes.Count; i++)
        //{
        //    if (indexes[i] == index)
        //        return;
        //}
        dictionaryPoints.Add(index, point);
        indexes.Add(index);
        points.Add(point);
        FindCenter();
    }
    private static float[] positia = new float[2];
    public void AddAndKdTree(int index, Vector3 point, KdTree<float, IndependentBuilding> tree)
    {
        if (dictionaryPoints.ContainsKey(index))
            return;

        //for (int i = 0; i < indexes.Count; i++)
        //{
        //    if (indexes[i] == index)
        //        return;
        //}
        positia = new float[2];
        positia[0] = point.x;
        positia[1] = point.z;
        dictionaryPoints.Add(index, point);
        tree.Add(positia,this);
        indexes.Add(index);
        points.Add(point);
        FindCenter();
    }

    public void Scale(float scal,Vector3[] vertices)
    {
        Vector3 direction = Vector3.zero;
        for (int i = 0; i < points.Count; i++)
        {

            direction = points[i] - center;
            points[i] = center + direction * scal;
            vertices[indexes[i]] = points[i];
        }
    }
    public void ScaleIndependently(Vector3 scale, Vector3[] vertices)
    {
        Vector3 direction = Vector3.zero;
        for (int i = 0; i < points.Count; i++)
        {

            direction = points[i] - center;
            //direction = direction * currentScale;// direction / currentScale;

            points[i] = new Vector3(center.x + direction.x * scale.x,
                center.y + direction.y * scale.y, center.z + direction.z * scale.z);
            vertices[indexes[i]] = points[i];
        }  
    }
    public void Move(Vector3 change,Vector3 [] vertices)
    {
        for (int i = 0; i < points.Count; i++)
        {
            points[i] += change;
            vertices[indexes[i]] = points[i];
        }
        FindCenter();
    }
    public void Rotate(Quaternion change, Vector3[] vertices)
    {
        Vector3 initialDir;
        for (int i = 0; i < points.Count; i++)
        {
            initialDir = points[i] - center;
            initialDir = Quaternion.Inverse(rotation) * initialDir;
            initialDir = change * initialDir;
            points[i] = center + initialDir;
            vertices[indexes[i]] = points[i];

        }

        FindCenter();
        rotation = change;
    }

    public static bool SameVertices(IndependentBuilding build1, IndependentBuilding build2)
    {
        foreach (var item in build1.indexes)
        {
            if (build2.dictionaryPoints.ContainsKey(item))
                return true;
            //if()
            //foreach (var item2 in build2.indexes)
            //{
            //    if (item == item2)
            //        return true;
            //}
        }
        //foreach (var item in build1.points)
        //{
        //    foreach (var item1 in build2.points)
        //    {
        //        if ((item - item1).magnitude < 0.75f)
        //        {
        //            return true;
        //        } 
        //    }
        //}
        return false;
    }
    public void GibData(IndependentBuilding from)
    {
        for (int i = 0; i < from.indexes.Count; i++)
        {
            if (!dictionaryPoints.ContainsKey(from.indexes[i]))
            {
                indexes.Add(from.indexes[i]);
                points.Add(from.points[i]);
                dictionaryPoints.Add(from.indexes[i], from.points[i]);
            }         
        }
        from.indexes.Clear();
        from.points.Clear();
        from.dictionaryPoints.Clear();
    }
}