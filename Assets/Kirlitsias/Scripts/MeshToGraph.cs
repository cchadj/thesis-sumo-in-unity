using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

//NM_Asphalt_No_Lines_Advanced - Normal dromoi
//-Decal_Road_Border_left(Clone)-Path Left v2 - Path Right v2

//NM_Cobble_Stone_Advanced

namespace RiseProject.Kirlitsias
{
    public class MeshToGraph : MonoBehaviour
    {
        JobFactory factory = new JobFactory();
        Thread[] threads = new Thread[4];
        IExecutor[] jobs = new IExecutor[4];
        private List<Junction> junctions = new List<Junction>();
        private List<TmpEdhe> edges = new List<TmpEdhe>();
        private bool ready = false;

        public List<Junction> Junctions
        {
            get { return junctions; }
        }

        public List<TmpEdhe> Edges
        {
            get { return edges; }
        }

        // Use this for initialization
        IEnumerator Start()
        {
            yield return new WaitForSeconds(5f);
            Debug.Log("Getting all transforms");
            List<Transform> allTransforms = new List<Transform>();
            PopulateJunctions();
            foreach (var item in GameObject.FindGameObjectsWithTag("diak"))
            {
                allTransforms.Add(item.transform);
            }

            List<IExecutor> parallelJobs = factory.TransformsToJobs(allTransforms);
            List<IExecutor> finished = new List<IExecutor>(); //Not really finished, will be though.
            Debug.Log(parallelJobs.Count);
            foreach (var item in parallelJobs)
            {
                if (item == null) continue;
                item.Execute();
                finished.Add(item);
                yield return new WaitForSeconds(0.01f);
            }

            PopulateJunctions();
            Debug.Log("Drawww");
            //int count = ((CommonEdgeJob)parallelJobs[0]).CountOfTrans();
            //CommonEdgeJob job = (CommonEdgeJob)parallelJobs[0];
            //job.left = new List<Edge>();
            //job.right = new List<Edge>();
            //JobHandle handle = job.Schedule(count, 1);
            //yield return new WaitUntil(() => handle.IsCompleted);
            for (int i = 0; i < finished.Count; i++)
            {
                CommonEdgeJob common = (CommonEdgeJob) finished[i];
                common.left.FindMyJunction(junctions);
                common.right.FindMyJunction(junctions);
                edges.Add(common.left);
                edges.Add(common.right);
                Debug.DrawRay(common.left.JunctionPosition() + new Vector3(0, 2, 0),
                    common.left.s[0] - common.left.JunctionPosition(), Color.magenta, 1000f);
                Debug.DrawRay(common.right.JunctionPosition() + new Vector3(0, 2, 0),
                    common.right.s[0] - common.right.JunctionPosition(), Color.magenta, 1000f);

                for (int j = 0; j < common.left.s.Count - 1; j++)
                {
                    Debug.DrawRay(common.left.s[j] + new Vector3(0, 2, 0), common.left.s[j + 1] - common.left.s[j],
                        Color.blue, 5f);
                }

                for (int j = 0; j < common.right.s.Count - 1; j++)
                {
                    Debug.DrawRay(common.right.s[j] + new Vector3(0, 2, 0), common.right.s[j + 1] - common.right.s[j],
                        Color.blue, 5f);
                }
            }

            ready = true;
        }

        public bool IsReady()
        {
            return ready;
        }

        private void PopulateJunctions()
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag("junction");

            foreach (var item in objs)
            {
                junctions.Add(new Junction(item.transform.position));
            }
        }
        //bool AvailableIndex()
        //{

        //}
        // Update is called once per frame
        void Update()
        {
        }
    }


    public class JobFactory
    {
        public const int COMMON_EDGE = 0;
        public const int PARKING_EDGE = 1;
        public const int ONEWAY_EDGE = 2;

        private const string NAME_EDGE = "NM_Asphalt_No_Lines_Advanced";
        public const string EDGE_DECAL = "Decal_Road_Border_left(Clone)";


        private Dictionary<int, List<Transform>> transformLists = new Dictionary<int, List<Transform>>();

        private int[] typeIndexes = new int[5];

        public JobFactory()
        {
            for (int i = 0; i < typeIndexes.Length; i++)
            {
                typeIndexes[i] = 0;
            }

            for (int i = 0; i < 5; i++)
            {
                transformLists.Add(i, new List<Transform>());
            }
        }


        public List<IExecutor> TransformsToJobs(List<Transform> transforms)
        {
            foreach (var transform in transforms)
            {
                transformLists[COMMON_EDGE].Add(transform);
                //if (transformLists.ContainsKey(TransformToType(transform)))
                //    transformLists[TransformToType(transform)].Add(transform);
            }

            List<IExecutor> parallelJobs = new List<IExecutor>();
            foreach (var item in transformLists)
            {
                int len = item.Value.Count;

                for (int i = 0; i < len; i++)
                {
                    parallelJobs.Add(GetProperJob(item.Key));
                }
            }

            return parallelJobs;
        }

        public IExecutor GetProperJob(int key)
        {
            switch (key)
            {
                case COMMON_EDGE:
                    CommonEdgeJob job = new CommonEdgeJob();
                    job.parentTrans = transformLists[key][0];
                    transformLists[key].RemoveAt(0);
                    return job;
                default: return null;
            }
        }

        private int TransformToType(Transform trans)
        {
            if (trans.name.Contains(NAME_EDGE))
            {
                int times = 0;
                foreach (Transform item in trans)
                {
                    if (item.name.Contains(EDGE_DECAL))
                        times++;
                }

                if (times == 3 || times == 4)
                    return COMMON_EDGE;
                else return ONEWAY_EDGE;
            }

            return -1;
        }
    }

    public abstract class IExecutor
    {
        public bool finished = false;
        public abstract void Execute();
    }

    public class CommonEdgeJob : IExecutor
    {
        public Transform parentTrans;
        public TmpEdhe left = new TmpEdhe();
        public TmpEdhe right = new TmpEdhe();

        List<Independent> independents = new List<Independent>();

        private static Vector3[] diastavrosis;

        private void InitDiastavrosis()
        {
            if (diastavrosis != null)
                return;
            GameObject[] dias = GameObject.FindGameObjectsWithTag("diastavrosi");
            diastavrosis = new Vector3[dias.Length];
            for (int i = 0; i < dias.Length; i++)
            {
                diastavrosis[i] = dias[i].transform.position;
            }
        }

        private float threshold = 8f;

        private int IndexOfNearestDiastavrosi(Vector3 point)
        {
            float minDistance = float.MaxValue;
            int index = -1;
            float tmp;
            for (int i = 0; i < diastavrosis.Length; i++)
            {
                if ((tmp = (point - diastavrosis[i]).magnitude) < threshold)
                {
                    if (tmp < minDistance)
                    {
                        minDistance = tmp;
                        index = i;
                    }
                }
            }

            return index;
        }

        public override void Execute()
        {
            InitDiastavrosis();
            Transform mTransform = parentTrans;
            TmpEdhe mLeft = left;
            TmpEdhe mRight = right;
            Transform neededIs = mTransform;
            Mesh mesh = parentTrans.GetComponent<MeshFilter>().mesh;
            Vector3 direction = Vector3.zero;
            Vector3 perpendic;
            bool exist = false;
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                foreach (var item in independents)
                {
                    if (item.vertices.ContainsKey(mesh.triangles[i]) ||
                        item.vertices.ContainsKey(mesh.triangles[i + 1]) ||
                        item.vertices.ContainsKey(mesh.triangles[i + 2]))
                    {
                        exist = true;
                        if (!item.vertices.ContainsKey(mesh.triangles[i]))
                        {
                            item.vertices.Add(mesh.triangles[i], mesh.vertices[mesh.triangles[i]]);
                            item.ordered.Add(mesh.vertices[mesh.triangles[i]]);
                        }

                        if (!item.vertices.ContainsKey(mesh.triangles[i + 1]))
                        {
                            item.vertices.Add(mesh.triangles[i + 1], mesh.vertices[mesh.triangles[i + 1]]);
                            item.ordered.Add(mesh.vertices[mesh.triangles[i + 1]]);
                        }

                        if (!item.vertices.ContainsKey(mesh.triangles[i + 2]))
                        {
                            item.vertices.Add(mesh.triangles[i + 2], mesh.vertices[mesh.triangles[i + 2]]);
                            item.ordered.Add(mesh.vertices[mesh.triangles[i + 2]]);
                        }

                        break;
                    }
                }

                if (exist == false)
                {
                    Independent item = new Independent();
                    independents.Add(item);
                    item.vertices.Add(mesh.triangles[i], mesh.vertices[mesh.triangles[i]]);
                    item.vertices.Add(mesh.triangles[i + 1], mesh.vertices[mesh.triangles[i + 1]]);
                    item.vertices.Add(mesh.triangles[i + 2], mesh.vertices[mesh.triangles[i + 2]]);
                    item.ordered.Add(mesh.vertices[mesh.triangles[i]]);
                    item.ordered.Add(mesh.vertices[mesh.triangles[i + 1]]);
                    item.ordered.Add(mesh.vertices[mesh.triangles[i + 2]]);
                }

                exist = false;
            }

            Vector3 prevPoint = independents[0].ordered[0];
            foreach (var item in independents)
            {
                Debug.Log(item.ordered.Count);
                if (item.ordered.Count != 6)
                    continue;
                FirstOrLastIndependentSpecial(item);
                direction = item.ordered[4] - item.ordered[0];
                Debug.DrawRay(item.ordered[0] + new Vector3(0, 1, 0), direction, Color.cyan, 10);
                perpendic = Perpendicular(direction);

                if ((prevPoint - item.ordered[0]).magnitude > 20)
                {
                    AddSpecial(item.ordered[0]);
                    prevPoint = item.ordered[0];
                }
                else
                {
                    mLeft.s.Add(item.ordered[0] + perpendic.normalized * 2);
                    mRight.s.Add(item.ordered[0] - perpendic.normalized * 2);
                    prevPoint = item.ordered[0];
                }
            }

            mRight.s.Reverse();
            //for ( int i = 0; i < mesh.vertices.Length-6; i += 6 )
            //{
            //    direction = mesh.vertices[i + 4] - mesh.vertices[i];
            //    Debug.DrawRay(mesh.vertices[i] + new Vector3(0, 1, 0), direction, Color.cyan,10);
            //    perpendic = Perpendicular(direction);
            //    mLeft.s.Add(mesh.vertices[i] + perpendic.normalized * 3);
            //    mRight.s.Add(mesh.vertices[i] - perpendic.normalized * 3);
            //}
        }

        private int GetIndependentsLastIndex()
        {
            for (int i = independents.Count - 1; i >= 0; i--)
            {
                if (independents[i].ordered.Count == 6)
                    return i;
            }

            return independents.Count;
        }

        private void FirstOrLastIndependentSpecial(Independent item)
        {
            int index = -1;
            if (independents[0] == item)
            {
                index = IndexOfNearestDiastavrosi(item.ordered[0]);
            }
            else if (independents[independents.Count - 1] == item)
            {
                index = IndexOfNearestDiastavrosi(item.ordered[4]);
            }
            else return;

            if (index == -1)
                return;
            if (independents[0] == item)
            {
                Vector3 dir = diastavrosis[index] - item.ordered[0];
                Vector3 dir2 = item.ordered[4] - item.ordered[0];
                if (Vector3.Dot(dir.normalized, dir2.normalized) > 0)
                    return;
                item.ordered[0] = diastavrosis[index];
            }
            else
            {
                Vector3 dir = diastavrosis[index] - item.ordered[4];
                Vector3 dir2 = item.ordered[4] - item.ordered[0];
                dir.y = 0;
                dir2.y = 0;
                if (Vector3.Dot(dir.normalized, dir2.normalized) < 0)
                    return;
                item.ordered[4] = diastavrosis[index] + dir;
                item.ordered[0] = diastavrosis[index];
            }
        }

        private void AddSpecial(Vector3 prev)
        {
            TmpEdhe mLeft = left;
            TmpEdhe mRight = right;
            List<Vector3> fixes = new List<Vector3>();
            string nam = "fix0 (";
            string name = "";
            for (int i = 0; i < 9; i++)
            {
                //fix0 (0)
                name = nam + "" + i + ")";
                fixes.Add(GameObject.Find(name).transform.position);
            }

            fixes.Reverse();
            //fixes.Add(prev);
            Vector3 dir;
            Vector3 perpendic = Vector3.zero;
            //mLeft.s.RemoveAt(mLeft.s.Count - 1); mRight.s.RemoveAt(mRight.s.Count - 1);

            for (int i = 0; i < fixes.Count - 1; i++)
            {
                dir = fixes[i + 1] - fixes[i];
                Debug.DrawRay(fixes[i], dir, Color.cyan, 5f);
                perpendic = Perpendicular(dir);
                mLeft.s.Add(fixes[i + 1] + perpendic.normalized * 2);
                mRight.s.Add(fixes[i + 1] - perpendic.normalized * 2);
            }
        }


        private Vector3 Perpendicular(Vector3 toGet)
        {
            Vector2 dir = new Vector2(toGet.x, toGet.z);
            return new Vector3(Vector2.Perpendicular(dir).x, 0, Vector2.Perpendicular(dir).y);
        }
    }

    public class Independent
    {
        public Dictionary<int, Vector3> vertices = new Dictionary<int, Vector3>();

        public List<Vector3> ordered = new List<Vector3>();
    }


//public class SingleEdgeJob : IJobParallelFor
//{

//    public Edge edge;
//    public void Execute(int index)
//    {
//        throw new System.NotImplementedException();
//    }
//}

    public class Junction
    {
        public List<TmpEdhe> inEdges = new List<TmpEdhe>();
        public List<TmpEdhe> outEdges = new List<TmpEdhe>();
        public string id_Same_With_Node = "";
        public Vector3 position;

        public Junction(Vector3 pss)
        {
            position = pss;
        }
    }


    public class TmpEdhe
    {
        public List<Vector3> s = new List<Vector3>();
        public bool finished = false;

        public Junction mJunction;
        //private bool initAsLeft = false;

        public Junction Start;
        public Junction End;

        public Vector3 JunctionPosition()
        {
            return mJunction.position;
        }

        public void FindMyJunction(List<Junction> junctions)
        {
            bool mIsIn = false;
            float leastDistance = float.MaxValue;
            int index = -1;
            for (int i = 0; i < junctions.Count; i++)
            {
                if (leastDistance > (junctions[i].position - s[0]).magnitude)
                {
                    leastDistance = (junctions[i].position - s[0]).magnitude;
                    index = i;
                }
            }

            mJunction = junctions[index];
            if (Intersection.CheckIfLeft(mJunction.position, s[1], s[0]))
            {
                mJunction.inEdges.Add(this);
                End = mJunction;
                mIsIn = true;
            }
            else
            {
                mJunction.outEdges.Add(this);
                Start = mJunction;
                mIsIn = false;
            }

            leastDistance = float.MaxValue;
            index = -1;
            for (int i = 0; i < junctions.Count; i++)
            {
                if (leastDistance > (junctions[i].position - s[s.Count - 1]).magnitude)
                {
                    leastDistance = (junctions[i].position - s[s.Count - 1]).magnitude;
                    index = i;
                }
            }

            if (mIsIn)
            {
                junctions[index].outEdges.Add(this);
                Start = junctions[index];
            }
            else
            {
                End = junctions[index];
                junctions[index].inEdges.Add(this);
            }

            //junctions.RemoveAt(index);
        }
    }
}
