
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityQuery;

public class MinimumEnclosingCircle : MonoBehaviour
{

    [UnityEngine.Header("For Debugging")]
    [SerializeField] private bool debug = true;
    [ReadOnly] public Vector3 center;
    [ReadOnly] public float radius;
    [ReadOnly] public List<Transform> trans;

    // For Debuggin 
    private CameraIntersect _cameraIntersect;
    private Vector3[] hitPoints = new Vector3[4];
    
    [HideInInspector] public int numberOfIntersectingPoints;
    private void Awake()
    {
        if (debug)
        {
            var r = new Random();
            for (var i = 0; i < 4; i++)
            {
                var t = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                t.name = "circlepoint" + 1;
                trans.Add(t);
                t.position = Vector3.zero +
                             new Vector3(Random.Range(-1f, 2f), Random.Range(-1f, 2f), Random.Range(-1f, 2f));
                t.localScale = Vector3.one * 4f;
            }
            _cameraIntersect = GetComponent<CameraIntersect>();
            enabled = true;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        numberOfIntersectingPoints = _cameraIntersect.FindIntersectionsWithPlane(out hitPoints);
        if (numberOfIntersectingPoints == 4)
        {
            for (int i = 0; i < 4; i++)
            {
                trans[i].position = hitPoints[i];
            }

            var c = SmallestEnclosingCircle.MakeUnityCircleFourPointsNonAlloc(trans.Select(x => x.position).ToArray());
            center = new Vector3(c.c.x, 0f, c.c.y);
            radius = c.r;

        }
    }

    public static float MinimumEnclosingCircleFourPoints(Vector3[] points, out Vector3 centre)
    {
        var minX = points[0].x;
        var maxX = points[0].x;
        var minZ = points[0].z;
        var maxZ = points[0].z;
       
        var greatestDistance = 0f;
        var min = points[0];
        var max = points[0];

        for (int i = 1; i < 4; i++)
        {
            var p = points[i];
            if (p.x > maxX)
            {
                maxX = p.x;
            }

            if (p.x < minX)
            {
                minX = p.x;
            }

            if (p.z > maxZ)
            {
                maxZ = p.z;
            }
            if (p.z < minZ)
            {
                minZ = p.z;
            }
            
        }

        min.x = minX;
        min.z = minZ;
        
        max.x = maxX;
        max.z = maxZ;
        
        Debug.DrawLine(min, max);
        centre = Vector3.Lerp(min, max, 0.5f);
        return 0.5f * Mathf.Sqrt( (minX - maxX) * (minX - maxX) + (minZ - maxZ) * (minZ - maxZ));
    }
}



#if UNITY_EDITOR
 
[CustomEditor(typeof(MinimumEnclosingCircle))]
public class MinimumEnclosingCircleEditor: Editor 
{
     
    private MinimumEnclosingCircle c;

    public void OnSceneGUI () 
    {       
        c = target as MinimumEnclosingCircle;

        if(!Application.isPlaying) return;
        if(c.numberOfIntersectingPoints != 4) return;
        
        Handles.color = Color.red;
        Handles.DrawWireDisc(c.center, Vector3.up, c.radius); // radius
    }
}
#endif