
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace TheTide.utils
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    public class SerializeMesh : MonoBehaviour
    {
        [HideInInspector] [SerializeField] Vector2[] uv;
        [HideInInspector] [SerializeField] Vector3[] verticies;
        [HideInInspector] [SerializeField] int[] triangles;
        [SerializeField] public bool serialized = false;
        // Use this for initialization

        void Awake()
        {
            if (serialized)
            {
                GetComponent<MeshFilter>().mesh = Rebuild();
            }
        }

        void Start()
        {
            if (serialized) return;

            Serialize();
        }

        public void Serialize()
        {
            Debug.Log("I am being serialized");
            var mesh = GetComponent<MeshFilter>().mesh;

            uv = mesh.uv;
            verticies = mesh.vertices;
            triangles = mesh.triangles;

            serialized = true;
        }

        public Mesh Rebuild()
        {
            Debug.Log("Rebuilding");
            Mesh mesh = new Mesh();
            mesh.vertices = verticies;
            mesh.triangles = triangles;
            mesh.uv = uv;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            GetComponent<MeshFilter>().mesh = mesh;
            return mesh;
        }
    }

#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SerializeMesh))]
    class SerializeMeshEditor : Editor
    {
        SerializeMesh obj;
 
        void OnSceneGUI()
        {
            obj = (SerializeMesh)target;
        }
 
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
 
            if (GUILayout.Button("Rebuild"))
            {
                if (obj)
                {
                    obj.gameObject.GetComponent<MeshFilter>().mesh = obj.Rebuild();
                }
            }
 
            if (GUILayout.Button("Serialize"))
            {
                if (obj)
                {
                   obj.Serialize();
                }
            }
        }
    }
#endif
}